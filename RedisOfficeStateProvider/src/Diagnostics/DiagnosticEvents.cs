using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DevExpress.Web.RedisOfficeStateProvider.Diagnostics {

    public enum RedisOfficeOperationType { AddCheckedOut, CheckOut, CheckIn, Find, HasWorkSessionId, UndoCheckOut, Remove }

    public delegate void RedisOfficeDiagnosticsEventHandler(RedisOfficeOperationType operationType, Dictionary<string, string> e);
    public delegate void RedisConnectionLogEventHandler(string logMessage);
    public delegate void RedisOperationTryRepeaterEventHandler(Exception e);

    public static class DiagnosticEvents {
        static object locker = new object();

        static RedisConnectionLogger connectionLogger;
        public static RedisConnectionLogger ConnectionLogger {
            get {
                if (connectionLogger == null) {
                    lock (locker) {
                        if (connectionLogger == null) {
                            connectionLogger = new RedisConnectionLogger();
                            connectionLogger.RedisConnectionMessageReceived += OnRedisConnectionMessageReceived;
                        }
                    }
                }
                return connectionLogger;
            }
        }

        static event RedisOfficeDiagnosticsEventHandler redisOperationCompleted;
        public static event RedisOfficeDiagnosticsEventHandler RedisOperationCompleted {
            add { redisOperationCompleted += value; }
            remove { redisOperationCompleted -= value; }
        }

        static event RedisConnectionLogEventHandler redisConnectionMessageReceived;
        public static event RedisConnectionLogEventHandler RedisConnectionMessageReceived {
            add { redisConnectionMessageReceived += value; }
            remove { redisConnectionMessageReceived -= value; }
        }

        static event RedisOperationTryRepeaterEventHandler redisOperationTryRepeaterTryException;
        public static event RedisOperationTryRepeaterEventHandler RedisOperationTryRepeaterTryException {
            add { redisOperationTryRepeaterTryException += value; }
            remove { redisOperationTryRepeaterTryException -= value; }
        }

        private static void OnRedisConnectionMessageReceived(string message) {
            redisConnectionMessageReceived?.Invoke(message);
        }
        public static void OnRedisOperationTryRepeaterTryException(Exception e) {
            redisOperationTryRepeaterTryException?.Invoke(e);
        }

        public static void OnCheckIn(string workSessionId, string documentId, string lockerId, string stateUpdateStatus, int locker_DeletedKeysCount, bool wasLockedByMe, bool wasLocked) {
            var eventArguments = new Dictionary<string, string>();
            eventArguments.Add("workSessionId", workSessionId);
            eventArguments.Add("documentId", documentId);
            eventArguments.Add("lockerId", lockerId);
            eventArguments.Add("wasLockedByMe", wasLockedByMe.ToString());
            eventArguments.Add("wasLocked", wasLocked.ToString());
            eventArguments.Add("stateUpdateStatus", stateUpdateStatus);
            eventArguments.Add("locker_DeletedKeysCount", locker_DeletedKeysCount.ToString());

            redisOperationCompleted?.Invoke(RedisOfficeOperationType.CheckIn, eventArguments);
        }
        public static void OnCheckOut(string workSessionId, string lockerId, bool wasLockedByAnother) {
            var eventArguments = new Dictionary<string, string>();
            eventArguments.Add("workSessionId", workSessionId);
            eventArguments.Add("lockerId", lockerId);
            eventArguments.Add("wasLockedByAnother", wasLockedByAnother.ToString());

            redisOperationCompleted?.Invoke(RedisOfficeOperationType.CheckOut, eventArguments);
        }
        public static void OnAddCheckOut(string workSessionId, string documentId, string lockerId, string stateUpdateStatus, bool sessionForThisDocumentIdFound, bool wasLockedByAnother, string docIdToStateIdUpdateStatus, string stateIdToDocIdUpdateStatus) {
            var eventArguments = new Dictionary<string, string>();
            eventArguments.Add("workSessionId", workSessionId);
            eventArguments.Add("documentId", documentId);
            eventArguments.Add("lockerId", lockerId);
            eventArguments.Add("wasLockedByAnother", wasLockedByAnother.ToString());
            eventArguments.Add("stateUpdateStatus", stateUpdateStatus);
            eventArguments.Add("sessionForThisDocumentIdFound", sessionForThisDocumentIdFound.ToString());
            eventArguments.Add("docIdToStateIdUpdateStatus", docIdToStateIdUpdateStatus.ToString());
            eventArguments.Add("stateIdToDocIdUpdateStatus", stateIdToDocIdUpdateStatus.ToString());

            redisOperationCompleted?.Invoke(RedisOfficeOperationType.AddCheckedOut, eventArguments);
        }

        public static void OnRemove(string workSessionId, string lockerId, bool wasLockedByAnother, int state_DeletedKeysCount, int stateToDoc_DeletedKeysCount, int docToState_DeletedKeysCount, int locker_DeletedKeysCount) {
            var eventArguments = new Dictionary<string, string>();
            eventArguments.Add("workSessionId", workSessionId);
            eventArguments.Add("lockerId", lockerId);
            eventArguments.Add("wasLockedByAnother", wasLockedByAnother.ToString());
            eventArguments.Add("state_DeletedKeysCount", state_DeletedKeysCount.ToString());
            eventArguments.Add("stateToDoc_DeletedKeysCount", stateToDoc_DeletedKeysCount.ToString());
            eventArguments.Add("docToState_DeletedKeysCount", docToState_DeletedKeysCount.ToString());
            eventArguments.Add("locker_DeletedKeysCount", locker_DeletedKeysCount.ToString());

            redisOperationCompleted?.Invoke(RedisOfficeOperationType.Remove, eventArguments);
        }

        public static void OnFind(string documentId, bool workSessionIdFound, string workSessionId) {
            var eventArguments = new Dictionary<string, string>();
            eventArguments.Add("documentId", documentId);
            eventArguments.Add("sessionIdFound", workSessionIdFound.ToString());
            eventArguments.Add("sessionId", workSessionId);

            redisOperationCompleted?.Invoke(RedisOfficeOperationType.Find, eventArguments);
        }

        public static void OnHasWorkSessionId(string workSessionId, bool sessionIdFound) {
            var eventArguments = new Dictionary<string, string>();
            eventArguments.Add("workSessionId", workSessionId);
            eventArguments.Add("sessionIdFound", sessionIdFound.ToString());

            redisOperationCompleted?.Invoke(RedisOfficeOperationType.HasWorkSessionId, eventArguments);
        }
        public static void OnUndoCheckOut(string workSessionId, string lockerId, int locker_DeletedKeysCount) {
            var eventArguments = new Dictionary<string, string>();
            eventArguments.Add("workSessionId", workSessionId);
            eventArguments.Add("lockerId", lockerId);
            eventArguments.Add("locker_DeletedKeysCount", locker_DeletedKeysCount.ToString());

            redisOperationCompleted?.Invoke(RedisOfficeOperationType.UndoCheckOut, eventArguments);
        }

    }
}
