using System;
using System.Configuration;
using Microsoft.Azure;
using Microsoft.WindowsAzure;

namespace DevExpress.Web.OfficeAzureCommunication.Utils {
    public static class ServiceConfigUtils {
        public static string GetAppSetting(string key) {
            return GetAppSetting(key, null);
        }
        public static string GetAppSetting(string key, string defaultValue) {
            string value = null;
            try {
                value = CloudConfigurationManager.GetSetting(key);
            } catch { }
        
            return value ?? defaultValue;
        }
    }
}
