using System;
using System.Text;
using System.Web;

namespace DevExpress.Web.OfficeAzureRoutingServer {

    interface IRoutingModuleCustomHandler {
        bool PreccessRequest(HttpApplication application);
    }

#if DEBUG
    partial class RoutingModuleBase : IHttpModule {
        static RoutingModuleBase() {
            RegisterCustomHandler(new RoutingModule_DiagnosticPage());
        }
    }

    class RoutingModule_DiagnosticPage : IRoutingModuleCustomHandler {
        public bool PreccessRequest(HttpApplication application) {
            bool handled = false;
            HttpContext context = application.Context;
            bool requestToProcess = context.Request.FilePath.Contains("diagnostic.aspx");

            if(requestToProcess) {
                string content = DevExpress.Web.OfficeAzureCommunication.Diagnostic.WorkSessionServerView.CreateDianosticPageHtml(System.Environment.MachineName, address => ARRHashHelper.CalculateHash(address));
                context.Response.Write(content);
                context.Response.End();
                handled = true;
            }
            return handled;
        }

    }
#endif
}
