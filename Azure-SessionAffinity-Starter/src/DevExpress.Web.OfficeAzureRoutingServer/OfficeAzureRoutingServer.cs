using DevExpress.Web.OfficeAzureCommunication;

namespace DevExpress.Web.OfficeAzureRoutingServer {
    public static class OfficeAzureRoutingServer {
        public static void Init() {
            InterRoleCommunicator.Initialize(false);
        }
    }
}
