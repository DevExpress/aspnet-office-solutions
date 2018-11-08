using System;
using Microsoft.WindowsAzure;
using Microsoft.Azure;
using DevExpress.Web.DatabaseHibernationProvider;

namespace Hibernation {
    public class HibernationStorageSettingsFromWebConfig : HibernatedItemsStorageSettings {
        public HibernationStorageSettingsFromWebConfig() : base(HibernationSettings.ConnectionString, HibernationSettings.TableName){
            ColumnNames.WorkSessionId = HibernationSettings.WorkSessionIdColumnName;
            ColumnNames.DocumentId = HibernationSettings.DocumentIdColumnName;
            ColumnNames.HibernationTime = HibernationSettings.HibernationTimeColumnName;
            ColumnNames.Header = HibernationSettings.HeaderColumnName;
            ColumnNames.Content = HibernationSettings.ContentColumnName;
        }
    }
    public static class HibernationSettings {
        const int DefaultHibernateTimeout = 30;
        const int DefaultHibernatedDocumentsDisposeTimeout = 24 * 60;

        static string CONNECTION_STRING_KEY = "HibernationConnectionString";
        static string TABLE_NAME_KEY = "HibernationTableName";
        static string WORKSESSIONID_COLUMN_NAME = "HibernationTableWorkSessionIdColumnName";
        static string DOCUMENTID_COLUMN_NAME = "HibernationTableDocumentIdColumnName";
        static string HIBERNATION_TIME_COLUMN_NAME = "HibernationTableHibernationTimeColumnName";
        static string HEADER_COLUMN_NAME = "HibernationTableHeaderColumnName";
        static string CONTENT_COLUMN_NAME = "HibernationTableContentColumnName";

        public static string ConnectionString {
            get { return CloudConfigurationManager.GetSetting(CONNECTION_STRING_KEY); }
        }

        public static string TableName {
            get { return CloudConfigurationManager.GetSetting(TABLE_NAME_KEY); }
        }

        public static string WorkSessionIdColumnName {
            get { return CloudConfigurationManager.GetSetting(WORKSESSIONID_COLUMN_NAME); }
        }

        public static string DocumentIdColumnName {
            get { return CloudConfigurationManager.GetSetting(DOCUMENTID_COLUMN_NAME); }
        }

        public static string HibernationTimeColumnName {
            get { return CloudConfigurationManager.GetSetting(HIBERNATION_TIME_COLUMN_NAME); }
        }

        public static string HeaderColumnName {
            get { return CloudConfigurationManager.GetSetting(HEADER_COLUMN_NAME); }
        }

        public static string ContentColumnName {
            get { return CloudConfigurationManager.GetSetting(CONTENT_COLUMN_NAME); }
        }

        public static TimeSpan HibernateTimeout {
            get {
                int timeoutValue = GetNumericValue("HibernateTimeout", DefaultHibernateTimeout);
                return TimeSpan.FromMinutes(timeoutValue);
            }
        }

        public static TimeSpan HibernatedDocumentsDisposeTimeout {
            get {
                int timeoutValue = GetNumericValue("HibernatedDocumentsDisposeTimeout", DefaultHibernatedDocumentsDisposeTimeout);
                return TimeSpan.FromMinutes(timeoutValue);
            }
        }

        static int GetNumericValue(string settingName, int defaultValue) {
            string settingValue = CloudConfigurationManager.GetSetting(settingName);
            int numericValue;
            if(int.TryParse(settingValue, out numericValue))
                return numericValue;
            else
                return defaultValue;
        }
    }
}