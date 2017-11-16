using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DevExpress.Web.OfficeAzureCommunication;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Azure.WebRole.Routing {

    public partial class _default : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
            DevExpress.Web.OfficeAzureCommunication.DiagnosticTools.WorkSessionServerView.Create(log, RoleEnvironment.CurrentRoleInstance.Id);
        }
    }
}