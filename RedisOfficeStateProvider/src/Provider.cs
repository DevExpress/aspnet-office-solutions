using System;
using DevExpress.Web.Office;

namespace DevExpress.Web.RedisOfficeStateProvider {

    public class CustomStateProvidingEventArgs {
        public CustomStateProvidingEventArgs(string workSessionId) {
            WorkSessionId = workSessionId;
        }
        public string WorkSessionId { get; internal set; }
        public string WorkSessionState { get; set; }
    }

    public delegate void CustomStateProvidingEventHandler(CustomStateProvidingEventArgs args);

    public class RedisOfficeStateProvider : OfficeStateProviderBase {

        public RedisOfficeStateProvider(string connectionString) : base(new RedisOfficeStateStorageRemote(connectionString)) { }

        new RedisOfficeStateStorageRemote Storage { get { return (RedisOfficeStateStorageRemote)base.Storage; } }
        RedisOfficeStateStorageSettings Settings { get { return Storage.Settings; } }

        public int StateTimeout { get { return Settings.StateTTL; } set { Settings.StateTTL = value; } }
        public int StateLockTimeout { get { return Settings.LockerTTL; } set { Settings.LockerTTL = value; } }
        public bool TrackStateLastAccessTime { get { return Settings.TrackStateLastAccessTime; } set { Settings.TrackStateLastAccessTime = value; } }

        public event CustomStateProvidingEventHandler CustomStateProviding {
            add { Storage.CustomStateProviding += value; }
            remove { Storage.CustomStateProviding -= value; }
        }
    }

}
