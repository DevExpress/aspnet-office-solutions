using System;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace DevExpress.Web.OfficeAzureCommunication.Diagnostic {
#if DEBUG

    public static class WorkSessionServerView {
        
        public static string CreateDianosticPageHtml(string serverName, Func<string, string> getHeaderValue) {
            RoutingTable.EnsureServerIsPrepared();
            StringBuilder html = new StringBuilder();
            html.AppendLine(@"<!DOCTYPE html>");
            html.AppendLine(@"<html><head>");
            html.AppendLine(@"<title>Diagnostic</title>");
            html.AppendLine(GetStylesBlock());
            html.AppendLine(GetScriptBlock());
            html.AppendLine(@"</head><body>");

            html.AppendFormat("{0}<hr/>", WorkSessionServerView.GetHeader(serverName));

            html.AppendFormat("{0}<hr/>", WorkSessionServerView.GetServersTable(getHeaderValue));

            if(RoutingTable.HasWorkSessions())
                html.AppendFormat("{0}<hr/>", WorkSessionServerView.GetWorkSessionTable());

            html.AppendFormat("{0}<hr/>", WorkSessionServerView.GetTraceLog());

            html.AppendLine(@"</body></html>");
            
            return html.ToString();
        }
        public static void CreateDianosticPageControl(Control container, string serverName) {
            container.Controls.Clear();
            var html = CreateDianosticPageHtml(serverName, null);
            LiteralControl wrapper = new LiteralControl(html);
            container.Controls.Add(wrapper);
        }

        static string GetHeader(string serverName) {
            var sb = new StringBuilder();
            sb.Append(@"<h1>");
            sb.AppendFormat(@"<span>{0}</span>", serverName);
            sb.AppendFormat(@"<span>{0}</span>", DateTime.Now.ToShortTimeString());
            sb.Append(@"</h1>");
            return sb.ToString();
        }
        static string GetServersTable(Func<string, string> getHeaderValue) {
            var sb = new StringBuilder();
            sb.AppendLine("<h2>Servers Table</h2>");
            sb.AppendLine("<table class='log'>");
            var row = string.Format("<tr><th>Role Instance ID</th><th>Role Name</th><th>Server Name</th><th>IP</th><th>Status</th><th>Available Memory (Mb)</th><th>Last Update Time</th><th>Hash</th></tr>");
            sb.AppendLine(row);
            sb.Append(GetRoleServers(RoleEnvironmentConfig.RoutingRoleName, null));
            sb.Append(GetRoleServers(RoleEnvironmentConfig.DocumentServerRoleName, getHeaderValue));
            sb.AppendLine("</table>");
            return sb.ToString();
        }
        static string GetRoleServers(string roleName, Func<string, string> getHeaderValue) {
            var sb = new StringBuilder();
            if(getHeaderValue == null)
                getHeaderValue = (v) => { return string.Empty; };
            var row = string.Format("<tr><td colspan='8' class='groupName'>{0}</td></tr>", roleName);
            sb.AppendLine(row);
            RoutingTable.ForEachWorkSessionServer(s => s.RoleName == roleName, (id, serverInfo) => {
                row = string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td><td><a href='javascript:void(0)' onclick='setCookie(\"ARRAffinity\", \"\"); setCookie(\"ARRAffinity\", \"{7}\");'>{7}</a></td></tr>",
                    serverInfo.RoleInstanceId,
                    serverInfo.RoleName,
                    serverInfo.HostServerName,
                    serverInfo.HostServerIP,
                    Enum.GetName(typeof(WorkSessionServerStatus), serverInfo.Status),
                    serverInfo.RemainingMemory,
                    serverInfo.LastUpdateTime,
                    getHeaderValue(serverInfo.HostServerIP)
                );
                sb.AppendLine(row);
            });
            return sb.ToString();
        }
        static string GetWorkSessionTable() {
            var sb = new StringBuilder();
            sb.AppendLine("<h2>WorkSessions Table</h2>");
            sb.AppendLine("<table class='log'>");
            var row = string.Format("<tr><th>Work Session ID</th><th>Document ID</th><th>RoleInstance Id</th><th>Created At</th><th>Processed At</th><th>Delay (sec)</th><th>Status</th></tr>");
            sb.AppendLine(row);

            RoutingTable.ForEachWorkSessionServer((id, serverInfo) => {
                foreach(var ws in serverInfo.WorkSessions.Select(v => v.Value)) {
                    row = string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td></tr>",
                        ws.WorkSessionID,
                        ws.DocumentId,
                        serverInfo.RoleInstanceId,
                        ws.CreateTime,
                        ws.ProcessedTime,
                        (ws.ProcessedTime - ws.CreateTime).Seconds,
                        Enum.GetName(typeof(WorkSessionStatus), ws.Status)
                    );
                    sb.AppendLine(row);
                }
            });

            sb.AppendLine("</table>");
            return sb.ToString();
        }

        static string GetTraceLog() {
            var sb = new StringBuilder();

            sb.Append("<h2>Trace log</h2>");
            sb.Append("<table class='log list'>");

            var logs = Logger.GetLog();
            foreach(var log in logs) {
                sb.Append(string.Format("<tr><td>{0}</td></tr>", log));
            }

            sb.AppendLine("</table>");
            return sb.ToString();
        }
        
        static string GetStylesBlock() {
            return @"
                <style type='text/css'> 
                    table.log td { 
                        text-align: center; 
                        padding: 2px 10px; 
                    }
                    table.log.list td { 
                        text-align: left; 
                    }
                    table.log th {
                        background-color: lightgray;
                    }
                    span { 
                        padding: 0px 10px; 
                    }
                    .groupName {
                        background-color: #e0e0e0;
                        text-align: left !important;
                        font-weight: bold;
                    }
                </style>
            ";
        }

        static string GetScriptBlock() {
            return @"
                <script type='text/javascript'>
                    function setCookie(cookieName, cookieValue){
                        document.cookie = cookieName + '=' + cookieValue + '; expires=0; path =/; domain =.' + window.location.hostname;
                    }
                </script>
            ";
        }
    }
    
#endif
}
