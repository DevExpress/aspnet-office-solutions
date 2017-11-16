using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Web.Office.Internal;

namespace DevExpress.Web.DatabaseHibernationProvider {
    public class HibernatedItem {
        public Guid WorkSessionId { get; set; }
        public string DocumentId { get; set; }
        public DateTime HibernationTime { get; set; }
        public byte[] Header { get; set; }
        public byte[] Content { get; set; }

        public HibernatedItem() { }

        public HibernatedItem(Guid workSessionId) {
            WorkSessionId = workSessionId;
        }

        public HibernatedItem(Guid workSessionId, string documentId, DateTime hibernationTime, byte[] header, byte[] content) : this(workSessionId) {
            DocumentId = documentId;
            HibernationTime = hibernationTime;
            Header = header;
            Content = content;
        }
        public void CheckOut(HibernatedItemsStorageSettings storageSettings, Guid workSessionId) {
            HibernatedItemsStorage storage = new HibernatedItemsStorage(storageSettings);
            storage.GetItemByWorkSessionId(workSessionId);
        }
    }
}
