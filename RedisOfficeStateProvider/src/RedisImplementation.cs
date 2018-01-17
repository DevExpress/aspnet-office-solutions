using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using DevExpress.Web.RedisOfficeStateProvider.Diagnostics;

namespace DevExpress.Web.RedisOfficeStateProvider {

    public static class RedisImplementation {

        const string tryToLock = @"
            local function tryToLock(lockerKeyName, lockerId, lockerTTL) 
                local wasLockedByAnother = false
                local wasLocked = redis.call('SETNX', lockerKeyName, lockerId) == 0
                if wasLocked then
                    local wasLockedBy = redis.call('GET', lockerKeyName)
                    wasLockedByAnother = wasLockedBy ~= lockerId
                end

                if not wasLockedByAnother then
                    redis.call('EXPIRE', lockerKeyName, lockerTTL)
                end

                return wasLockedByAnother
            end";
        const string checkStateExists = @"
            local function checkStateExists(stateKeyName) 
                local stateExistsStatus = redis.call('EXISTS', stateKeyName)
                local sessionExists = stateExistsStatus == 1
                return sessionExists
            end";
        const string getWorkSessionIdFromDocumentId = @"
            local function getWorkSessionIdFromDocumentId(DocumentIdKeyName) 
                local workSessionId = redis.call('GET', DocumentIdKeyName)
                local workSessionIdFound = not (type(workSessionId) == 'boolean' and not workSessionId)

                local retValues = {} 
                retValues[1] = workSessionIdFound
                retValues[2] = workSessionId

                return retValues
            end";

        public static bool CheckIn(RedisConnection redisConnection, string lockerId, string workSessionId, string documentId, string state, RedisOfficeStateStorageSettings settings) {
            var keyBunch = new KeyNameHelper(workSessionId, documentId);

            string[] keyArgs = new string[] { keyBunch.LockerKeyName, keyBunch.StateKeyName, keyBunch.DocumentIdToWorkSessionIdKeyName, keyBunch.WorkSessionIdToDocumentIdKeyName };
            object[] valueArgs = new object[] { lockerId, state, workSessionId, documentId };

            string checkInScript = @"
                local lockerKeyName = KEYS[1]
                local stateKeyName = KEYS[2]
                local documentIdToWorkSessionIdKeyName = KEYS[3]
                local workSessionIdToDocumentIdKeyName= KEYS[4]

                local lockerId = ARGV[1]
                local state = ARGV[2]
                local workSessionId = ARGV[3]
                local documentId = ARGV[4]

                local stateUpdateStatus = 'failed'
                local docIdToStateIdUpdateStatus = 'failed'
                local stateIdToDocIdUpdateStatus  = 'failed'
                local locker_DeletedKeysCount = 0

                updateATime(stateKeyName)
                
                local wasLockedBy = redis.call('GET', lockerKeyName)
                local lockerWasNotFound = type(wasLockedBy) == 'boolean' and not wasLockedBy
                local wasLocked = not lockerWasNotFound

                local wasLockedByMe = wasLockedBy == lockerId
                local wasLockedByAnother = wasLocked and not wasLockedByMe
                
                if not wasLockedByAnother then
                    stateUpdateStatus = redis.call('SET', stateKeyName, state)
                    setExpired(stateKeyName)
                    if not (documentId == nil or documentId == '') then
                        docIdToStateIdUpdateStatus = redis.call('SET', documentIdToWorkSessionIdKeyName, workSessionId)
                        stateIdToDocIdUpdateStatus = redis.call('SET', workSessionIdToDocumentIdKeyName, documentId)
                        setExpired(documentIdToWorkSessionIdKeyName)
                        setExpired(workSessionIdToDocumentIdKeyName)
                    end
                end
                if wasLockedByMe then
                    locker_DeletedKeysCount = redis.call('DEL', KEYS[1])
                end

                local retValues = {} 
                retValues[1] = stateUpdateStatus
                retValues[2] = locker_DeletedKeysCount
                retValues[3] = wasLockedByMe
                retValues[4] = wasLocked
                --[[
                docIdToStateIdUpdateStatus
                stateIdToDocIdUpdateStatus
                --]]

                return retValues
            ";

            var expireScript = ExpireHelper.GetExpireScript(settings);
            var atimeUpdateScript = GetAccessTimeUpdateScript(settings);
            var script = expireScript + atimeUpdateScript + checkInScript;
            var results = (RedisResult[])(RedisResult)redisConnection.ScriptEvaluate(script, keyArgs, valueArgs);

            string stateUpdateStatus = (string)results[0];
#if DebugTest
            int locker_DeletedKeysCount = (int)results[1];
            bool wasLockedByMe = (bool)results[2];
            bool wasLocked = (bool)results[3];

            DiagnosticEvents.OnCheckIn(workSessionId, documentId, lockerId, stateUpdateStatus, locker_DeletedKeysCount, wasLockedByMe, wasLocked);
#endif
            bool success = stateUpdateStatus == "OK";

            return success;
        }

