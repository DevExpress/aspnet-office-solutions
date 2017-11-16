using System;
using DevExpress.Web.OfficeAzureDocumentServer;
using DevExpress.Web.DatabaseHibernationProvider;
using Hibernation;

namespace DocumentSite {
    public class Global : System.Web.HttpApplication {
        protected void Application_Start(object sender, EventArgs e) {
            OfficeAzureDocumentServer.Init();
            HibernationInit();
        }

        private void HibernationInit() {
            DevExpress.Web.Office.Internal.WorkSessionProcessing.SetWorkSessionHibernateProvider(new DatabaseWorksessionHibernationProvider(new HibernationStorageSettingsFromWebConfig()));
            DevExpress.Web.Office.DocumentManager.HibernateTimeout = HibernationSettings.HibernateTimeout;
            DevExpress.Web.Office.DocumentManager.HibernatedDocumentsDisposeTimeout = HibernationSettings.HibernatedDocumentsDisposeTimeout;
            DevExpress.Web.Office.DocumentManager.HibernateAllDocumentsOnApplicationEnd = true;
            DevExpress.Web.Office.DocumentManager.EnableHibernation = true;
        }
    }
}