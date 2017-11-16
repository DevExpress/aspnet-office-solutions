using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace DocumentSite.Diagnostic {
    public partial class Diagnostic : System.Web.UI.UserControl {
        protected void Page_Init(object sender, EventArgs e) {
            LabelInstanceID.Text = RoleEnvironment.CurrentRoleInstance.Id;
            LabelMachineName.Text = Environment.MachineName;
        }
    }
}