using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevExpress.Web.OfficeAzureCommunication {
    public class WorkSessionServerState : ServerStateBase<Guid, WorkSessionInfo> {
        public new static Guid GetKey(WorkSessionInfo info) {
            return info.WorkSessionID;
        }
        public new static List<WorkSessionInfo> CreateObjectsFromMessage(Message msg) {
            return msg.WorkSessions;
        }
        public static List<WorkSessionInfo> GetWorkSessionsStoredOnServer(string serverName) {
            //var infos = new List<WorkSessionInfo>();
            return All.Where(v => string.Equals(v.Value.HostServerName, serverName, StringComparison.InvariantCultureIgnoreCase)).Select(x => x.Value).Select(x => new WorkSessionInfo(x.WorkSessionID, x.DocumentId)).ToList();
            //foreach(var p in All.Where(v => string.Equals(v.Value.HostServerName, serverName, StringComparison.InvariantCultureIgnoreCase)).Select(x => x.Value)) {
            //    infos.Add(new WorkSessionInfo(p.WorkSessionID, p.DocumentId));
            //}
            //return infos;
        }
        public static WorkSessionInfo GetWorkSessionInfoByID(string strWorkSessionID) {
            if(Guid.TryParse(strWorkSessionID, out Guid workSessionID))
                return GetWorkSessionInfoByID(workSessionID);
            return null;
        }
        public static WorkSessionInfo GetWorkSessionInfoByID(Guid workSessionID) {
            WorkSessionInfo workSessionInfo = null;
            ForEach((id, wi) => {
                if(Guid.Equals(workSessionID, wi.WorkSessionID))
                    workSessionInfo = wi;
            });
            return workSessionInfo;
        }
        public static WorkSessionInfo GetWorkSessionInfoByDocumentID(string documentID) {
            WorkSessionInfo workSessionInfo = null;
            ForEach((id, wi) => {
                if(Guid.Equals(documentID, wi.DocumentId))
                    workSessionInfo = wi;
            });
            return workSessionInfo;
        }
        public static string FindWorkSessionServer(string workSessionId) {
            WorkSessionInfo ws = GetWorkSessionInfoByID(workSessionId);
            if(ws != null)
                return ws.HostServerIP;
            return null;
        }
        static ServerStateChangedController<WorkSessionInfo> stateController = null;
        protected new IStateController GetStateController() {
            if(stateController == null)
                stateController = new ServerStateChangedController<WorkSessionInfo>(
                    GetAllObjectValues,
                    OnWorkSessionsAdded,
                    OnWorkSessionsRemoved,
                    new WorkSessionInfoComparer());
            return stateController;
        }
        static void OnWorkSessionsAdded(IEnumerable<WorkSessionInfo> servers) {
            UpdateCache();
        }
        static void OnWorkSessionsRemoved(IEnumerable<WorkSessionInfo> servers) {
            UpdateCache();
        }
    }

}
