using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using DevExpress.Web.OfficeAzureCommunication;

namespace DevExpress.Web.OfficeAzureRoutingServer {

    public abstract partial class RoutingModuleBase : IHttpModule {

        public void Init(HttpApplication application) {
            application.BeginRequest += new EventHandler(Application_BeginRequest);
        }

        protected void Application_BeginRequest(object sender, EventArgs e) {
            HttpApplication application = (HttpApplication)sender;
            string workSessionID = ExtractRoutingKeyFromRequest(application.Request);

            bool handled = CustomProcessing(application);
            if(handled) return;
            DoRequestRouting(application, workSessionID);
        }
        
        protected abstract string ExtractRoutingKeyFromRequest(HttpRequest request);

        public void Dispose() { }

        static void DoRequestRouting(HttpApplication application, string workSessionID) {
            RoutingTable.EnsureServerIsPrepared();

            WorkSessionServerInfo serverInfo = RoutingTable.GetWorkSessionServerInfoByWorkSessionID(workSessionID);
            if(serverInfo != null && serverInfo.Status != WorkSessionServerStatus.Online) {
                throw new Exception("WorkSession is not available");
            }

            if(serverInfo == null && !HeaderCookieHelper.HasServerAffinity(application, RoutingConfiguration.AffinityCookieName))
                serverInfo = RoutingTable.FindAvailableServerForNewWorkSession();

            if(serverInfo != null && RoutingConfiguration.UseCookie) {
                FarmConfigurationManager.AddServer(serverInfo.HostServerIP, 8080);
                string newCookieValue = ARRHashHelper.CalculateHash(serverInfo.HostServerIP);
                HeaderCookieHelper.PatchHeaderCookieGUID(application, RoutingConfiguration.AffinityCookieName, newCookieValue);
            }
        }

        static List<IRoutingModuleCustomHandler> customHandlers = new List<IRoutingModuleCustomHandler>();
        internal static void RegisterCustomHandler(IRoutingModuleCustomHandler customHandler) {
            if(!customHandlers.Contains(customHandler))
                customHandlers.Add(customHandler);
        }
        static bool CustomProcessing(HttpApplication application){
            bool handled = false;
            foreach(var customHandler in customHandlers) {
                handled = customHandler.PreccessRequest(application);
                if(handled) break;
            }
            return handled;
        }
    }

}