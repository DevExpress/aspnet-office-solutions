using Microsoft.WindowsAzure.ServiceRuntime;

namespace DocumentSite {
    public class WebRole : RoleEntryPoint {
        public override bool OnStart() {
            WebRoleConfiguration.OnStart();
            return base.OnStart();
        }

        public override void OnStop() {
            WebRoleConfiguration.OnStop();
            base.OnStop();
        }
    }
}
