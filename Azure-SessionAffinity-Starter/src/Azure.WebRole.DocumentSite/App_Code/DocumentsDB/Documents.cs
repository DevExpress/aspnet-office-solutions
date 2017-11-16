using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using DB.Documents.DAL;

namespace DocumentSite {

    public class DocumentsUnitOfWork : UnitOfWork {
        public DocumentsUnitOfWork() : base(DocumentsConnectionStrings.Current) { }
    }

    public class DocumentItemInfo {
        Item item;
        public DocumentItemInfo(Item item) {
            this.item = item;
        }

        public string Id { get { return item.Id.ToString(); } }
        public string Name { get { return item.IsRoot ? "" : item.Name; } }
        public bool IsFolder { get { return item.IsFolder; } }
        public string OnClick { get { return GetDocumentEditorRequestUrl(Id); } }
        
        public OfficeDocumentProcessorType DocumentProcessorType { get { return DocumentFormatUtils.GetDocumentProcessorType(NameExtension); } }
        public Object DocumentFormat { get { return DocumentFormatUtils.GetDocumentProcessorType(NameExtension); } }

        internal string NameExtension { get { return System.IO.Path.GetExtension(Name).Replace(".", ""); } }

        string GetDocumentEditorRequestUrl(string documentID) {
            return string.Format("~/Default.aspx?editor={0}&docId={1}",
                DocumentProcessorPages.GetDocumentProcessorPage(DocumentProcessorType),
                HttpContext.Current.Server.UrlEncode(documentID));
        }
    }

    public enum OfficeDocumentProcessorType { Unknown, RichEdit, Spreadsheet }

    public class DocumentFormatContainer {
        public OfficeDocumentProcessorType DocumentProcessorType { get; private set; }
        public object DocumentFormat { get; private set; }

        public DocumentFormatContainer(OfficeDocumentProcessorType documentProcessorType, object documentFormat) {
            DocumentProcessorType = documentProcessorType;
            DocumentFormat = documentFormat;
        }
    }

    public static class DocumentFormatUtils {
        static Dictionary<string, DocumentFormatContainer> register = new Dictionary<string, DocumentFormatContainer>();
        static void Register(string extension, OfficeDocumentProcessorType documentProcessorType, object documentFormat) {
            register.Add(extension, new DocumentFormatContainer(documentProcessorType, documentFormat));
        }

        static DocumentFormatUtils() {
            Register("rtf",  OfficeDocumentProcessorType.RichEdit,   DevExpress.XtraRichEdit.DocumentFormat.Rtf);
            Register("doc",  OfficeDocumentProcessorType.RichEdit,   DevExpress.XtraRichEdit.DocumentFormat.Doc);
            Register("docx", OfficeDocumentProcessorType.RichEdit,   DevExpress.XtraRichEdit.DocumentFormat.OpenXml);
            Register("txt",  OfficeDocumentProcessorType.RichEdit,   DevExpress.XtraRichEdit.DocumentFormat.PlainText);
            Register("xlsx", OfficeDocumentProcessorType.Spreadsheet, DevExpress.Spreadsheet.DocumentFormat.Xlsx);
        }

        public static OfficeDocumentProcessorType GetDocumentProcessorType(string documentFileNameExtension) {
            var documentFormatContainer = GetFormatContainer(documentFileNameExtension);
            return documentFormatContainer != null ?  documentFormatContainer.DocumentProcessorType : OfficeDocumentProcessorType.Unknown;
        }

        public static object GetDocumentFormat(string documentFileNameExtension) {
            var documentFormatContainer = GetFormatContainer(documentFileNameExtension);
            return documentFormatContainer != null ?  documentFormatContainer.DocumentFormat : null;
        }

        static DocumentFormatContainer GetFormatContainer(string documentFileNameExtension) { 
            DocumentFormatContainer formatContainer;
            return register.TryGetValue(documentFileNameExtension, out formatContainer) ? formatContainer : null;
        }

    }

    public static class DocumentProcessorPages {
        static Dictionary<OfficeDocumentProcessorType, string> documentProcessorPages = new Dictionary<OfficeDocumentProcessorType, string>();

        static DocumentProcessorPages() {
            documentProcessorPages.Add(OfficeDocumentProcessorType.Spreadsheet, "Spreadsheet");
            documentProcessorPages.Add(OfficeDocumentProcessorType.RichEdit, "RichEdit");
            documentProcessorPages.Add(OfficeDocumentProcessorType.Unknown, "Unknown");
        }

        public static string GetDocumentProcessorPage(OfficeDocumentProcessorType documentProcessorType) {
            return documentProcessorPages[documentProcessorType];
        }
    }

    public static class DocumentRequestParams {
        const string DucumentIDKey = "docId";
        const string WorkSessionIDKey = "dxwsid";
        const string EditorKey = "editor";

        public static string DocumentID { get { return QueryString[DucumentIDKey]; } }
        public static string WorkSessionID { get { return QueryString[WorkSessionIDKey]; } }
        public static string EditorPageUrl { get { return QueryString[EditorKey]; } }

        public static bool IsOpeningByDocumentId {
            get { return !string.IsNullOrEmpty(DocumentID) && string.IsNullOrEmpty(WorkSessionID); }
        }
        public static bool IsOpeningByWorkSessionID {
            get { return string.IsNullOrEmpty(DocumentID) && !string.IsNullOrEmpty(WorkSessionID); }
        }
        public static bool RedirectRequired { get { return !string.IsNullOrEmpty(EditorPageUrl) && IsOpeningByDocumentId; } }

        public static string GetRedirectUrl(string workSessionID) { 
            return string.Format("~/{0}.aspx?{1}={2}", EditorPageUrl, WorkSessionIDKey, workSessionID); 
        }
        static NameValueCollection QueryString { get { return HttpContext.Current.Request.QueryString; } }
    }

}