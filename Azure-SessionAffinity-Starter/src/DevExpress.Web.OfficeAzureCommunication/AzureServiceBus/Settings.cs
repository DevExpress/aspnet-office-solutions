using DevExpress.Web.OfficeAzureCommunication.Utils;

namespace DevExpress.Web.OfficeAzureCommunication {
    public class ServiceBusSettings {
        public virtual string ServiceBusURISchema { get; private set; }
        public virtual string ServiceNamespace { get; private set; }
        public virtual string ServicePath { get; private set; }
        public virtual string SharedAccessKeyName { get; private set; }
        public virtual string SharedAccessKey { get; private set; }

        public ServiceBusSettings(string serviceNamespace, 
                                    string servicePath,
                                    string sharedAccessKeyName, 
                                    string sharedAccessKey) 
            : this("sb", serviceNamespace, servicePath, sharedAccessKeyName, sharedAccessKey) {
        }
        public ServiceBusSettings(string serviceBusURISchema, 
                                    string serviceNamespace, 
                                    string servicePath,
                                    string sharedAccessKeyName, 
                                    string sharedAccessKey) {
            ServiceBusURISchema = serviceBusURISchema;
            ServiceNamespace = serviceNamespace;
            ServicePath = servicePath;
            SharedAccessKeyName = sharedAccessKeyName;
            SharedAccessKey = sharedAccessKey;
        }
        protected ServiceBusSettings ()	{ }
    }

    public class ServiceBusSettingsFromConfiguration : ServiceBusSettings {
        public override string ServiceBusURISchema { get  { return ServiceConfigUtils.GetAppSetting("ServiceBusURISchema", "sb"); }  }
        public override string ServiceNamespace    { get  { return ServiceConfigUtils.GetAppSetting("ServiceBusNamespace"); }  }
        public override string ServicePath         { get  { return ServiceConfigUtils.GetAppSetting("ServiceBusPath"); }  }
        public override string SharedAccessKeyName { get  { return ServiceConfigUtils.GetAppSetting("ServiceBusSharedAccessKeyName"); }  }
        public override string SharedAccessKey     { get  { return ServiceConfigUtils.GetAppSetting("ServiceBusSharedAccessKey"); }  }

        public ServiceBusSettingsFromConfiguration() { }
    }

    public static class BroadcastNamespaces {
        public const string DataContractNamespace = "http://DataContractNamespace";
        public const string ServiceContractNamespace = "http://ServiceContractNamespace";
    }
}