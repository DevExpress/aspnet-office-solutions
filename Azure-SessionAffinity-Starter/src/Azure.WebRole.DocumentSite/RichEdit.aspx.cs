using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DevExpress.Web.Office;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace DocumentSite {
    public partial class RichEdit : System.Web.UI.Page {
        protected void Page_Init(object sender, EventArgs e) {
            RibbonHelper.HideFileTab(ASPxRichEdit1);
            if(DocumentRequestParams.IsOpeningByWorkSessionID)
                (ASPxRichEdit1 as OfficeWorkSessionControl).AttachToWorkSession(Guid.Parse(DocumentRequestParams.WorkSessionID));
        }
    }
}