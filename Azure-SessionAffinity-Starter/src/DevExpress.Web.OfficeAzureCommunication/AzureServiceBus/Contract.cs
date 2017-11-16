using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DevExpress.Web.OfficeAzureCommunication {
    [ServiceContract(Name = "MessageServiceContract", Namespace = BroadcastNamespaces.ServiceContractNamespace)]
    public interface IMessageServiceContract {
        [OperationContract(IsOneWay = true)]
        void Publish(Message msg);
    }

    public interface IMessageServiceChannel : IMessageServiceContract, IClientChannel { }

    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single, 
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class MessageService : IMessageServiceContract {
        private object locker = new object();

        public void Publish(Message msg) {
            lock(locker) {
                try {
                    MessageDispatcher.ProcessMessage(msg);
                } catch { }
            }
        }
    }
}