        public static bool CheckOut(RedisConnection redisConnection, string lockerId, string workSessionId, RedisOfficeStateStorageSettings settings, out string state) {
            var keyBunch = new KeyNameHelper(workSessionId);

            string[] keyArgs = new string[] { keyBunch.LockerKeyName, keyBunch.StateKeyName };
            object[] valueArgs = new object[] { lockerId, settings.LockerTTL };

            string checkOutScript = @"
                local lockerKeyName = KEYS[1]
                local stateKeyName = KEYS[2]

                local lockerId = ARGV[1]
                local lockerTTL = ARGV[2]

                local wasLockedByAnother = false
                local state = ''

                updateATime(stateKeyName)

                if checkStateExists(stateKeyName) then

                    wasLockedByAnother = tryToLock(lockerKeyName, lockerId, lockerTTL)
                    
                    if not wasLockedByAnother then
                        state = redis.call('GET', stateKeyName)
                    end

                end

                local retValues = {} 
                retValues[1] = wasLockedByAnother
                retValues[2] = state
                return retValues
            ";

            var atimeUpdateScript = GetAccessTimeUpdateScript(settings);

            var results = (RedisResult[])(RedisResult)redisConnection.ScriptEvaluate(atimeUpdateScript + checkStateExists + tryToLock + checkOutScript, keyArgs, valueArgs);

            bool wasLockedByAnother = (bool)results[0];
            state = (string)results[1];
            if (state == "")
                state = null;


#if DebugTest
            DiagnosticEvents.OnCheckOut(workSessionId, lockerId, wasLockedByAnother);
#endif
            if(wasLockedByAnother)
                throw new CannotCheckoutStateCheckedOutByAnotherProcessException();

            return !wasLockedByAnother;
        }

        internal static bool AddCheckedOut(RedisConnection redisConnection, string lockerId, string workSessionId, string documentId, RedisOfficeStateStorageSettings settings) {
            var keyBunch = new KeyNameHelper(workSessionId, documentId);
            string[] keyArgs = new string[] { keyBunch.LockerKeyName, keyBunch.StateKeyName, keyBunch.DocumentIdToWorkSessionIdKeyName, keyBunch.WorkSessionIdToDocumentIdKeyName };
            object[] valueArgs = new object[] { lockerId, workSessionId, documentId, settings.LockerTTL };

            string addCheckedOutScript = @"
                local lockerKeyName = KEYS[1]
                local stateKeyName = KEYS[2]
                local documentIdToWorkSessionIdKeyName = KEYS[3]
                local workSessionIdToDocumentIdKeyName = KEYS[4]

                local lockerId = ARGV[1]
                local workSessionId = ARGV[2]
                local documentId = ARGV[3]
                local lockerTTL = ARGV[4]

                local stateUpdateStatus = 'failed'
                local docIdToStateIdUpdateStatus = 'failed'
                local stateIdToDocIdUpdateStatus  = 'failed'
                local wasLockedByAnother = false

                updateATime(stateKeyName)

                local documentIdSearch = getWorkSessionIdFromDocumentId(documentIdToWorkSessionIdKeyName)
                local sessionForThisDocumentIdFound = documentIdSearch[1];
                if not sessionForThisDocumentIdFound then

                    wasLockedByAnother = tryToLock(lockerKeyName, lockerId, lockerTTL)

                    local state = ''
                    if not wasLockedByAnother then
                        stateUpdateStatus = redis.call('SET', stateKeyName, state)
                        setExpired(stateKeyName)
                        if not (documentId == nil or documentId == '') then
                            docIdToStateIdUpdateStatus = redis.call('SET', documentIdToWorkSessionIdKeyName, workSessionId)
                            stateIdToDocIdUpdateStatus = redis.call('SET', workSessionIdToDocumentIdKeyName, documentId)
                            setExpired(documentIdToWorkSessionIdKeyName)
                            setExpired(workSessionIdToDocumentIdKeyName)
                        end
                    end
                end

                local retValues = {} 
                retValues[1] = sessionForThisDocumentIdFound
                retValues[2] = wasLockedByAnother

                retValues[3] = stateUpdateStatus
                retValues[4] = docIdToStateIdUpdateStatus
                retValues[5] = stateIdToDocIdUpdateStatus

                return retValues
            ";

            var expireScript = ExpireHelper.GetExpireScript(settings);
            var atimeUpdateScript = GetAccessTimeUpdateScript(settings);
            var script = expireScript + atimeUpdateScript + getWorkSessionIdFromDocumentId + tryToLock + addCheckedOutScript;
            var results = (RedisResult[])(RedisResult)redisConnection.ScriptEvaluate(script, keyArgs, valueArgs);

            var sessionForThisDocumentIdFound = (bool)results[0];
            var wasLockedByAnother = (bool)results[1];

            string stateUpdateStatus = (string)results[2];

#if DebugTest

            var docIdToStateIdUpdateStatus = (string)results[3];
            var stateIdToDocIdUpdateStatus = (string)results[4];

            DiagnosticEvents.OnAddCheckOut(workSessionId, documentId, lockerId, stateUpdateStatus, sessionForThisDocumentIdFound, wasLockedByAnother, docIdToStateIdUpdateStatus, stateIdToDocIdUpdateStatus);
#endif
            bool success = stateUpdateStatus == "OK";

            if(sessionForThisDocumentIdFound)
                throw new CannotAddWorkSessionThatAlreadyExistsException();

            return success;
        }

