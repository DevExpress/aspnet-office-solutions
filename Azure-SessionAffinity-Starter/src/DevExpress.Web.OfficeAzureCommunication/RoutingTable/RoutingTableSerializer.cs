using System.Collections.Generic;
using System.IO;

namespace DevExpress.Web.OfficeAzureCommunication {
    public static class RoutingTableSerializer {
        static string FilePath { get { return Path.Combine(Path.GetTempPath(), "ServerState.txt"); } }
        static void Serialize(RoutingTableStateDescriptor descriptor) {
            using(FileStream file = new FileStream(FilePath, FileMode.Create)) {
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(file, descriptor.Servers);
                file.Close();
            }
        }
        static RoutingTableStateDescriptor Deserialize() {
            RoutingTableStateDescriptor descriptor = new RoutingTableStateDescriptor();
            if(File.Exists(FilePath)) {
                using(FileStream file = new FileStream(FilePath, FileMode.Open, FileAccess.Read)) {
                    if(file.Length > 0) {
                        var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        descriptor.Servers.AddRange((List<WorkSessionServerInfo>)formatter.Deserialize(file));
                    }
                    file.Close();
                }
            }
            return descriptor;
        }
        public static void UpdateCache() {
            Serialize(new RoutingTableStateDescriptor(RoutingTable.GetWorkSessionServerInstances()));
        }
        public static void RestoreDataFromCache() {
            RoutingTableStateDescriptor descriptor = Deserialize();
            RoutingTable.AddWorkSessionServers(descriptor.Servers);
        }
    }

    public class RoutingTableStateDescriptor {
        public List<WorkSessionServerInfo> Servers { get; }
        public RoutingTableStateDescriptor() {
            Servers = new List<WorkSessionServerInfo>();
        }
        public RoutingTableStateDescriptor(List<WorkSessionServerInfo> servers) : this() {
            Servers.AddRange(servers);
        }
    }
}
