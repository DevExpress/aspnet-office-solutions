using DevExpress.Web.Office.Internal;
using DevExpress.Web.RedisOfficeStateProvider;
using DevExpress.Web.RedisOfficeStateProvider.Diagnostics;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedisDiagnostics {
    public static class RedisLogger {
        static int lockCount = 0;
        static void Lock() { lockCount++; }
        static void UnLock() { lockCount--; }
        static bool Locked() { return lockCount != 0; }

        public static void Init() {
            DiagnosticEvents.RedisOperationCompleted += LogRedisOperationEvent;
            DiagnosticEvents.RedisConnectionMessageReceived += LogRedisConnectionMessage;
            DiagnosticEvents.RedisOperationTryRepeaterTryException += LogTryRepeaterTryException;
            WorkSessionAdminTools.CheckedOut += LogInternalCheckoutEvent;
            WorkSessionAdminTools.HandlerException += LogHandlerException;
        }

        private static void LogTryRepeaterTryException(Exception e) {
            string status = GetExceptionLogText(e);
            LogRedisOperationEventCore(
                "TryRepeater Step-Try Exception",
                "-",
                "-",
                StateLockerIdentificator.GeGlobalDomainId(), 
                status);
        }

        private static void LogHandlerException(IWorkSession workSession, DocumentRequestProcessingExceptionEventArgs e) {
            string status = GetExceptionLogText(e.Exception);

            LogRedisOperationEventCore(
                "Handler Exception",
                e.WorkSessionId.ToString(),
                e.DocumentInfo != null ? e.DocumentInfo.DocumentId : "none",
                StateLockerIdentificator.GeGlobalDomainId(),
                status);
        }

        private static string GetExceptionLogText(Exception e) {
            return string.Format("Exception type: {0}\n Message:{1}", e.GetType(), e.Message);
        }

        public static void LogInternalCheckoutEvent(IWorkSession workSession, DocumentDiagnosticEventArgs args) {
            LogRedisOperationEventCore(
                "internal check out",
                workSession != null ? workSession.ID.ToString() : "none",
                workSession != null ? workSession.DocumentInfo.DocumentId : "none",
                StateLockerIdentificator.GeGlobalDomainId(), "unknown");
        }

        public static string PatchDomainId(string domainId) {
            return domainId + "(" + System.Diagnostics.Process.GetCurrentProcess().Id.ToString() + "/" + StateLockerIdentificator.GetProcessesThreadId() + ")";
        }

        public static void LogRedisConnectionMessage(string message) {
            if (Locked())
                return;

            Lock();
            LogToRedis("Redis Connection Message:" + message);
            UnLock();
        }

        public static void LogRedisOperationEvent(RedisOfficeOperationType operationType, System.Collections.Generic.Dictionary<string, string> args) {
            var command = Enum.GetName(typeof(RedisOfficeOperationType), operationType);
            var workSessionId = args.ContainsKey("workSessionId") ? args["workSessionId"] : "";
            var documentId = args.ContainsKey("documentId") ? args["documentId"] : "";
            var lockerId = args.ContainsKey("lockerId") ? args["lockerId"] : "";

            args.Remove("workSessionId");
            args.Remove("documentId");
            args.Remove("lockerId");

            var sb = new StringBuilder();
            foreach (KeyValuePair<string, string> kvp in args) {
                sb.AppendFormat("{0}='{1}' ", kvp.Key, kvp.Value);
            }

            LogRedisOperationEventCore(command, workSessionId, documentId, lockerId, sb.ToString());
        }

        public static void LogRedisOperationEventCore(string command, string workSessionId, string documentId, string lockerId, string status) {
            lockerId = PatchDomainId(lockerId);
            var handler = "";
            if (!string.IsNullOrEmpty(lockerId))
                handler = "handler=" + lockerId;

            RedisValue value = string.Format("|{0}|{1}|{2}|{3}|{4}", command, workSessionId, documentId, status, handler);
            LogToRedis(value);
        }

        private static void LogToRedis(RedisValue value) {
            var now = DateTime.Now;
            var time = string.Format("{0}({1}:{2}:{3}:{4})", now.Ticks, now.Hour, now.Minute, now.Second, now.Millisecond);
            RedisKey key = "log_" + time;

            RedisOfficeStateStorageRemote.RedisConnection.Database.StringAppend(key, value);
        }
    }
}