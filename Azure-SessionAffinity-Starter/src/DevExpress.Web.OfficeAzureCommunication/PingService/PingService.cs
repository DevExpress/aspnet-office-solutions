using System;

namespace DevExpress.Web.OfficeAzureCommunication {

    class PingService : TimeoutService {
        static object syncRoot = new Object();
        TimeoutServiceSettings settings = new TimeoutServiceSettingsFromConfiguration();

        private PingService() { }

        private static volatile PingService instance;
        protected static PingService Singleton {
            get {
                if(instance == null) {
                    lock(syncRoot) {
                        if(instance == null) {
                            instance = new PingService();
                        }
                    }
                }

                return instance;
            }
        }

        protected override TimeoutServiceSettings Settings { get { return settings; }  }

        protected void InitSettings(TimeoutServiceSettings settings) {
            this.settings = settings;
        }

        protected override void OnServiceCore() {
            var workSessionInfos = RoutingTable.GetWorkSessionServerInstances();
            WorkSessionMessenger.SendMessage(MessageOperation.Ping, workSessionInfos);
        }

        public static void Start(TimeoutServiceSettings settings) {
            if(settings.BroadcastInterval < 0)
                return;
            Singleton.InitSettings(settings);
            Singleton.StartInternal();
        }
        public static void Stop() {
            Singleton.StopInternal();
        }

    }
}
