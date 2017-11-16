using System.Collections.Generic;
using Microsoft.Web.Administration;
using ConfigurationElementAttributes = System.Collections.Generic.Dictionary<string, string>;

namespace DevExpress.Web.OfficeAzureRoutingServer {
    class IISWebFarmConfigurationUtils {
        const string WEB_FARMS_TAGNAME = "webFarms";
        const string WEB_FARM_TAGNAME = "webFarm";

        internal static ConfigurationElement SetUp(string farmName, bool createUrlRewriteRule = true, bool useClientAffinity = true) {
            using(ServerManager manager = new ServerManager()) {
                ConfigurationElementCollection webFarmsConfiguration = GetWebFarmsConfiguration(manager);
                ConfigurationElement webFarm = GetWebFarmConfiguration(webFarmsConfiguration, farmName);
                if(webFarm == null) {
                    webFarm = webFarmsConfiguration.CreateElement(WEB_FARM_TAGNAME);
                    webFarm["name"] = farmName;
                    webFarmsConfiguration.Add(webFarm);
                    manager.CommitChanges();

                    if(createUrlRewriteRule)
                        IISUrlRewriterConfigurationUtils.CreateURLRewriteRule(farmName);
                    SetApplicationRequestRoutingParameters(farmName, useClientAffinity);
                }
                return webFarm;
            }
        }

        static void SetApplicationRequestRoutingParameters(string farmName, bool useClientAffinity) {
            using(ServerManager manager = new ServerManager()) {
                ConfigurationElementCollection webFarmsConfiguration = GetWebFarmsConfiguration(manager);
                ConfigurationElement webFarm = GetWebFarmConfiguration(webFarmsConfiguration, farmName);
                if(webFarm != null) {
                    ConfigurationElement connectionConfiguration = webFarm.GetChildElement("applicationRequestRouting");
                    if(useClientAffinity)
                        SetAffinityParameters(connectionConfiguration);
                    SetHealthCheckPageParameters(connectionConfiguration);
                    manager.CommitChanges();
                }
            }
        }

        static void SetAffinityParameters(ConfigurationElement connectionConfiguration) {
            ConfigurationElement affinityElement = connectionConfiguration.GetChildElement("affinity");
            if(affinityElement != null)
                affinityElement.SetAttributeValue("useCookie", true);
        }

        static void SetHealthCheckPageParameters(ConfigurationElement connectionConfiguration) {
            ConfigurationElement healthCheckElement = connectionConfiguration.GetChildElement("healthCheck");
            if(healthCheckElement != null)
                ConfigurationElementUtils.SetAttribute(healthCheckElement, WebFarmConfiguration.HealthCheckConfiguration);
        }

        internal static bool AddServer(string farmName, string endpoint, int port) {
            return AddServers(farmName, new List<string>() { endpoint }, port);
        }
        internal static bool AddServers(string farmName, IEnumerable<string> endpoints, int port) {
            using(ServerManager manager = new ServerManager()) {
                ConfigurationElementCollection webFarmsConfiguration = GetWebFarmsConfiguration(manager);
                ConfigurationElement webFarm = GetWebFarmConfiguration(webFarmsConfiguration, farmName);
                if(webFarm != null) {
                    ConfigurationElementCollection servers = webFarm.GetCollection();
                    bool configurationChanged = false;
                    foreach(string endPoint in endpoints) {
                        Dictionary<string, string> attributes = new Dictionary<string, string>() { { "address", endPoint } };
                        ConfigurationElement serverConfiguration = ConfigurationElementUtils.FindElement(servers, "server", attributes);
                        if(serverConfiguration == null) {
                            serverConfiguration = servers.CreateElement("server");
                            serverConfiguration["address"] = endPoint;
                            serverConfiguration["enabled"] = true;

                            ConfigurationElement connectionConfiguration = serverConfiguration.GetChildElement("applicationRequestRouting");
                            connectionConfiguration["httpPort"] = port;
                        
                            servers.Add(serverConfiguration);
                            configurationChanged = true;
                        }
                    }
                    if(configurationChanged) 
                        manager.CommitChanges();
                    return true;
                }
            }
            return false;
        }

        internal static bool RemoveServers(string farmName, IEnumerable<string> endpoints) {
            using(ServerManager manager = new ServerManager()) {
                ConfigurationElementCollection webFarmsConfiguration = GetWebFarmsConfiguration(manager);
                ConfigurationElement webFarm = GetWebFarmConfiguration(webFarmsConfiguration, farmName);
                if(webFarm != null) {
                    ConfigurationElementCollection servers = webFarm.GetCollection();
                    bool configurationChanged = false;
                    foreach(string endPoint in endpoints) {
                        Dictionary<string, string> attributes = new Dictionary<string, string>() { { "address", endPoint } };
                        ConfigurationElement serverConfiguration = ConfigurationElementUtils.FindElement(servers, "server", attributes);
                        if(serverConfiguration != null && ConfigurationElementUtils.HasAttribute(serverConfiguration, "address", endPoint)) {
                            servers.Remove(serverConfiguration);
                            configurationChanged = true;
                        }
                    }
                    if(configurationChanged) 
                        manager.CommitChanges();
                    return true;
                }
            }
            return false;
        }

        internal static IEnumerable<string> GetServersFromConfiguration(string farmName) {
            List<string> serversList = new List<string>();
            using(ServerManager manager = new ServerManager()) {
                ConfigurationElementCollection webFarmsConfiguration = GetWebFarmsConfiguration(manager);
                ConfigurationElement webFarm = GetWebFarmConfiguration(webFarmsConfiguration, farmName);
                if(webFarm != null) {
                    ConfigurationElementCollection servers = webFarm.GetCollection();
                    foreach(ConfigurationElement serverConfiguration in servers) {
                        serversList.Add(ConfigurationElementUtils.GetAttributValue(serverConfiguration, "address"));
                    }
                }
            }
            return serversList;
        }

        static ConfigurationElementCollection GetWebFarmsConfiguration(ServerManager manager) {
            return manager.GetApplicationHostConfiguration().GetSection(WEB_FARMS_TAGNAME).GetCollection();
        }
        static ConfigurationElement GetWebFarmConfiguration(ConfigurationElementCollection webFarmConfiguration, string farmName) {
            var attributes = new ConfigurationElementAttributes() { { "name", farmName } };
            return ConfigurationElementUtils.FindElement(webFarmConfiguration, WEB_FARM_TAGNAME, attributes);
        }
    }
}
