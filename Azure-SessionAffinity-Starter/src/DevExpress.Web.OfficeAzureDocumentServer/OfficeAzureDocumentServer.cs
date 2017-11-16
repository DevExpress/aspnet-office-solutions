using DevExpress.Web.OfficeAzureCommunication;
using DevExpress.Web.Office.Internal;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace DevExpress.Web.OfficeAzureDocumentServer {
    public static class OfficeAzureDocumentServer {
        public static void Init() {
            InterRoleCommunicator.Initialize();
            InitControlsConjunction();
            InitServerCommunication();
        }

        static void InitControlsConjunction() {
            WorkSessionAdminTools.Created += WorkSessionAdminTools_Created;
            WorkSessionAdminTools.Disposed += WorkSessionAdminTools_Disposed;
            WorkSessionAdminTools.AutoSaved += WorkSessionAdminTools_AutoSaved;
            WorkSessionAdminTools.Hibernated += WorkSessionAdminTools_Hibernated;
            WorkSessionAdminTools.WokeUp += WorkSessionAdminTools_WokeUp;
        }

        static void WorkSessionAdminTools_WokeUp(IWorkSession workSession, DocumentDiagnosticEventArgs e) {
            WorkSessionMessenger.SendMessage(MessageOperation.WakeUpWorkSession, workSession.ID, workSession.DocumentInfo.DocumentId);
            #if DEBUG
            Log(workSession, "WokenUp");
            #endif
        }

        static void WorkSessionAdminTools_Hibernated(IWorkSession workSession, DocumentDiagnosticEventArgs e) {
            WorkSessionMessenger.SendMessage(MessageOperation.HibernateWorkSession, workSession.ID, workSession.DocumentInfo.DocumentId);
            #if DEBUG
            Log(workSession, "Hibernated");
            #endif
        }

        static void WorkSessionAdminTools_AutoSaved(IWorkSession workSession, DocumentDiagnosticEventArgs e) {
            WorkSessionMessenger.SendMessage(MessageOperation.AutoSaveWorkSession, workSession.ID, workSession.DocumentInfo.DocumentId);
            #if DEBUG
            Log(workSession, "AutoSaved");
            #endif
        }

        static void WorkSessionAdminTools_Created(IWorkSession workSession, DocumentDiagnosticEventArgs e) {
            WorkSessionMessenger.SendMessage(MessageOperation.AddWorkSession, workSession.ID, workSession.DocumentInfo.DocumentId);
            #if DEBUG
            Log(workSession, "Add");
            #endif
        }

        static void WorkSessionAdminTools_Disposed(IWorkSession workSession, DocumentDiagnosticEventArgs e) {
            WorkSessionMessenger.SendMessage(MessageOperation.RemoveWorkSession, workSession.ID, workSession.DocumentInfo.DocumentId);
            #if DEBUG
            Log(workSession, "Remove");
            #endif
        }
        
        #if DEBUG
        private static void Log(IWorkSession workSession, string eventName) {
            OfficeAzureCommunication.Diagnostic.Logger.Log(string.Format("WorkSessionRegisterer: {0} {1} {2}", eventName, workSession.ID, workSession.DocumentInfo.DocumentId));
        }
        #endif

        static void InitServerCommunication() {
            RoutingTable.ServerRemoved += OnServerRemoved;
        }

        private static void OnServerRemoved(WorkSessionServerInfo server) {
            if(server.RoleInstanceId == RoleEnvironment.CurrentRoleInstance.Id) {
                ShutDown();
            }
        }

        static void ShutDown() {
            InterRoleCommunicator.ShutDown();
            DevExpress.Web.Office.DocumentManager.HibernateAllDocuments();
        }
    }
}
