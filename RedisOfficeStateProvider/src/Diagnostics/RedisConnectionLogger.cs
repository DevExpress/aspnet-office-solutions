using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevExpress.Web.RedisOfficeStateProvider.Diagnostics {

    public class RedisConnectionLogger : TextWriter {
        public override Encoding Encoding => Encoding.ASCII;

        event RedisConnectionLogEventHandler redisConnectionMessageReceived;
        public event RedisConnectionLogEventHandler RedisConnectionMessageReceived {
            add { redisConnectionMessageReceived += value; }
            remove { redisConnectionMessageReceived -= value; }
        }

        public override void WriteLine(string value) {
            base.WriteLine(value);
            RaiseMessageReceived(value);
        }
        public override void WriteLine(string format, object arg0) {
            base.WriteLine(format, arg0);
            RaiseMessageReceived(string.Format(format, arg0));
        }
        public override void WriteLine(string format, object arg0, object arg1) {
            base.WriteLine(format, arg0, arg1);
            RaiseMessageReceived(string.Format(format, arg0, arg1));
        }
        public override void WriteLine(string format, object arg0, object arg1, object arg2) {
            base.WriteLine(format, arg0, arg1, arg2);
            RaiseMessageReceived(string.Format(format, arg0, arg1, arg2));
        }
        public override void Write(string format, params object[] arg) {
            base.Write(format, arg);
            RaiseMessageReceived(string.Format(format, arg));
        }

        void RaiseMessageReceived(string logMessage){ 
            redisConnectionMessageReceived?.Invoke(logMessage);
        }
    }
}
