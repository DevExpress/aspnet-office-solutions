using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DevExpress.Web.OfficeAzureCommunication;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace DocumentSite {
    public partial class Default : System.Web.UI.Page {
        #if DEBUG
        protected void Page_Load(object sender, EventArgs e) {
            CreateDiagnosticPage();
        }

        private void CreateDiagnosticPage() {
            DevExpress.Web.OfficeAzureCommunication.Diagnostic.WorkSessionServerView.CreateDianosticPageControl(log, RoleEnvironment.CurrentRoleInstance.Id);
        }

        #endif

    }
}