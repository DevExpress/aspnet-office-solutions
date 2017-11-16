using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using DevExpress.Web.OfficeAzureCommunication.Utils;
#if DEBUG
using DevExpress.Web.OfficeAzureCommunication.Diagnostic;
#endif

namespace DevExpress.Web.OfficeAzureCommunication {
    using WorkSessionServerInfoDict = ConcurrentDictionary<string, WorkSessionServerInfo>;
    using EventHandlerDict = ConcurrentDictionary<string, ServerChangedEventHandler>;

    public delegate void ServerChangedEventHandler(WorkSessionServerInfo server);

    public static class RoutingTable {
        static string ServerAddedEventKey = "ServerAdded";
        static string ServerRemovedEventKey = "ServerRemoved";

        static object locker = new object();
        static WorkSessionServerInfoDict all = new WorkSessionServerInfoDict();
        public static WorkSessionServerInfoDict All {
            get { return all; }
        }

        #region Events
        static EventHandlerDict serverEvents = new EventHandlerDict();

        public static event ServerChangedEventHandler ServerAdded {
            add {
                if(!value.Method.IsStatic)
                    throw new NotSupportedException("Only Static event handlers are allowed for the ServerAdded event");
                AddEventHandler(ServerAddedEventKey, value);
            }
            remove {
                RemoveEventHandler(ServerAddedEventKey, value);
            }
        }

        public static event ServerChangedEventHandler ServerRemoved {
            add {
                if(!value.Method.IsStatic)
                    throw new NotSupportedException("Only Static event handlers are allowed for the ServerRemoved event");
                AddEventHandler(ServerRemovedEventKey, value);
            }
            remove {
                RemoveEventHandler(ServerRemovedEventKey, value);
            }
        }
        static void AddEventHandler(string eventName, ServerChangedEventHandler eventHandler) {
            var eventElement = serverEvents.GetOrAdd(eventName, eventHandler);
            if(!serverEvents[eventName].GetInvocationList().Contains(eventHandler))
                eventElement += eventHandler;
        }

        static void RemoveEventHandler(string eventName, ServerChangedEventHandler eventHandler) {
            if(serverEvents.ContainsKey(eventName))
                serverEvents[eventName] -= eventHandler;
        }
        #endregion

        static WorkSessionServerStatus selfStatus { get; set; }
        public static bool ReadyForRouting { get { return GetAllDocumentServers().Count() > 0; } }
        public static bool ReadyForPing {
            get {
                return !ServerIsShuttingDown();
            }
        }

        public static void EnsureServerIsPrepared() {
            if(!ReadyForRouting)
                RoutingTableSerializer.RestoreDataFromCache();
            ForEachWorkSessionServer((id, server) => {
                if(string.IsNullOrEmpty(server.RoleName)) {
                    All.TryRemove(id, out server);
                }

            });
        }
        public static void SetSelfState(WorkSessionServerStatus status) {
            selfStatus = WorkSessionServerStatus.Online;
        }
        
        public static WorkSessionServerStatus GetSelfStatus() {
            return selfStatus;
        }

        static bool ServerIsShuttingDown() {
            return GetSelfStatus() == WorkSessionServerStatus.ShuttingDown;
        }
       
        public static void ForEachWorkSessionServer(Action<string, WorkSessionServerInfo> action) {
            lock(locker) {
                BeginUpdate();
                try {
                    foreach(var kvp in All) {
                        action(kvp.Key, kvp.Value);
                    }

                } finally {
                    EndUpdate();
                }
            }
        }
        public static void ForEachWorkSessionServer(Predicate<WorkSessionServerInfo> predicate, Action<string, WorkSessionServerInfo> action) {
            lock(locker) {
                BeginUpdate();
                try {
                    foreach(var kvp in All) {
                        if(predicate(kvp.Value))
                            action(kvp.Key, kvp.Value);
                    }

                } finally {
                    EndUpdate();
                }
            }
        }
        static void AddWorkSessionServer(WorkSessionServerInfo server) {
            lock(locker) {
                BeginUpdate();
                try {
                    WorkSessionServerInfo serverWithoutWorkSessions = new WorkSessionServerInfo(server);
                    All.AddOrUpdate(server.RoleInstanceId, serverWithoutWorkSessions, (k, s) => {
                        s.HostServerIP = serverWithoutWorkSessions.HostServerIP;
                        s.HostServerName = serverWithoutWorkSessions.HostServerName;
                        s.LastUpdateTime = DateTime.Now;
                        s.RemainingMemory = serverWithoutWorkSessions.RemainingMemory;
                        s.SetStatus(serverWithoutWorkSessions.Status);
                        return s;
                    });
                } finally {
                    EndUpdate();
                }
            }
        }
        public static void AddWorkSessionServers(List<WorkSessionServerInfo> servers) {
            foreach(WorkSessionServerInfo server in servers)
                All.AddOrUpdate(server.RoleInstanceId, server, (k, s) => server);
        }
        public static void RemoveWorkSessionServer(WorkSessionServerInfo server) {
            lock(locker) {
                BeginUpdate();
                try {
                    bool found = All.TryGetValue(server.RoleInstanceId, out WorkSessionServerInfo deletedServerInfo);
                    if(found)
                        All.TryRemove(server.RoleInstanceId, out deletedServerInfo);
                } finally {
                    EndUpdate();
                }
            }
        }

