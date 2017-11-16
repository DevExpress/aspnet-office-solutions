using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;

namespace DevExpress.Web.OfficeAzureRoutingServer {
    public static class IISUrlRewriterConfigurationUtils
    {
        public static void CreateURLRewriteRule(string serverFarm, bool forceRecreate = true)
        {
            using (ServerManager manager = new ServerManager())
            {
                ConfigurationElementCollection urlRewriterConfiguration = manager.GetApplicationHostConfiguration().GetSection("system.webServer/rewrite/globalRules").GetCollection();
                string str = string.Format("ARR_{0}_loadbalance", serverFarm);

                var attributes = new Dictionary<string, string>() { { "name", str } };
                var urlRewriterRule = ConfigurationElementUtils.FindElement(urlRewriterConfiguration, "rule", attributes);

                if (urlRewriterRule != null)
                {
                    if (forceRecreate)
                        urlRewriterConfiguration.Remove(urlRewriterRule);
                    else
                        throw new InvalidOperationException("Cannot create rule with duplicate name");
                }
                ConfigurationElement element = urlRewriterConfiguration.CreateElement("rule");
                element["name"] = str;
                element["patternSyntax"] = "Wildcard";
                element["stopProcessing"] = true;
                element["enabled"] = true;
                element.GetChildElement("match")["url"] = "*";
                ConfigurationElement childElement = element.GetChildElement("action");
                childElement["type"] = "Rewrite";
                childElement["url"] = "http://" + serverFarm + "/{R:0}";
                urlRewriterConfiguration.Add(element);
                manager.CommitChanges();
            }
        }
    }
}
