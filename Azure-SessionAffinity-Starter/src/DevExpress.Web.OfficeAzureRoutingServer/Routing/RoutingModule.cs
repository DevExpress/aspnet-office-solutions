using System.Web;

namespace DevExpress.Web.OfficeAzureRoutingServer {

    public class RoutingModule : RoutingModuleBase {
        protected override string ExtractRoutingKeyFromRequest(HttpRequest request) {
            return RoutingKeyHelper.ExtractRoutingKeyFromRequest(request);
        }
    }

    public class RoutingKeyHelper {
        public static string ExtractRoutingKeyFromRequest(HttpRequest request) {
            string routingKey = string.Empty;
            if(!string.IsNullOrEmpty(request.QueryString[RoutingConfiguration.QueryStringParameterName])) {
                routingKey = request.QueryString[RoutingConfiguration.QueryStringParameterName];
            } else if(!string.IsNullOrEmpty(request.Params[RoutingConfiguration.RequestParamKeyName])) {
                routingKey = request.Params[RoutingConfiguration.RequestParamKeyName];
            }
            return routingKey;
        }
    }

}