        public static void CheckForOutdatedServers(TimeSpan serverInfoExpirationInterval) {
            lock(locker) {
                BeginUpdate();
                try {
                    DateTime serverInfoExpirationTime = DateTime.Now.Subtract(serverInfoExpirationInterval);
                    IEnumerable<WorkSessionServerInfo> outdatedServers = All.Where(serverInfo => (serverInfo.Value.LastUpdateTime < serverInfoExpirationTime && serverInfo.Value.Status == WorkSessionServerStatus.Online)
                    || string.IsNullOrEmpty(serverInfo.Value.RoleName)).Select(serverInfo => serverInfo.Value);
                    foreach(WorkSessionServerInfo server in outdatedServers) {
                        server.SetStatus(WorkSessionServerStatus.PingTimeout);
                    }
                } finally {
                    EndUpdate();
                }
            }
        }

        public static void GetWorkSessionServerStateFromMessage(Message msg) {
            EnsureServerIsPrepared();
            if(msg.Sender.Status == WorkSessionServerStatus.Online)
                AddWorkSessionServer(msg.Sender);
            else if(msg.Sender.Status == WorkSessionServerStatus.ShuttingDown && msg.Sender.RoleInstanceId == RoleInstanceUtils.GetCurrentRoleInstanceID())
                SetSelfState(WorkSessionServerStatus.ShuttingDown);
        }
        public static void AddWorkSession(Message msg) {
            lock(locker) {
                BeginUpdate();
                try {
                    WorkSessionServerInfo existingServerInfo = All.GetOrAdd(msg.RoleInstanceId, msg.Sender);
                    foreach(KeyValuePair<Guid, WorkSessionInfo> ws in msg.Sender.WorkSessions) {
                        #if DEBUG
                        Diagnostic.Logger.Log(string.Format("Add worksession {0} to server {1}", ws.Key, existingServerInfo.RoleInstanceId));
                        #endif
                        existingServerInfo.WorkSessions.AddOrUpdate(ws.Key, ws.Value, (k, w) => {
                            w.DocumentId = ws.Value.DocumentId;
                            w.ProcessedTime = ws.Value.ProcessedTime;
                            w.SetStatus(ws.Value.Status);
                            return w;
                        });
                    }
                    EnsureWorkSessionsAreNotDuplicated(existingServerInfo);
                } finally {
                    EndUpdate();
                }
            }
        }
        static void EnsureWorkSessionsAreNotDuplicated(WorkSessionServerInfo workSessionServerInfo) {
            lock(locker) {
                foreach(KeyValuePair<Guid, WorkSessionInfo> ws in workSessionServerInfo.WorkSessions) {
                    IEnumerable<WorkSessionServerInfo> workSessionServers = GetWorkSessionServersByWorkSessionID(ws.Key);
                    foreach(var server in workSessionServers) {
                        if(server.RoleInstanceId != workSessionServerInfo.RoleInstanceId)
                            server.WorkSessions.TryRemove(ws.Key, out WorkSessionInfo removedWorkSession);
                    }
                }
            }
        }
        public static void RemoveWorkSession(Message msg) {
            lock(locker) {
                BeginUpdate();
                try {
                    WorkSessionServerInfo existingServerInfo;
                    if(!All.TryGetValue(msg.RoleInstanceId, out existingServerInfo))
                        return;
                    existingServerInfo = All.GetOrAdd(msg.RoleInstanceId, msg.Sender);
                    foreach(KeyValuePair<Guid, WorkSessionInfo> ws in msg.Sender.WorkSessions) {
                        bool found = existingServerInfo.WorkSessions.TryGetValue(ws.Key, out WorkSessionInfo deletedWorkSessionInfo);
                        if(found) {
                            #if DEBUG
                            Diagnostic.Logger.Log(string.Format("Remove worksession {0} from server {1}", deletedWorkSessionInfo.WorkSessionID, existingServerInfo.RoleInstanceId));
                            #endif
                            existingServerInfo.WorkSessions.TryRemove(ws.Key, out deletedWorkSessionInfo);
                        }
                    }
                } finally {
                    EndUpdate();
                }
            }
        }

