using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.Web.OfficeAzureCommunication;
using DevExpress.Web.OfficeAzureCommunication.Utils;
using DevExpress.Web.OfficeAzureRoutingServer;
using Microsoft.Web.Administration;

namespace Routing {
    public static class WebRoleConfiguration {
        static int FarmConfigurationUpdateInterval { get { return int.Parse(ServiceConfigUtils.GetAppSetting(ConfigurationKeys.FarmConfigurationUpdateInterval)); } }

        public static void OnStart(string documentServerRoleName, int documentServerPort) {
            ConfigureIIS();
            InterRoleCommunicator.SetUp();
            InitWebRoleState();
            SubscribeServerEvents();
        }

        public static void OnStop() {
            PrepareForShutDown();
        }

       static void ConfigureIIS() {
            SetUpAppInitializationModule();
            SetUpAlwaysRunning();
            SetUpFarmDelayed();
        }

        static void SetUpAppInitializationModule() {
            using(var serverManager = new ServerManager()) {
                foreach(var application in serverManager.Sites.SelectMany(c => c.Applications)) {
                    application["preloadEnabled"] = true;
                }

                foreach(var appPool in serverManager.ApplicationPools) {
                    appPool["startMode"] = "AlwaysRunning";
                }

                serverManager.CommitChanges();
            }
        }

        // IS may not be fully configured during the startup task stage in the startup process, 
        // so role-specific data may not be available. 
        // Startup tasks that require role-specific data should use 
        // Microsoft.WindowsAzure.ServiceRuntime.RoleEntryPoint.OnStart.
        // https://azure.microsoft.com/en-us/documentation/articles/cloud-services-startup-tasks/
        static void SetUpAlwaysRunning() {
            string command = @"call ./Setup/SetUpAppPoolsAlwaysRunning.cmd";
            CommandLineUtils.CmdExecute(command);
        }

        static void SetUpFarmDelayed() {
            int SetUpFarmDelay = 10000;
            Task.Delay(SetUpFarmDelay).ContinueWith(_ => {
                SetUpFarm();
            });
        }

        static void SetUpFarm() {
            try {            
                FarmConfigurationManager.SetUpFarm();
            } catch(Exception) {  }
        }

        static void InitWebRoleState() {
            NotifyServers(MessageOperation.RegisterServer);
        }

        static void SubscribeServerEvents() {
            RoutingTable.ServerAdded += OnDocumentServerAdded;
            RoutingTable.ServerRemoved += OnDocumentServerRemoved;
        }

        static void OnDocumentServerRemoved(WorkSessionServerInfo server) {
            if(server.IsDocumentServer()) {
                SendHibernateRequest(server);
                WebRoleConfiguration.UnregisterServer(server.HostServerIP);
            }
        }

        static void SendHibernateRequest(WorkSessionServerInfo server) {
            string shutDownPageUrl = GetShutdownRequestUrl(server.HostServerIP);
            Trace.TraceWarning("Send shutdown request to " + shutDownPageUrl + ". Server " + Environment.MachineName);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(shutDownPageUrl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            response.Close();
        }

        static string GetShutdownRequestUrl(string serverAddress) {
            return string.Format("http://{0}:{1}/Default.aspx?ShutDown=true", serverAddress, RoleEnvironmentConfig.DocumentServerPort);
        }
        static void OnDocumentServerAdded(WorkSessionServerInfo server) {
            if(server.IsDocumentServer())
                WebRoleConfiguration.RegisterServer(server.HostServerIP);
        }

        static void PrepareForShutDown() {
            RoutingTable.SetSelfState(WorkSessionServerStatus.ShuttingDown);
            NotifyServers(MessageOperation.UnregisterServer);
        }

        static void NotifyServers(MessageOperation operation) {
            WorkSessionMessenger.SendMessage(operation, new List<WorkSessionServerInfo>());
        }

        public static void TrackServerChanges() {
            while(true) {
                IEnumerable<string> documentServers = RoutingTable.GetServerAdressesByRoleName(RoleEnvironmentConfig.DocumentServerRoleName);
                FarmConfigurationManager.UpdateServersRegistration(documentServers, RoleEnvironmentConfig.DocumentServerPort);
                Thread.Sleep(FarmConfigurationUpdateInterval);
            }
        }

        public static void RegisterServer(string serverAddress) {
            FarmConfigurationManager.RegisterServer(serverAddress, RoleEnvironmentConfig.DocumentServerPort);
        }

        public static void UnregisterServer(string serverAddress) {
            FarmConfigurationManager.RemoveServers(new List<string>() { serverAddress });
        }
    }
}
