﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.0
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HarmonyApp.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>Basel</string>
  <string>Rotterdam</string>
  <string>Stockholm Production</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection Vaults {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["Vaults"]));
            }
            set {
                this["Vaults"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AM1")]
        public string View {
            get {
                return ((string)(this["View"]));
            }
            set {
                this["View"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2008-01-01")]
        public global::System.DateTime StartDate {
            get {
                return ((global::System.DateTime)(this["StartDate"]));
            }
            set {
                this["StartDate"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://informea.pops.int/Meetings2/asbMeetings.svc")]
        public string TermsServiceUri {
            get {
                return ((string)(this["TermsServiceUri"]));
            }
            set {
                this["TermsServiceUri"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://www.basel.int/Portals/4/download.aspx?d=")]
        public string BaselUrl {
            get {
                return ((string)(this["BaselUrl"]));
            }
            set {
                this["BaselUrl"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://www.pic.int/Portals/5/download.aspx?d=")]
        public string RotterdamUrl {
            get {
                return ((string)(this["RotterdamUrl"]));
            }
            set {
                this["RotterdamUrl"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://chm.pops.int/Portals/0/download.aspx?d=")]
        public string StockholmProductionUrl {
            get {
                return ((string)(this["StockholmProductionUrl"]));
            }
            set {
                this["StockholmProductionUrl"] = value;
            }
        }
    }
}
