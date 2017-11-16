using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DevExpress.Spreadsheet.Functions;
using DevExpress.Web.Office;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace DocumentSite {
    public partial class Spreadsheet : System.Web.UI.Page {
        protected void Page_Init(object sender, EventArgs e) {
            RibbonHelper.HideFileTab(ASPxSpreadsheet1);
            if(DocumentRequestParams.IsOpeningByWorkSessionID)
                (ASPxSpreadsheet1 as OfficeWorkSessionControl).AttachToWorkSession(Guid.Parse(DocumentRequestParams.WorkSessionID));
        }
    }
}