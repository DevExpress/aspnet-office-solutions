using System;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Linq;
using System.Collections.ObjectModel;

namespace DevExpress.Web.OfficeAzureCommunication {
    public delegate void RoleInstanceNumberChangedEventHandler(int number);

    public partial class InterRoleCommunicator {
        static object syncRoot = new Object();
        static volatile InterRoleCommunicator Instance;

        public static void Initialize(bool webRoleContext) {
            SetUp();
            if(!webRoleContext)
                InitPingService();
            else
                SubscribeRoleEnvironmentEvents();
            
        }
        static void SetUp() {
            SetUp(new ServiceBusSettingsFromConfiguration());
        }

        static void SetUp(ServiceBusSettings serviceBusSettings) {
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

        static void SubscribeRoleEnvironmentEvents() {
            RoleEnvironment.Changing += RoleEnvironment_Changing;
            RoleEnvironment.Changed += RoleEnvironment_Changed;
        }

        static int instanceCount;
        static event RoleInstanceNumberChangedEventHandler RoleInstanceNumberChangedCore;
        public static event RoleInstanceNumberChangedEventHandler RoleInstanceNumberChanged {
            add {
                if(!value.Method.IsStatic)
                    throw new NotSupportedException("Only Static event handlers are allowed for the ServerEnvironmentChanged event");
                if(RoleInstanceNumberChangedCore == null || !RoleInstanceNumberChangedCore.GetInvocationList().Contains(value))
                    RoleInstanceNumberChangedCore += value;
            }
            remove {
                RoleInstanceNumberChangedCore -= value;
            }
        }
        static void RoleEnvironment_Changed(object sender, RoleEnvironmentChangedEventArgs e) {
            if(!ChangesInCurrentRole(e.Changes))
                return;
            int serverCountDifference = RoleEnvironment.CurrentRoleInstance.Role.Instances.Count - instanceCount;
            if(serverCountDifference < 0)
                RaiseServerNumberChanged(serverCountDifference);
        }

        static void RoleEnvironment_Changing(object sender, RoleEnvironmentChangingEventArgs e) {
            if(!ChangesInCurrentRole(e.Changes))
                return;
            instanceCount = RoleEnvironment.CurrentRoleInstance.Role.Instances.Count;
        }

        static bool ChangesInCurrentRole(ReadOnlyCollection<RoleEnvironmentChange> changes) {
            var topologyChangesInCurrentRole = from ch in changes.OfType<RoleEnvironmentTopologyChange>()
                          where ch.RoleName == RoleEnvironment.CurrentRoleInstance.Role.Name
                          select ch;
            return topologyChangesInCurrentRole.Any();
        }

        static void RaiseServerNumberChanged(int number) {
            if(RoleInstanceNumberChangedCore != null)
                RoleInstanceNumberChangedCore(number);
        }
    }

}