        public static void ChangeWorkSessionStatus(Message msg) {
            lock(locker) {
                BeginUpdate();
                try {
                    if(string.IsNullOrEmpty(msg.Sender.RoleName) || msg.Sender.Status != WorkSessionServerStatus.Online)
                        return;
                    WorkSessionServerInfo existingServerInfo = All.GetOrAdd(msg.RoleInstanceId, msg.Sender);
                    var wsStatus = GetStatusFromOperation(msg.MessageOperation);
                    foreach(KeyValuePair<Guid, WorkSessionInfo> ws in msg.Sender.WorkSessions) {
                        bool found = existingServerInfo.WorkSessions.TryGetValue(ws.Key, out WorkSessionInfo updatedWorkSessionInfo);
                        if(found) {
                            #if DEBUG
                            Diagnostic.Logger.Log(string.Format("Change worksession status from {0} to {1}", updatedWorkSessionInfo.Status.ToString(), wsStatus));
                            #endif
                            updatedWorkSessionInfo.SetStatus(wsStatus);
                        }
                    }
                } finally {
                    EndUpdate();
                }
            }
        }
        private static WorkSessionStatus GetStatusFromOperation(MessageOperation messageOperation) {
            switch(messageOperation) {
                case MessageOperation.AutoSaveWorkSession:
                    return WorkSessionStatus.AutoSaved;
                case MessageOperation.HibernateWorkSession:
                    return WorkSessionStatus.Hibernated;
                case MessageOperation.WakeUpWorkSession:
                    return WorkSessionStatus.WokenUp;
                case MessageOperation.AddWorkSession:
                    return WorkSessionStatus.Loaded;
                case MessageOperation.RemoveWorkSession:
                    return WorkSessionStatus.Removed;
            }
            return WorkSessionStatus.Loaded;
        }
        public static void UpdateAllWorkSessionsFromOneServer(Message msg) {
            lock(locker) {
                BeginUpdate();
                try {
                    HashSet<string> updatedServerIDs = new HashSet<string>();
                    foreach(WorkSessionServerInfo serverInfo in msg.RegisteredServers) {
                        if(string.IsNullOrEmpty(serverInfo.RoleName))
                            continue;

                        if(!All.TryGetValue(serverInfo.RoleInstanceId, out WorkSessionServerInfo existingServer)) {
                            if(msg.Sender.RoleInstanceId == serverInfo.RoleInstanceId || serverInfo.WorkSessions.Count() > 0) {
                            #if DEBUG
                                Diagnostic.Logger.Log(string.Format("UpdateAll adds server {0}", serverInfo.RoleInstanceId));
                            #endif
                            } else
                                continue;
                        }
                        WorkSessionServerInfo currentServer = All.GetOrAdd(serverInfo.RoleInstanceId, serverInfo);
                        HashSet<Guid> updatedWorkSessionIDs = new HashSet<Guid>();
                        foreach(WorkSessionInfo workSessionInfo in serverInfo.WorkSessions.Select(v => v.Value)) {
                            #if DEBUG
                            if(!currentServer.WorkSessions.TryGetValue(workSessionInfo.WorkSessionID, out WorkSessionInfo existingWorkSession))
                                Diagnostic.Logger.Log(string.Format("UpdateAll adds worksession {0} on server {1}", existingWorkSession.WorkSessionID, currentServer.RoleInstanceId));
                            #endif
                            currentServer.WorkSessions.GetOrAdd(workSessionInfo.WorkSessionID, workSessionInfo);
                            updatedWorkSessionIDs.Add(workSessionInfo.WorkSessionID);
                        }
                        if(currentServer.RoleInstanceId == msg.Sender.RoleInstanceId) {
                            foreach(WorkSessionInfo workSessionInfo in currentServer.WorkSessions.Select(w => w.Value)) {
                                if(!updatedWorkSessionIDs.Contains(workSessionInfo.WorkSessionID)){
                                    currentServer.WorkSessions.TryRemove(workSessionInfo.WorkSessionID, out WorkSessionInfo deletedWorkSessionInfo);
                                    #if DEBUG
                                    Diagnostic.Logger.Log(string.Format("UpdateAll removes worksession {0} from server {1}", workSessionInfo.WorkSessionID, currentServer.RoleInstanceId));
                                    #endif
                                }
                            }
                        }
                        updatedServerIDs.Add(serverInfo.RoleInstanceId);
                    }
                } finally {
                    EndUpdate();
                }
            }
        }
        public static WorkSessionServerInfo GetWorkSessionServerInfoByWorkSessionID(string strWorkSessionID) {
            if(Guid.TryParse(strWorkSessionID, out Guid workSessionID))
                return GetWorkSessionServerInfoByWorkSessionID(workSessionID);
            return null;
        }
        static WorkSessionServerInfo GetWorkSessionServerInfoByWorkSessionID(Guid workSessionID) {
            return GetWorkSessionServersByWorkSessionID(workSessionID).FirstOrDefault();
        }
        static IEnumerable<WorkSessionServerInfo> GetWorkSessionServersByWorkSessionID(Guid workSessionID) {
            lock(locker) {
                var servers = from serverInfo in All.Select(x => x.Value).ToArray()
                             from workSessionInfo in serverInfo.WorkSessions.Select(ws => ws.Value).ToArray()
                             where workSessionInfo.WorkSessionID == workSessionID
                             select serverInfo;
                return servers;
            }
        }
        public static bool IsServerAvailable(WorkSessionServerInfo serverInfo) {
            return !string.IsNullOrEmpty(serverInfo.RoleName) && serverInfo.Status == WorkSessionServerStatus.Online;
        }
        static IEnumerable<WorkSessionServerInfo> GetAllDocumentServers() {
            return All.Where(serverInfo => serverInfo.Value.RoleName == RoleEnvironmentConfig.DocumentServerRoleName).Select(serverInfo => serverInfo.Value);
        }
        public static WorkSessionServerInfo FindAvailableServerForNewWorkSession() {
            IEnumerable<WorkSessionServerInfo> availableServers = GetAllDocumentServers()
                .Where(serverInfo => serverInfo.Status == WorkSessionServerStatus.Online)
                .OrderByDescending(serverInfo => serverInfo.RemainingMemory);
            if(availableServers.Count() > 0)
                return availableServers.First();

            return null;
        }
        public static WorkSessionInfo GetWorkSessionInfoByDocumentID(string documentID) {
            IEnumerable<WorkSessionInfo> workSessions = GetWorkSessions().Where(wsInfo => wsInfo.DocumentId == documentID);
            if(workSessions.Count() > 0)
                return workSessions.First();
            return null;
        }
        public static List<WorkSessionInfo> GetWorkSessions() {
            lock(locker) {
                var workSessions = from serverInfo in All.Select(x => x.Value).ToArray()
                                  from workSessionInfo in serverInfo.WorkSessions.Select(ws => ws.Value).ToArray()
                                  select workSessionInfo;
                return workSessions.ToList();
            }
        }
        public static List<string> GetServerAdressesByRoleName(string roleName) {
            return All.Where(serverInfo => serverInfo.Value.RoleName == roleName).Select(serverInfo => serverInfo.Value.HostServerIP).ToList();
        }
        public static bool HasWorkSessions() {
            return GetWorkSessionServerInstances().Select(serverInfo => serverInfo.WorkSessions.Count).Sum() > 0;
        }

