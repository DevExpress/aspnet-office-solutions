using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Web.Office.Internal;

namespace DevExpress.Web.DatabaseHibernationProvider {
    public class DatabaseWorksessionHibernationProvider : WorkSessionHibernationProviderBase {
        public DatabaseWorksessionHibernationProvider(HibernatedItemsStorageSettings settings) : base(new DatabaseOfficeWorkSessionStorage(settings)) {
        }
    }
}
