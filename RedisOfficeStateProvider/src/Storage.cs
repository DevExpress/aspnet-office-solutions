using DevExpress.Web.Office;
using DevExpress.Web.RedisOfficeStateProvider;
using System;

namespace DevExpress.Web.RedisOfficeStateProvider {

    public class RedisOfficeStateStorageSettings {
        public int StateTTL { get; set; }
        public int LockerTTL { get; set; }
        public bool TrackStateLastAccessTime { get; set; }
    }

    public class RedisOfficeStateStorageRemote : IOfficeStateStorageRemote {

        static object lockObject = new object();
        private static RedisConnection redisConnection;

        public RedisOfficeStateStorageRemote(string connectionString) {
            Settings = new RedisOfficeStateStorageSettings() { StateTTL = -1, LockerTTL = 10, TrackStateLastAccessTime = false };
            ConnectionString = connectionString;
        }

        public static string ConnectionString { get; private set; }
        public static RedisConnection RedisConnection {
            get {
                if (redisConnection == null) {
                    lock (lockObject) {
                        if (redisConnection == null) {
                            redisConnection = new RedisConnection(ConnectionString);
                        }
                    }
                }
                return redisConnection;
            }
        }

        public RedisOfficeStateStorageSettings Settings { get; internal set; }

        event CustomStateProvidingEventHandler customStateProviding;
        public event CustomStateProvidingEventHandler CustomStateProviding {
            add { customStateProviding += value; }
            remove { customStateProviding -= value; }
        }

        string RaiseCustomStateProviding(string workSessionId) {
            string state = null;
            if (customStateProviding != null) {
                var arg = new CustomStateProvidingEventArgs(workSessionId);
                customStateProviding.Invoke(arg);
                state = arg.WorkSessionState;
            }
            return state;
        }

        protected virtual string LockerId { get { return OfficeStateProviderBase.GlobalLockerId; } }

        #region IOfficeStateStorageRemote

        public bool AddCheckedOut(string workSessionId, string documentId) {
            return (bool)TryRepeater.Do(() => RedisImplementation.AddCheckedOut(RedisConnection, LockerId, workSessionId, documentId.ToLower(), Settings));
        }

        public bool CheckIn(string workSessionId, string documentId, string workSessionState) {
            return (bool)TryRepeater.Do(() => RedisImplementation.CheckIn(RedisConnection, LockerId, workSessionId, documentId.ToLower(), workSessionState, Settings));
        }

        public string FindWorkSessionId(string documentId) {
            return (string)TryRepeater.Do(() => RedisImplementation.FindWorkSessionId(RedisConnection, documentId.ToLower()));
        }

        public bool HasWorkSessionId(string workSessionId) {
            return (bool)TryRepeater.Do(() => RedisImplementation.HasWorkSessionId(RedisConnection, workSessionId, Settings));
        }

        public void Remove(string workSessionId) {
            TryRepeater.Do(() => RedisImplementation.Remove(RedisConnection, LockerId, workSessionId, Settings));
        }

        public bool CheckOut(string workSessionId, out string workSessionState) {
            string workSessionStateFromRedis = null;
            var success = (bool)TryRepeater.Do(() => RedisImplementation.CheckOut(RedisConnection, LockerId, workSessionId, Settings, out workSessionStateFromRedis));

            if (workSessionStateFromRedis != null)
                workSessionState = workSessionStateFromRedis;
            else
                workSessionState = RaiseCustomStateProviding(workSessionId);

            return success;
        }
        public void UndoCheckOut(string workSessionId) {
            TryRepeater.Do(() => RedisImplementation.UndoCheckOut(RedisConnection, LockerId, workSessionId, Settings));
        }

        public void Set(string key, string value) {
            RedisConnection.Database.StringSet(key, value);
        }

        public string Get(string key) {
            return RedisConnection.Database.StringGet(key);
        }

        #endregion
    }

}
