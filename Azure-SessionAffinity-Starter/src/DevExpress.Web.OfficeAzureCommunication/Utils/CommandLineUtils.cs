using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevExpress.Web.OfficeAzureCommunication.Utils {
    public static class CommandLineUtils {
        public static void CmdExecute(string command) {
            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;
            Process process = Process.Start(processInfo);
            process.WaitForExit();
        }
    }
}
