using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using DevExpress.Web.Office;

namespace DevExpress.Web.SqlOfficeStateProvider {

    public class SqlOfficeStateStorageRemote : IOfficeStateStorageRemote {

        protected string ConnectionString { get; private set; }

        public SqlOfficeStateStorageRemote(string connectionString) {
            ConnectionString = connectionString;
        }

        protected virtual string LockerId { get { return OfficeStateProviderBase.GlobalLockerId; } }

        public bool AddCheckedOut(string workSessionId, string documentId) {
            return (bool)TryRepeater.Do(() => SqlImplementation.AddCheckedOut(ConnectionString, workSessionId, documentId.ToLower(), LockerId));
        }

        public bool CheckIn(string workSessionId, string documentId, string workSessionState) {
            return (bool)TryRepeater.Do(() => SqlImplementation.CheckIn(ConnectionString, workSessionId, documentId.ToLower(), workSessionState, LockerId));
        }

        public string FindWorkSessionId(string documentId) {
            return (string)TryRepeater.Do(() => SqlImplementation.FindWorkSessionId(ConnectionString, documentId.ToLower()));
        }

        public bool HasWorkSessionId(string workSessionId) {
            return (bool)TryRepeater.Do(() => SqlImplementation.HasWorkSessionId(ConnectionString, workSessionId));
        }

        public void Remove(string workSessionId) {
            TryRepeater.Do(() => SqlImplementation.Remove(ConnectionString, workSessionId, LockerId));
        }

        public bool CheckOut(string workSessionId, out string workSessionState) {
            string workSessionStateFromSql = null;
            var success = (bool)TryRepeater.Do(() => SqlImplementation.CheckOut(ConnectionString, workSessionId, LockerId, out workSessionStateFromSql));
            workSessionState = workSessionStateFromSql;
            return success;
        }

        public void UndoCheckOut(string workSessionId) {
            TryRepeater.Do(() => SqlImplementation.UndoCheckOut(ConnectionString, workSessionId, LockerId));
        }

        public void Set(string key, string value) {
            SqlImplementation.Set(ConnectionString, key, value);
        }
        public string Get(string key) {
            return SqlImplementation.Get(ConnectionString, key);
        }
    }

}
