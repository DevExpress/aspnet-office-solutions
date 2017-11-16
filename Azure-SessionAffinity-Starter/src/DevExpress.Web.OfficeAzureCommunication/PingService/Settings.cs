using DevExpress.Web.OfficeAzureCommunication.Utils;

namespace DevExpress.Web.OfficeAzureCommunication {
    public class TimeoutServiceSettings {
        protected const int DefaultPingInterval = 30;
        protected const int DefaultServerExpirationInterval = 90;

        public virtual int BroadcastInterval { get; private set; }
        public virtual int ServerStatusExpirationInterval { get; private set; }

        protected TimeoutServiceSettings() { }
        public TimeoutServiceSettings(int broadcastInterval, int timeoutInterval) {
            BroadcastInterval = broadcastInterval;
            ServerStatusExpirationInterval = timeoutInterval;
        }
    }

    public class TimeoutServiceSettingsFromConfiguration : TimeoutServiceSettings {
        public override int BroadcastInterval { get { return GetTimerInterval(ConfigurationKeys.ServerStatusBroadcastInterval, DefaultPingInterval); } }
        public override int ServerStatusExpirationInterval { get { return GetTimerInterval(ConfigurationKeys.ServerStatusExpirationInterval, DefaultServerExpirationInterval); } }

        public TimeoutServiceSettingsFromConfiguration() { }

        int GetTimerInterval(string settingName, int defaultValue) {
            string intervalValueFromConfig = ServiceConfigUtils.GetAppSetting(settingName, defaultValue.ToString());
            if(int.TryParse(intervalValueFromConfig, out int intervalValue))
                return intervalValue;
            return -1;
        }
    }

    
}