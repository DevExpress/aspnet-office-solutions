using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace DevExpress.Web.OfficeAzureCommunication {

    public enum WorkSessionStatus { Loaded, AutoSaved, Hibernated, WokenUp, Removed }

    public enum WorkSessionServerStatus { Online, PingTimeout, ShuttingDown }

    public enum MessageOperation { Ping, RegisterServer, UnregisterServer, ShutDownServer, AddWorkSession, RemoveWorkSession, WakeUpWorkSession, HibernateWorkSession, AutoSaveWorkSession }

    [DataContract(Namespace = BroadcastNamespaces.DataContractNamespace)]
    public class Message {
        [DataMember]
        public string RoleInstanceId { get; private set; }
        [DataMember]
        public MessageOperation MessageOperation { get; protected set; }
        [DataMember]
        public List<WorkSessionServerInfo> RegisteredServers { get; private set; }
        #if DEBUG
        [DataMember]
        public string DiagnosticMessage { get; set; }
        #endif
        [DataMember]
        public DateTime CreateTime { get; private set; }

        public WorkSessionServerInfo Sender {
            get { return RegisteredServers.FirstOrDefault(s => s.RoleInstanceId == RoleInstanceId); }
        }
        public string HostServerName { get { return Sender.HostServerName; } }
        public string HostServerIP { get { return Sender.HostServerIP; } }


        public Message(string roleInstanceId, string hostServerName, string hostServerIP, MessageOperation messageOperation, List<WorkSessionServerInfo> registeredServers) {            
            RoleInstanceId = roleInstanceId;
            MessageOperation = messageOperation;
            AppendServers(registeredServers);
            SetSenderInfo(new WorkSessionServerInfo(roleInstanceId, hostServerName, hostServerIP, RoutingTable.GetSelfStatus()));
            CreateTime = DateTime.Now;
        }

        void AppendServers(List<WorkSessionServerInfo> registeredServers) {
            RegisteredServers = new List<WorkSessionServerInfo>();
            RegisteredServers.AddRange(registeredServers);
        }

        void SetSenderInfo(WorkSessionServerInfo sender) {
            if(RegisteredServers.Count == 0 || Sender.RoleInstanceId != RoleInstanceId)     
                RegisteredServers.Add(sender);
            Sender.GetServerParameters();
        }

        public bool HasWorkSessionInfo() {
            return RegisteredServers.Select(s => s.WorkSessions.Count()).Sum() > 0;
        }
    }
}