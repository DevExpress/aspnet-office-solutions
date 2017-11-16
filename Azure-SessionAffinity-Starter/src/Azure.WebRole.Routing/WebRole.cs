using DevExpress.Web.OfficeAzureCommunication;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Routing {
    public class WebRole : RoleEntryPoint {
        public override bool OnStart() {
            WebRoleConfiguration.OnStart(RoleEnvironmentConfig.DocumentServerRoleName, RoleEnvironmentConfig.DocumentServerPort);
            return base.OnStart();
        }

        public override void OnStop() {
            WebRoleConfiguration.OnStop();
            base.OnStop();
        }

        public override void Run() {
            WebRoleConfiguration.TrackServerChanges();
        }
    }
}
