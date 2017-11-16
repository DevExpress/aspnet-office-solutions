using System;
using System.Web;
using DevExpress.Web;
using DevExpress.Web.OfficeAzureCommunication;
using DevExpress.Web.ASPxRichEdit;
using DevExpress.Web.ASPxSpreadsheet;
using DevExpress.Web.Office;
using DB.Documents.DAL;

namespace DocumentSite {
    public partial class _Default : System.Web.UI.Page {
        protected DocumentItemInfo DocumentViewModel {
            get {
                return new DocumentItemInfo(this.Page.GetDataItem() as Item);
            }
        }

        protected void Page_Init(object sender, EventArgs e) {
            ProcessRequest(HttpContext.Current.Request);
        }

        protected void Page_Load(object sender, EventArgs e) {
            var unitOfWork = new DocumentsUnitOfWork();

            Repeater1.DataSource = unitOfWork.ItemRepository.Get();
            Repeater1.DataBind();
        }

        private void ProcessRequest(HttpRequest httpRequest) {
            if(DocumentRequestParams.RedirectRequired) { 
                var workSessionInfo = RoutingTable.GetWorkSessionInfoByDocumentID(DocumentRequestParams.DocumentID);
                var workSessionID = workSessionInfo != null ? workSessionInfo.WorkSessionID : Guid.Empty;
                if(workSessionID ==  Guid.Empty)
                    workSessionID = OpenDocumentID(DocumentRequestParams.DocumentID);
                if(workSessionID !=  Guid.Empty)
                    Response.Redirect(DocumentRequestParams.GetRedirectUrl(workSessionID.ToString()), true);
            }
        }

        private Guid OpenDocumentID(string documentID) {
            var unitOfWork = new DocumentsUnitOfWork();

            var documentItem = unitOfWork.ItemRepository.GetByID(long.Parse(documentID));
            var documentItemInfo = new DocumentItemInfo(documentItem);
            
            var editorType = documentItemInfo.DocumentProcessorType;
            OfficeWorkSessionControl editor = null;

            if(editorType == OfficeDocumentProcessorType.Spreadsheet) { 
                var spreadsheetControl = new ASPxSpreadsheet();
                spreadsheetControl.Open(
                    documentItemInfo.Id, 
                    (DevExpress.Spreadsheet.DocumentFormat)DocumentFormatUtils.GetDocumentFormat(documentItemInfo.NameExtension), 
                    () => documentItem.Content.Data);

                editor = spreadsheetControl;
            }

            if(editorType == OfficeDocumentProcessorType.RichEdit) { 
                var richEditControl = new ASPxRichEdit();
                richEditControl.Open(
                    documentItemInfo.Id, 
                    (DevExpress.XtraRichEdit.DocumentFormat)DocumentFormatUtils.GetDocumentFormat(documentItemInfo.NameExtension), 
                    () => documentItem.Content.Data);

                editor = richEditControl;
            }

            return editor.GetWorkSessionID();
        }
    }
}