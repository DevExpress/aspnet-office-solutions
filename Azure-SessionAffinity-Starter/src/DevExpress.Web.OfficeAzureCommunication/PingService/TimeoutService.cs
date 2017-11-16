using System;
using System.Web;

namespace DevExpress.Web.OfficeAzureCommunication {
    abstract class TimeoutService {
        const string timerCacheKey = "DevExpressASPNETOfficeAzureDocumentServerServiceTimer";

        bool active = false;

        protected abstract TimeoutServiceSettings Settings { get; }

        protected internal void StartInternal() {
            if(this.active) return;
            this.active = true;

            OnService();
        }

        protected internal void StopInternal() {
            this.active = false;
        }

        void SetServiceTimer(int interval) {
            SetServiceTimer(TimeSpan.FromSeconds(interval));
        }
        void SetServiceTimer(TimeSpan timeout) {
            if(!this.active) return;

            if(HttpRuntime.Cache[timerCacheKey] != null)
                return;

            HttpRuntime.Cache.Insert(
                timerCacheKey,
                string.Empty,
                null,
                System.Web.Caching.Cache.NoAbsoluteExpiration,
                timeout,
                System.Web.Caching.CacheItemPriority.NotRemovable,
                (key, value, reason) => OnService());
        }

        void OnService() {
            if(!this.active) return;
            
            OnServiceCore();
            
            SetServiceTimer(Settings.BroadcastInterval);
        }

        protected  abstract void OnServiceCore();
    }
}
