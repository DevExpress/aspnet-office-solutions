using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Web.OfficeAzureCommunication;
using DevExpress.Web.OfficeAzureCommunication.Utils;
using Microsoft.Web.Administration;

namespace DocumentSite {

    public static class WebRoleConfiguration {
        public static void OnStart() {
            SetUpAppInitializationModule();
            SetUpAppPoolToEnable32BitApplications();
            InterRoleCommunicator.Initialize(true);
            InterRoleCommunicator.RoleInstanceNumberChanged += OnRoleInstanceNumberChanged;
            InitWebRoleState();
        }

        private static void OnRoleInstanceNumberChanged(int number) {
            if(number < 0) {
                List<WorkSessionServerInfo> affectedServers = new List<WorkSessionServerInfo>();
                RoutingTable.ForEachWorkSessionServer((roleInstanceId, serverInfo) => {
                    if(serverInfo.IsProbablyShuttingDown())
                        affectedServers.Add(serverInfo);
                });
                WorkSessionMessenger.SendMessage(MessageOperation.ServerNumberDecreased, affectedServers);
            }
        }

        public static void OnStop() {
            SendShutdownNotification();
        }
        static void SetUpAppInitializationModule() {
            using(var serverManager = new ServerManager()) {
                foreach(var application in serverManager.Sites.SelectMany(c => c.Applications)) {
                    application["preloadEnabled"] = true;
                }

                foreach(var appPool in serverManager.ApplicationPools) {
                    appPool.AutoStart = true;
                    appPool["startMode"] = "AlwaysRunning";
                    appPool.ProcessModel.IdleTimeout = TimeSpan.Zero;
                    appPool.Recycling.PeriodicRestart.Time = TimeSpan.Zero;
                }

                serverManager.CommitChanges();
            }
        }
        static void SetUpAppPoolToEnable32BitApplications() {
            string command = @"call ./Setup/SetUpAppPoolsToEnable32BitApplications.cmd";
            CommandLineUtils.CmdExecute(command);
        }
        static bool IsRoutingRoleRunning() {
            return RoleInstanceUtils.GetRoleInstanceAdressList(RoleEnvironmentConfig.RoutingRoleName).Count() > 0;
        }
        static void InitWebRoleState() {
            NotifyRoutingServer();
        }
        static void NotifyRoutingServer() {
            var serverStateTask = Task.Run(async () => {
                while(!IsRoutingRoleRunning())
                    await Task.Delay(1000);
                WorkSessionMessenger.SendMessage(MessageOperation.RegisterServer, new List<WorkSessionServerInfo>());
            });
        }
        static void SendShutdownNotification() {
            WorkSessionMessenger.SendMessage(MessageOperation.UnregisterServer, new List<WorkSessionServerInfo>());
        }
    }

}
