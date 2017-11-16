using System;
using DevExpress.Web.OfficeAzureRoutingServer;

namespace Routing {
    public class Global : System.Web.HttpApplication {

        protected void Application_Start(object sender, EventArgs e) {
            OfficeAzureRoutingServer.Init();
        }
    }
}