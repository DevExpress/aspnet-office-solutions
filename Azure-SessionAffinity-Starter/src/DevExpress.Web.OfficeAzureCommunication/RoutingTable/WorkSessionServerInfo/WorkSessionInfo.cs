using System;
using System.Runtime.Serialization;

namespace DevExpress.Web.OfficeAzureCommunication {
    [DataContract(Namespace = BroadcastNamespaces.DataContractNamespace)]
    [Serializable]
    public class WorkSessionInfo {
        [DataMember]
        public Guid WorkSessionID { get; set; }
        [DataMember]
        public string DocumentId { get; set; }
        [DataMember]
        public WorkSessionStatus Status { get; private set; }
        public long SenderTicks { get { return CreateTime.Ticks; } }
        public DateTime CreateTime { get; set; }
        public DateTime ProcessedTime { get; set; }

        public WorkSessionInfo(Guid workSessionID, string documentId) {
            WorkSessionID = workSessionID;
            DocumentId = documentId;
        }

        public void SetStatus(WorkSessionStatus status) {
            Status = status;
        }
    }
}
