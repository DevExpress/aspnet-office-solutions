using System;
using System.Configuration;

namespace AWS_RedisOfficeStateProvider_Starter
{
    public class Global_asax : System.Web.HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            DevExpress.Web.ASPxWebControl.CallbackError += new EventHandler(Application_Error);
            var redisConnectionString = ConfigurationManager.ConnectionStrings["RedisStateStorageConnectionString"].ConnectionString;
            var provider = new DevExpress.Web.RedisOfficeStateProvider.RedisOfficeStateProvider(redisConnectionString);
            DevExpress.Web.Office.DocumentManager.StateProvider = provider;
        }
        void Application_End(object sender, EventArgs e)
        {
            // Code that runs on application shutdown
        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
        }

        void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started
        }

        void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.
        }
    }
}