        internal static bool Remove(RedisConnection redisConnection, string lockerId, string workSessionId, RedisOfficeStateStorageSettings settings) {
            var keyBunch = new KeyNameHelper(workSessionId);

            string[] keyArgs = new string[] { keyBunch.LockerKeyName, keyBunch.StateKeyName };
            object[] valueArgs = new object[] { lockerId, workSessionId };

            const string removeScriptTemplate = @"
                local lockerKeyName = KEYS[1]
                local stateKeyName = KEYS[2]

                local lockerId = ARGV[1]
                local workSessionId = ARGV[2]
                local lockerTTL = 1

                local wasLockedByAnother = tryToLock(lockerKeyName, lockerId, lockerTTL)

                local state_DeletedKeysCount = 0
                local stateToDoc_DeletedKeysCount = 0
                local docToState_DeletedKeysCount = 0
                local locker_DeletedKeysCount = 0

                if not wasLockedByAnother then
                    state_DeletedKeysCount = redis.call('DEL', stateKeyName)
                    removeATime(stateKeyName)

                    local stateToDocKeyName = workSessionId .. '{0}'
                    local docId = redis.call('GET', stateToDocKeyName)
                    local docIdFound = not (type(docId) == 'boolean' and not docId)

                    stateToDoc_DeletedKeysCount = redis.call('DEL', stateToDocKeyName)
                    
                    if docIdFound then
                        local docToStateKeyName = docId .. '{1}'
                        docToState_DeletedKeysCount = redis.call('DEL', docToStateKeyName)
                    end

                    locker_DeletedKeysCount = redis.call('DEL', lockerKeyName)
                end

                local retValues = {{}} 
                retValues[1] = wasLockedByAnother
                retValues[2] = state_DeletedKeysCount
                retValues[3] = stateToDoc_DeletedKeysCount
                retValues[4] = docToState_DeletedKeysCount
                retValues[5] = locker_DeletedKeysCount
                
                return retValues
            ";
            var atimeRemoveScript = GetAccessTimeUpdateScript(settings);

            string removeScript = string.Format(atimeRemoveScript + tryToLock + removeScriptTemplate, KeyNameHelper.WSToDocPostfix, KeyNameHelper.DocToWSPostfix);

            var results = (RedisResult[])(RedisResult)redisConnection.ScriptEvaluate(removeScript, keyArgs, valueArgs);

            var wasLockedByAnother = (bool)results[0];
            var state_DeletedKeysCount = (int)results[1];
#if DebugTest
            var stateToDoc_DeletedKeysCount = (int)results[2];
            var docToState_DeletedKeysCount = (int)results[3];
            var locker_DeletedKeysCount = (int)results[4];
            DiagnosticEvents.OnRemove(workSessionId, lockerId, wasLockedByAnother, state_DeletedKeysCount, stateToDoc_DeletedKeysCount, docToState_DeletedKeysCount, locker_DeletedKeysCount);
#endif
            if(wasLockedByAnother)
                throw new CannotRemoveStateCheckedOutByAnotherProcessException();
            return true;
        }

