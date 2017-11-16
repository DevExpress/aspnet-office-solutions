using DevExpress.Web.OfficeAzureCommunication.Utils;

namespace DevExpress.Web.OfficeAzureCommunication {
    public static class RoleEnvironmentConfig {
        public static string DocumentServerRoleName {
            get { return ServiceConfigUtils.GetAppSetting(ConfigurationKeys.DocumentServerRoleName); }
        }
        public static string RoutingRoleName {
            get { return ServiceConfigUtils.GetAppSetting(ConfigurationKeys.RoutingServerRoleName); }
        }
        public static int DocumentServerPort {
            get {
                return int.Parse(ServiceConfigUtils.GetAppSetting(ConfigurationKeys.DocumentServerPort));
            }
        }
    }
}
