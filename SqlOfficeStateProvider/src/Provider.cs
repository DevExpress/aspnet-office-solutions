using System;
using DevExpress.Web.Office;

namespace DevExpress.Web.SqlOfficeStateProvider {

    public class SqlOfficeStateProvider : OfficeStateProviderBase {
        public SqlOfficeStateProvider(string connectionString) : base(new SqlOfficeStateStorageRemote(connectionString)) {
            CommandTimeout = 600;
        }

        public static int CommandTimeout { get; set; }
    }

}
