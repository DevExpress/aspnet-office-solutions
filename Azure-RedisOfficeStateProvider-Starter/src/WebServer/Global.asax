<%@ Application Language="C#" %>

<script RunAt="server">

    void Application_Start(object sender, EventArgs e) {
        // Code that runs on application startup

        // Office State Provider
        var redisConnectionString = ConfigurationManager.ConnectionStrings["RedisStateStorageConnectionString"].ConnectionString;
        var provider = new DevExpress.Web.RedisOfficeStateProvider.RedisOfficeStateProvider(redisConnectionString);
        DevExpress.Web.Office.DocumentManager.StateProvider = provider;

        // Settings
        // provider.StateTimeout = 0;
        // provider.StateLockTimeout = 10;
        // provider.TrackStateLastAccessTime = false;

		// Custom State Providing
        //provider.CustomStateProviding += (DevExpress.Web.RedisOfficeStateProvider.CustomStateProvidingEventArgs args) => {
        //    var connectionString = "{Put your SQL Server connection string here}";
        //    var tableName = "states";
        //    var command = string.Format(@"SELECT state from {0} WHERE id=@id", tableName);

        //    try {
        //        using (var connection = new System.Data.SqlClient.SqlConnection(connectionString)) {
        //            var cmd = new System.Data.SqlClient.SqlCommand();
        //            cmd.CommandText = command;
        //            cmd.Parameters.AddWithValue("@id", args.WorkSessionId + "_state");
        //            cmd.Connection = connection;
        //            connection.Open();
        //            args.WorkSessionState = System.Text.Encoding.ASCII.GetString((byte[])cmd.ExecuteScalar());
        //            connection.Close();
        //        }
        //    } catch (Exception exception) {
        //    }

        //};

        // Redis log
        // RedisDiagnostics.RedisLogger.Init();
    }

    void Application_End(object sender, EventArgs e) {
        //  Code that runs on application shutdown

    }

    void Application_Error(object sender, EventArgs e) {
        // Code that runs when an unhandled error occurs

    }

    void Session_Start(object sender, EventArgs e) {
        // Code that runs when a new session is started

    }

    void Session_End(object sender, EventArgs e) {
        // Code that runs when a session ends. 
        // Note: The Session_End event is raised only when the sessionstate mode
        // is set to InProc in the Web.config file. If session mode is set to StateServer 
        // or SQLServer, the event is not raised.

    }

</script>
