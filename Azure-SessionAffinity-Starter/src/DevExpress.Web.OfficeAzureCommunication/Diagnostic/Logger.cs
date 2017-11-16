#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Web.OfficeAzureCommunication.Diagnostic {

    public static class Logger {
        static System.IO.StreamWriter file = null;
        static List<string> log = new List<string>();

        static Logger() {
            if(!InitLogFile(""))
                InitLogFile(System.IO.Path.GetTempPath());
        }

        private static bool InitLogFile(string path) {
            try {
                file = new System.IO.StreamWriter(System.IO.Path.Combine(path, "aspxlog.txt"));
                file.WriteLine("test start");
            } catch {
                return false;
            }
            return true;
        }

        public static void Log(string msg) {
            string now = DateTime.Now.ToString();
            msg = now + ": " + msg;

            SaveLogMessage(msg);
        }
        public static string GetWorkSessionList(Message msg) {
            StringBuilder sb = new StringBuilder();
            foreach(var server in msg.RegisteredServers) {
                sb.AppendFormat("{0} (remaining memory {1}", server.RoleInstanceId, server.RemainingMemory);
                if(server.WorkSessions.Count() > 0)
                    sb.Append(". WorkSessions: ");
                foreach(var ws in server.WorkSessions.Select(v => v.Value))
                    sb.AppendFormat("{0}, ", ws.WorkSessionID);
                sb.Append("); ");
            }
            return sb.ToString();
        }
        public static void Log(Message msg) {
            string message = string.Format("\"{0}\" says: \"{1}\". Registered servers: {2}", 
                msg.RoleInstanceId, msg.MessageOperation.ToString(), GetWorkSessionList(msg));
            Log(message);
        } 

        private static void SaveLogMessage(string msg) {
            log.Add(msg);

            if(file != null) {
                file.WriteLine(msg);
                file.Flush();
            }
        }

        public static List<string> GetLog() {
            return log;
        }

    }
}

#endif