        public static string FindWorkSessionId(RedisConnection redisConnection, string documentId) {
            var keyBunch = new KeyNameHelper("*", documentId);

            string[] keyArgs = new string[] { keyBunch.DocumentIdToWorkSessionIdKeyName };
            object[] valueArgs = new object[] { };
            
            const string findScript = @"
                return getWorkSessionIdFromDocumentId(KEYS[1])
            ";
            var results = (RedisResult[])(RedisResult)redisConnection.ScriptEvaluate(getWorkSessionIdFromDocumentId + findScript, keyArgs, valueArgs);

            bool workSessionIdFound = (bool)results[0];
            string workSessionId = (string)results[1];

#if DebugTest

            DiagnosticEvents.OnFind(documentId, workSessionIdFound, workSessionId);
#endif

            if (workSessionIdFound)
                return workSessionId;
            return null;
        }

        public static bool HasWorkSessionId(RedisConnection redisConnection, string workSessionId, RedisOfficeStateStorageSettings settings) {
            var keyBunch = new KeyNameHelper(workSessionId);

            string[] keyArgs = new string[] { keyBunch.StateKeyName };
            object[] valueArgs = new object[] { };

            string hasWorkSessionIdScript = @"
                local stateKeyName = KEYS[1]

                local status = redis.call('EXISTS', stateKeyName)
                local stateFound = status == 1

                local retValues = {} 
                retValues[1] = stateFound

                return retValues
            ";

            var results = (RedisResult[])(RedisResult)redisConnection.ScriptEvaluate(hasWorkSessionIdScript, keyArgs, valueArgs);

            bool stateFound = (bool)results[0];

#if DebugTest
            DiagnosticEvents.OnHasWorkSessionId(workSessionId, stateFound);
#endif

            return stateFound;
        }
        public static bool UndoCheckOut(RedisConnection redisConnection, string lockerId, string workSessionId, RedisOfficeStateStorageSettings settings) {
            var keyBunch = new KeyNameHelper(workSessionId);

            string[] keyArgs = new string[] { keyBunch.LockerKeyName, keyBunch.StateKeyName };
            object[] valueArgs = new object[] { lockerId };

            string undoCheckOutScript = @"
                local locker_DeletedKeysCount = 0
                local lockerKeyName = KEYS[1]
                local stateKeyName = KEYS[2]
                local lockerId = ARGV[1]

                updateATime(stateKeyName)

                local wasLockedBy = redis.call('GET', lockerKeyName)
                local wasLockedByMe = wasLockedBy == lockerId
                if wasLockedByMe then
                    locker_DeletedKeysCount = redis.call('DEL', lockerKeyName)
                end

                local retValues = {} 
                    retValues[1] = locker_DeletedKeysCount
                return retValues
            ";

            var accesstimeUpdateScript = GetAccessTimeUpdateScript(settings);

            var results = (RedisResult[])(RedisResult)redisConnection.ScriptEvaluate(accesstimeUpdateScript + undoCheckOutScript, keyArgs, valueArgs);
            int locker_DeletedKeysCount = (int)results[0];

#if DebugTest
            DiagnosticEvents.OnUndoCheckOut(workSessionId, lockerId, locker_DeletedKeysCount);
#endif
            var success = locker_DeletedKeysCount == 1;
            return success;
        }

