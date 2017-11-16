using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Web.Office;
using DevExpress.Web.Office.Internal;

namespace DevExpress.Web.DatabaseHibernationProvider {
    public class DatabaseOfficeWorkSessionStorage : IWorkSessionHibernationStorage {
        HibernatedItemsStorage CurrentHibernationStorage { get; set; }
        public DatabaseOfficeWorkSessionStorage(HibernatedItemsStorageSettings settings) {
            CurrentHibernationStorage = new HibernatedItemsStorage(settings);
        }
        public void CheckIn(WorkSessionStateContainer hibernationContainer, Guid workSessionId) {
            HibernatedItem item = new HibernatedItem(workSessionId, hibernationContainer.Descriptor.DocumentPathOrID, DateTime.Now, 
                WorkSessionStateDatabaseSerializer.Serialize(hibernationContainer.Descriptor), WorkSessionStateDatabaseSerializer.Serialize(hibernationContainer.Chamber));
            CurrentHibernationStorage.CheckIn(item);
        }

        public WorkSessionStateContainer CheckOut(Guid workSessionId) {
            return CheckOut(workSessionId, false);
        }

        protected WorkSessionStateContainer LoadHibernationContainerDescriptor(Guid workSessionId) {
            return CheckOut(workSessionId, true);
        }

        protected WorkSessionStateContainer CheckOut(Guid workSessionId, bool descriptorOnly) {
            HibernatedItem item = CurrentHibernationStorage.CheckOut(workSessionId);
            WorkSessionStateContainer container = null;
            if(item != null) {
                container = WorkSessionStateDatabaseSerializer.Deserialize(item, descriptorOnly);
                CurrentHibernationStorage.DeleteItem(workSessionId);
            }
            return container;
        }

        public void DisposeOutdatedHibernationContainers() {
            var itemExpirationTime = DateTime.Now - DocumentManager.HibernatedDocumentsDisposeTimeout;
            CurrentHibernationStorage.DeleteExpiredItems(itemExpirationTime);
        }

        public bool IsHibernationStorageValid(string hibernationStorage) {
            return CurrentHibernationStorage.IsStorageValid();
        }

        public bool HasWorkSessionId(Guid workSessionId) {
            return CurrentHibernationStorage.HasItem(workSessionId);
        }

        public void Remove(Guid currentWorkSessionID) {
            CurrentHibernationStorage.DeleteItem(currentWorkSessionID);
        }

        public WorkSessionStateContainer WakeUp(Guid workSessionId) {
            return CheckOut(workSessionId, false);
        }

        public void Hibernate(WorkSessionStateContainer hibernationContainer, Guid workSessionId) {
            CheckIn(hibernationContainer, workSessionId);
        }

        public Guid FindWorkSessionId(string documentPathOrId) {
            return CurrentHibernationStorage.FindWorkSessionId(documentPathOrId);
        }
    }


    public static class WorkSessionStateDatabaseSerializer {
        public static byte[] Serialize(object serializableObject) {
            using(MemoryStream stream = new MemoryStream()) {
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    formatter.Serialize(stream, serializableObject);
                return stream.ToArray();
            }
        }
        public static WorkSessionStateContainer Deserialize(HibernatedItem item, bool descriptorOnly) {
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            HibernationChamberDescriptor header = (HibernationChamberDescriptor)formatter.Deserialize(new MemoryStream(item.Header));
            HibernationChamber body = descriptorOnly ? null : (HibernationChamber)formatter.Deserialize(new MemoryStream(item.Content));

            return new WorkSessionStateContainer(header, body);
        }
    }
}
