using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using DevExpress.Web.RedisOfficeStateProvider.Diagnostics;

namespace DevExpress.Web.RedisOfficeStateProvider {

    public class RedisConnection {
        ConnectionMultiplexer connectionMultiplexer;

        IDatabase database;
        public IDatabase Database { get { return database; } }

        public RedisConnection(string connectionString) {
            if (Diagnostics.DiagnosticEvents.ConnectionLogger == null) {
                connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
            } else {
                connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString, Diagnostics.DiagnosticEvents.ConnectionLogger);
            }

            database = connectionMultiplexer.GetDatabase();
        }

        public object ScriptEvaluate(string script, string[] keyArgs, object[] valueArgs) {
            RedisKey[] redisKeyArgs = GetRedisKeys(keyArgs);
            RedisValue[] redisValueArgs = GetRedisValues(valueArgs);

            return database.ScriptEvaluate(script, redisKeyArgs, redisValueArgs);
        }

        private static RedisKey[] GetRedisKeys(string[] keyArgs) {
            RedisKey[] redisKeyArgs = new RedisKey[keyArgs.Length];
            
            int i = 0;
            foreach (string key in keyArgs) {
                redisKeyArgs[i] = key;
                i++;
            }

            return redisKeyArgs;
        }

        private static RedisValue[] GetRedisValues(object[] valueArgs) {
            RedisValue[] redisValueArgs = new RedisValue[valueArgs.Length];

            int i = 0;
            foreach (object val in valueArgs) {
                if (val.GetType() == typeof(byte[]))
                    redisValueArgs[i] = (byte[])val;
                else
                    redisValueArgs[i] = val.ToString();
                i++;
            }

            return redisValueArgs;
        }

    }
}
