using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DevExpress.Web;
using DevExpress.Web.ASPxRichEdit;
using DevExpress.Web.ASPxSpreadsheet;

namespace DocumentSite {
    public static class RibbonHelper {
        public static void HideFileTab(ASPxSpreadsheet spreadsheet) {
            spreadsheet.CreateDefaultRibbonTabs(true);
            RemoveRibbonTab(spreadsheet.RibbonTabs, typeof(SRFileTab));
            spreadsheet.ActiveTabIndex = 0;
        }
        public static void HideFileTab(ASPxRichEdit richedit) {
            richedit.CreateDefaultRibbonTabs(true);
            RemoveRibbonTab(richedit.RibbonTabs, typeof(RERFileTab));
            richedit.ActiveTabIndex = 0;
        }
        static void RemoveRibbonTab(Collection<RibbonTab> ribbonTabs, Type tabTypeToRemove) {
            foreach(RibbonTab tab in ribbonTabs) {
                if(tab.GetType() == tabTypeToRemove) {
                    ribbonTabs.Remove(tab);
                    break;
                }
            }
        }
    }
}
