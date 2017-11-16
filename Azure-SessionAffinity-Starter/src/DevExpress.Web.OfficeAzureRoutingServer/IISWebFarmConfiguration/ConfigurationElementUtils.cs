using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevExpress.Web.OfficeAzureRoutingServer {
    public static class ConfigurationElementUtils {
        public static ConfigurationElement FindElement(ConfigurationElementCollection collection, string elementTagName, Dictionary<string, string> attributes) {
            foreach(ConfigurationElement element in collection) {
                if(!string.Equals(element.Schema.Name, elementTagName, StringComparison.OrdinalIgnoreCase))
                    continue;
                var satisfy = attributes.All(kvp => HasAttribute(element, kvp));
                if(satisfy)
                    return element;
            }
            return null;
        }
        public static bool HasAttribute(ConfigurationElement element, KeyValuePair<string, string> attribute) {
            return HasAttribute(element, attribute.Key, attribute.Value);
        }
        public static bool HasAttribute(ConfigurationElement element, string attributeName, string attributeValue) {
            return string.Equals(attributeValue,
                GetAttributValue(element, attributeName),
                StringComparison.OrdinalIgnoreCase);
        }
        public static string GetAttributValue(ConfigurationElement element, string attributeName) {
            var attribute = element.Attributes.FirstOrDefault(attr => string.Equals(attr.Name, attributeName));
            return (attribute != null) ?
                string.Concat(attribute.Value) :
                string.Empty;
        }
        public static void SetAttribute(ConfigurationElement element, Dictionary<string, string> attributes) {
            foreach(var attribute in attributes) {
                element.SetAttributeValue(attribute.Key, attribute.Value);
            }
        }
    }
}
