using System;
using System.Collections.Generic;
using System.Linq;

namespace DevExpress.Web.OfficeAzureCommunication {
    public static class MessageDispatcher {
        public static void ProcessMessage(Message msg) {
            #if DEBUG
            DevExpress.Web.OfficeAzureCommunication.Diagnostic.Logger.Log(msg);
            #endif

            PreprocessMessage(msg);
            switch(msg.MessageOperation) {
                case MessageOperation.Ping:
                    Ping(msg);
                    break;
                case MessageOperation.AddWorkSession:
                case MessageOperation.WakeUpWorkSession:
                    Add(msg);
                    break;
                case MessageOperation.RemoveWorkSession:
                case MessageOperation.HibernateWorkSession:
                    Remove(msg);
                    break;
                case MessageOperation.AutoSaveWorkSession:
                    ChangeStatus(msg);
                    break;
                case MessageOperation.UnregisterServer:
                    UnregisterServer(msg);
                    break;
                case MessageOperation.ServerNumberDecreased:
                    ServerNumberDecreased(msg);
                    break;
            }
        }

        private static void ServerNumberDecreased(Message msg) {
            List<WorkSessionServerInfo> affectedServers = msg.RegisteredServers.Where(s => s.RoleInstanceId != msg.Sender.RoleInstanceId).ToList();
            RoutingTable.RaiseServerNumberDecreased(affectedServers);
        }

        static bool NeedToUpdateServerStateFromMessage(Message msg) {
            return msg.MessageOperation != MessageOperation.RemoveWorkSession && 
                msg.MessageOperation != MessageOperation.HibernateWorkSession && 
                msg.MessageOperation != MessageOperation.UnregisterServer;
        }
        static void PreprocessMessage(Message msg) {
            if(NeedToUpdateServerStateFromMessage(msg))
                GetServerStateFromMessage(msg);
            SetWorkSessionProperties(msg);
        }

        static void SetWorkSessionProperties(Message msg) {
            foreach(var w in msg.Sender.WorkSessions) {
                w.Value.CreateTime = msg.CreateTime;
                w.Value.ProcessedTime = DateTime.Now;
            }
        }

        static void GetServerStateFromMessage(Message msg) {
            RoutingTable.GetWorkSessionServerStateFromMessage(msg);
        }
        private static void UnregisterServer(Message msg) {
            RoutingTable.RemoveWorkSessionServer(msg.Sender);
        }
        static void Ping(Message msg) {
            TimeoutServiceSettingsFromConfiguration pingSettings = new TimeoutServiceSettingsFromConfiguration();
            RoutingTable.CheckForOutdatedServers(TimeSpan.FromSeconds(pingSettings.ServerStatusExpirationInterval));
            RoutingTable.UpdateAllWorkSessionsFromOneServer(msg);
        }
        static void Add(Message msg) {
            RoutingTable.AddWorkSession(msg);
        }
        static void Remove(Message msg) {
            RoutingTable.RemoveWorkSession(msg);
        }
        static void ChangeStatus(Message msg) {
            RoutingTable.ChangeWorkSessionStatus(msg);
        }
    }
}
