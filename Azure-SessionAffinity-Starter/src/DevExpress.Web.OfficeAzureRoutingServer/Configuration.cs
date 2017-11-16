using System.Collections.Generic;
using DevExpress.Web.OfficeAzureCommunication;
using DevExpress.Web.OfficeAzureCommunication.Utils;

namespace DevExpress.Web.OfficeAzureRoutingServer {
    public static class WebFarmConfiguration
    {
        private const string defaultFarmName = "ARRAffinity";
        
        public static string FarmName { get { return defaultFarmName; } }

        public static Dictionary<string, string> HealthCheckConfiguration { get {
                Dictionary<string, string> configuration= new Dictionary<string, string>();
                configuration.Add("url", string.Format("http://localhost:{0}/{1}", RoleEnvironmentConfig.DocumentServerPort,
                    ServiceConfigUtils.GetAppSetting(ConfigurationKeys.HealthCheckPageUrl)));
                configuration.Add("responseMatch", ServiceConfigUtils.GetAppSetting(ConfigurationKeys.HealthCheckResponseMatch));
                return configuration;
            }
        }
    }

    public static class RoutingConfiguration {
        private const string defaultAffinityCookieName = "ARRAffinity";
        private const string queryStringParameterName = "dxwsid";
        private const string requestParamKeyName = "ASPxOfficeWorkSessionID";
        
        public static string AffinityCookieName { get { return defaultAffinityCookieName; } }
        public static string QueryStringParameterName { get { return queryStringParameterName; } }
        public static string RequestParamKeyName { get { return requestParamKeyName; } }
        public static bool UseCookie { get { return true; } }

    }
}