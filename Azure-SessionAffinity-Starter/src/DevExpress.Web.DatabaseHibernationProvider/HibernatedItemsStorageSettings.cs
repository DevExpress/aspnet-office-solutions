using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevExpress.Web.DatabaseHibernationProvider {
    public class HibernatedItemsStorageSettings {
        public string ConnectionString { get; private set; }
        public string TableName { get; private set; }
        public HibernationTableColumnNames ColumnNames { get; set; }
        public HibernatedItemsStorageSettings(string connectionString, string tableName) {
            ConnectionString = connectionString;
            TableName = tableName;
            ColumnNames = new HibernationTableColumnNames();
        }
    }

    public class HibernationTableColumnNames {
        const string DefaultWorkSessionId = "WorkSessionId";
        const string DefaultDocumentId = "DocumentId";
        const string DefaultHibernationTime = "HibernationTime";
        const string DefaultHeader = "Header";
        const string DefaultContent = "Content";

        protected string GetStringProperty(string propertyValue, string defaultValue) {
            if(!string.IsNullOrEmpty(propertyValue))
                return propertyValue;
            return defaultValue;
        }

        private string workSessionId;
        public string WorkSessionId {
            get {
                return GetStringProperty(workSessionId, DefaultWorkSessionId);
            }
            set {
                workSessionId = value;
            }
        }

        private string documentId;
        public string DocumentId {
            get {
                return GetStringProperty(documentId, DefaultDocumentId);
            }
            set {
                documentId = value;
            }
        }

        private string hibernationTime;
        public string HibernationTime {
            get {
                return GetStringProperty(hibernationTime, DefaultHibernationTime);
            }
            set {
                hibernationTime = value;
            }
        }

        private string header;
        public string Header {
            get {
                return GetStringProperty(header, DefaultHeader);
            }
            set {
                header = value;
            }
        }

        private string content;
        public string Content {
            get {
                return GetStringProperty(content, DefaultContent);
            }
            set {
                content = value;
            }
        }
        public HibernationTableColumnNames() { }
        internal List<string> GetAllColumns() {
            return new List<string>() { WorkSessionId, DocumentId, HibernationTime, Header, Content };
        }
    }
}
