using System;
using System.Threading;

namespace DevExpress.Web.SqlOfficeStateProvider {

    public static class TryRepeater {

        public static int TryCount { get; set; }
        public static int TryInterval { get; set; }

        static TryRepeater() {
            TryCount = 10;
            TryInterval = 1000;
        }

        public static object Do(Func<object> task) {
            for (int tryIndex = 0; tryIndex < TryCount; tryIndex++) {
                bool lastTry = tryIndex == TryCount - 1;
                try {
                    return task();
                } catch (Exception e) {
                    if (lastTry || e is IDoNotRetryRedisOfficeException)
                        throw e;
                    Thread.Sleep(TryInterval);
                }
            }
            return null;
        }
    }

}
