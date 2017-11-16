using System.Collections.Generic;
using System.Linq;

namespace DevExpress.Web.OfficeAzureRoutingServer {
    public static class FarmConfigurationManager {
        static object locker = new object();

        public static void SetUpFarm() {
            lock(locker) {
                try {
                    IISWebFarmConfigurationUtils.SetUp(WebFarmConfiguration.FarmName);
                } catch { }
            }
        }

        public static void RegisterServer(string serverAddress, int port) {
            AddServer(serverAddress, port);
        }

        public static void UpdateServersRegistration(IEnumerable<string> serverAddresses, int port) {
            AddNewServers(serverAddresses, port);
            RemoveOfflineServers(serverAddresses, port);            
        }

        static void AddNewServers(IEnumerable<string> serverAddresses, int port) {
            IEnumerable<string> serversOnFarm = GetServersFromFarmConfiguration();
            var newServers = serverAddresses.Except(serversOnFarm);
            AddServers(newServers, port);
        }

        public static void RemoveOfflineServers(IEnumerable<string> serverAddresses, int port) {
            IEnumerable<string> serversOnFarm = GetServersFromFarmConfiguration();
            var offlineServers = serversOnFarm.Except(serverAddresses);
            RemoveServers(offlineServers);
        }

        static IEnumerable<string> GetServersFromFarmConfiguration() {
            lock(locker) {
                var farmName = WebFarmConfiguration.FarmName;
                return IISWebFarmConfigurationUtils.GetServersFromConfiguration(farmName);
            }
        }

        public static void AddServers(IEnumerable<string> serverAddresses, int port) {
            lock(locker) {
                var farmName = WebFarmConfiguration.FarmName;
                foreach(var serverAddress in serverAddresses) {
                    IISWebFarmConfigurationUtils.AddServer(farmName, serverAddress, port);
                }
            }
        }

        public static void AddServer(string serverAddress, int port) {
            lock(locker) {
                var farmName = WebFarmConfiguration.FarmName;
                IISWebFarmConfigurationUtils.AddServer(farmName, serverAddress, port);
            }
        }

        public static void RemoveServers(IEnumerable<string> servers) {
            lock(locker) {
                var farmName = WebFarmConfiguration.FarmName;
                IISWebFarmConfigurationUtils.RemoveServers(farmName, servers);
            }
        }
    }
}