        #region State
        static ServerListStateChangedController stateController = new ServerListStateChangedController(
            GetWorkSessionServerInstances,
            OnServersAdded,
            OnServersRemoved);
        public static List<WorkSessionServerInfo> GetWorkSessionServerInstances() {
            return All.Select(serverInfo => serverInfo.Value).ToList();
        }
        static void BeginUpdate() {
            stateController.BeginUpdate();
        }
        static void EndUpdate() {
            stateController.EndUpdate();
        }
        static void UpdateCache() {
            RoutingTableSerializer.UpdateCache();
        }
        static void RestoreDataFromCache() {
            RoutingTableSerializer.RestoreDataFromCache();
        }
        static void OnServersAdded(IEnumerable<WorkSessionServerInfo> servers) {
            UpdateCache();
            foreach(WorkSessionServerInfo server in servers)
                RaiseServerAddedEvent(server);
        }
        static void OnServersRemoved(IEnumerable<WorkSessionServerInfo> servers) {
            UpdateCache();
            foreach(WorkSessionServerInfo server in servers)
                RaiseServerRemovedEvent(server);
        }
        static void RaiseServerAddedEvent(WorkSessionServerInfo server) {
            RaiseEvent(ServerAddedEventKey, server);
        }
        static void RaiseServerRemovedEvent(WorkSessionServerInfo server) {
            RaiseEvent(ServerRemovedEventKey, server);
        }
        static void RaiseEvent(string eventName, WorkSessionServerInfo server) {
            if(serverEvents.ContainsKey(eventName) && serverEvents[eventName] != null)
                serverEvents[eventName](server);
        }
        #endregion
    }
}
