using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Web.OfficeAzureCommunication.Utils;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace DevExpress.Web.OfficeAzureCommunication {
    public static class WorkSessionMessenger {
        public static void SendMessage(MessageOperation operation, List<WorkSessionServerInfo> registeredServers) {
            var message = CreateMessage(operation, registeredServers);
            InterRoleCommunicator.SendMessage(message);
        }

        public static void SendMessage(MessageOperation operation, Guid workSessionID, string documentID) {
            var message = CreateMessage(operation, workSessionID, documentID);
            InterRoleCommunicator.SendMessage(message);
        }

        static Message CreateMessage(MessageOperation operation, List<WorkSessionServerInfo> registeredServers) {
            var message = new Message(
                RoleEnvironment.CurrentRoleInstance.Id,
                System.Environment.MachineName,
                NetUtils.GetLocalIPAddress(),
                operation,
                registeredServers
            );
            return message;
        }

        static Message CreateMessage(MessageOperation operation, Guid workSessionID, string documentID) {
            Message message = CreateMessage(operation, new List<WorkSessionServerInfo>());
            message.Sender.WorkSessions.GetOrAdd(workSessionID, new WorkSessionInfo(workSessionID, documentID));
            return message;
        }
    }
}