        public static bool Set(RedisConnection redisConnection, string key, string value, RedisOfficeStateStorageSettings settings) {
            string[] keyArgs = new string[] { key };
            object[] valueArgs = new object[] { value };

            string stringGetScript = @"
                local keyName = KEYS[1]
                local value = ARGV[1]

                local status = redis.call('SET', keyName, value)
                setExpired(keyName)

                local retValues = {} 
                    retValues[1] = status
                return retValues
            ";

            var expireScript = ExpireHelper.GetExpireScript(settings);
            var script = expireScript + stringGetScript;
            var results = (RedisResult[])(RedisResult)redisConnection.ScriptEvaluate(script, keyArgs, valueArgs);

            string setStatus = (string)results[0];
            return setStatus == "OK";
        }
        public static string Get(RedisConnection redisConnection, string key, RedisOfficeStateStorageSettings settings) {
            string[] keyArgs = new string[] { key };
            object[] valueArgs = new object[] { };

            string stringGetScript = @"
                local keyName = KEYS[1]

                local value = redis.call('GET', keyName)
                setExpired(keyName)

                local retValues = {} 
                    retValues[1] = value
                return retValues
            ";

            var expireScript = ExpireHelper.GetExpireScript(settings);
            var script = expireScript + stringGetScript;
            var results = (RedisResult[])(RedisResult)redisConnection.ScriptEvaluate(script, keyArgs, valueArgs);

            var value = (string)results[0];
            return value;
        }

        static string GetAccessTimeUpdateScript(RedisOfficeStateStorageSettings settings) { 
            return AccessTimeUpdateScriptHelper.GetAccessTimeUpdateScript(settings);
        }
    }

    public static class ExpireHelper { 
        const string setExpireEnabled = @"
            local function setExpired(keyName) 
                redis.call('EXPIRE', keyName, {0})
            end";
        const string setExpireDisabled = @"
            local function setExpired(keyName) 
                redis.call('PERSIST', keyName)
            end";


        public static string GetExpireScript(RedisOfficeStateStorageSettings settings) { 
            bool expireEnabled = settings.StateTTL > 0;
            return expireEnabled ? string.Format(setExpireEnabled, settings.StateTTL) : 
                setExpireDisabled;

        }
    }

    public static class AccessTimeUpdateScriptHelper { 
        const string AccessTimeSortedSetName = "DXOfficeState_Atimes";

        static string GetCurrentUnixTimeStamp() { 
            var epoc = new DateTime(1970, 1, 1);
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(epoc)).TotalSeconds;
            return unixTimestamp.ToString();
        }

        const string disabledAccessTimeScript = @"
            local function removeATime(stateId)
            end
            local function updateATime(stateId) 
            end";

        const string AccessTimeScript = @"
            local function processATime(stateId, update) 
                local score = {0}
                local atimeSortedSet = '{1}'
                local stateATimeItemName = stateId

                redis.call('ZREM', atimeSortedSet, stateATimeItemName)
                if update then
                    redis.call('ZADD', atimeSortedSet, score, stateATimeItemName)
                end
            end
            local function removeATime(stateId)
                processATime(stateId, false)
            end
            local function updateATime(stateId) 
                processATime(stateId, true)
            end
            --[[
            local function getOldestNotUsedState()
                redis.call('ZRANGE', '{1}', 0, 0)
                return zrange '{1}' 0 0
            end
            ]]--
        ";

        public static string GetAccessTimeUpdateScript(RedisOfficeStateStorageSettings settings) {
            string score = GetCurrentUnixTimeStamp();
            var script = settings.TrackStateLastAccessTime ? AccessTimeScript : disabledAccessTimeScript;
            return string.Format(script, score, AccessTimeSortedSetName);
        }
    }

    public class KeyNameHelper {
        public const string WSToDocPostfix = "_WSToDoc";
        public const string DocToWSPostfix = "_DocToWS";
        const string StatePostfix = "_State";
        const string LockPostfix = "_Lock";

        public string StateKeyName { get; private set; }
        public string LockerKeyName { get; private set; }
        public string DocumentIdToWorkSessionIdKeyName { get; private set; }
        public string WorkSessionIdToDocumentIdKeyName { get; private set; }

        public KeyNameHelper(string workSessionId, string documentId) : this(workSessionId) {
            if (!string.IsNullOrEmpty(documentId)) {
                DocumentIdToWorkSessionIdKeyName = documentId + DocToWSPostfix;
                WorkSessionIdToDocumentIdKeyName = workSessionId + WSToDocPostfix;
            } else {
                DocumentIdToWorkSessionIdKeyName = "";
                WorkSessionIdToDocumentIdKeyName = "";
            }
        }
        public KeyNameHelper(string workSessionId) {
            StateKeyName = workSessionId + StatePostfix;
            LockerKeyName = workSessionId + LockPostfix;
        }
    }

}
