using System;

namespace DevExpress.Web.OfficeAzureCommunication {

    public partial class InterRoleCommunicator {
        static object syncRoot = new Object();
        static volatile InterRoleCommunicator Instance;

        public static void Initialize() {
            SetUp();
            InitPingService();
        }
        public static void SetUp() {
            SetUp(new ServiceBusSettingsFromConfiguration());
        }

        public static void SetUp(ServiceBusSettings serviceBusSettings) {
            if(Instance == null) {
                lock(syncRoot) {
                    if(Instance == null) {
                        Instance = new InterRoleCommunicator(serviceBusSettings);
                    }
                }
            }
        }

        public static void ShutDown() {
            ShutDownPingService();
        }

        static void InitPingService() {
            InitPingService(new TimeoutServiceSettingsFromConfiguration());
        }

        static void InitPingService(TimeoutServiceSettings pingServiceSettings) {
            PingService.Start(pingServiceSettings);
        }

        static void ShutDownPingService() {
            PingService.Stop();
        }

        InterRoleCommunicator(ServiceBusSettings serviceBusSettings) {
            ServiceBusSettings = serviceBusSettings;
            Subscribe();
        }

        ServiceBusPublisher<IMessageServiceChannel> publisher;
        ServiceBusSubscriber<MessageService> subscriber;

        ServiceBusSettings ServiceBusSettings { get; set; }
       
        ServiceBusPublisher<IMessageServiceChannel> Publisher {
            get {
                if(publisher == null)
                    publisher = new ServiceBusPublisher<IMessageServiceChannel>(ServiceBusSettings);
                return publisher;
            }
        }

        internal static void SendMessage(Message msg) {
            lock(syncRoot) {
                if(Instance == null)
                    throw new InvalidOperationException("Call the OfficeAzureDocumentServer.Init(...) static method on Application_Start of Document Server's Global.asax");
                Instance.Publish(msg);
            }
        }
        
        void Publish(Message msg) {
            Publisher.Channel.Publish(msg);
        }
        void Subscribe() {
            if(subscriber == null)
                subscriber = new ServiceBusSubscriber<MessageService>(ServiceBusSettings);
        }
    }

}