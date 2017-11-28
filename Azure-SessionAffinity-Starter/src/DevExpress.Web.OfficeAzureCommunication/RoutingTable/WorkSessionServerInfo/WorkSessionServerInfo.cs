using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using DevExpress.Web.OfficeAzureCommunication.Utils;

namespace DevExpress.Web.OfficeAzureCommunication {
    using WorkSessionDict = ConcurrentDictionary<Guid, WorkSessionInfo>;
    [DataContract(Namespace = BroadcastNamespaces.DataContractNamespace)]
    [Serializable]
    public class WorkSessionServerInfo {
        [DataMember]
        public string RoleInstanceId { get; set; }
        [DataMember]
        public string HostServerName { get; set; }
        [DataMember]
        public string HostServerIP { get; set; }
        [DataMember]
        public float RemainingMemory { get; set; }
        [DataMember]
        public DateTime LastUpdateTime { get; set; }
        [DataMember]
        public WorkSessionServerStatus Status { get; private set; }
        [DataMember]
        public WorkSessionDict WorkSessions { get; private set; }

        public string RoleName {
            get {
                var role = RoleInstanceUtils.GetRoleByInstanceID(RoleInstanceId);
                if(role != null)
                    return role.Name;
                return string.Empty;
            }
        }

        public WorkSessionServerInfo() {
            WorkSessions = new WorkSessionDict();
        }

        public WorkSessionServerInfo(string roleInstanceId, string hostServerName, string hostServerIP) : this() {
            RoleInstanceId = roleInstanceId;
            HostServerName = hostServerName;
            HostServerIP = hostServerIP;
            GetServerParameters();
            Status = WorkSessionServerStatus.Online;
        }

        public WorkSessionServerInfo(WorkSessionServerInfo serverInfo) : this() {
            RoleInstanceId = serverInfo.RoleInstanceId;
            HostServerName = serverInfo.HostServerName;
            HostServerIP = serverInfo.HostServerIP;
            LastUpdateTime = serverInfo.LastUpdateTime;
            RemainingMemory = serverInfo.RemainingMemory;
            Status = serverInfo.Status;
        }

        private float GetAvailableMemory() {
            PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes", true);
            return ramCounter.NextValue();
        }

        public void GetServerParameters() {
            RemainingMemory = GetAvailableMemory();
            LastUpdateTime = DateTime.Now;
        }

        public void SetStatus(WorkSessionServerStatus status) {
            Status = status;
        }

        public bool IsDocumentServer() {
            return RoleName == RoleEnvironmentConfig.DocumentServerRoleName;
        }
        public bool IsProbablyShuttingDown() {
            return string.IsNullOrEmpty(RoleName);
        }
        public bool IsCurrent() {
            return RoleInstanceId == RoleInstanceUtils.GetCurrentRoleInstanceID();
        }
    }

    public class WorkSessionServerInfoComparer : IEqualityComparer<WorkSessionServerInfo> {
        public bool Equals(WorkSessionServerInfo x, WorkSessionServerInfo y) {
            if(Object.ReferenceEquals(x, y))
                return true;
            if(Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.RoleInstanceId == y.RoleInstanceId &&
                x.HostServerName == y.HostServerName &&
                x.HostServerIP == y.HostServerIP;
        }

        public int GetHashCode(WorkSessionServerInfo obj) {
            string workSessionServerInfoString = string.Format("{0}:{1}:{2}", obj.RoleInstanceId, obj.HostServerName, obj.HostServerIP);
            return workSessionServerInfoString.GetHashCode();
        }
    }
}
