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
  <string>Intranet</string>
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
        [global::System.Configuration.DefaultSettingValueAttribute("Web service")]
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
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>Basel	http://www.basel.int/Portals/4/download.aspx?d=</string>
  <string>Rotterdam	http://www.pic.int/Portals/5/download.aspx?d=</string>
  <string>Stockholm Production	http://chm.pops.int/Portals/0/download.aspx?d=</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection Urls {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["Urls"]));
            }
            set {
                this["Urls"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://informea.pops.int/CountryProfiles/brsTreatyProfile.svc")]
        public string TreatiesServiceUrl {
            get {
                return ((string)(this["TreatiesServiceUrl"]));
            }
            set {
                this["TreatiesServiceUrl"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool IsDeleteUnprocessed {
            get {
                return ((bool)(this["IsDeleteUnprocessed"]));
            }
            set {
                this["IsDeleteUnprocessed"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("30")]
        public int Interval {
            get {
                return ((int)(this["Interval"]));
            }
            set {
                this["Interval"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1000")]
        public int DbReconnectAfter {
            get {
                return ((int)(this["DbReconnectAfter"]));
            }
            set {
                this["DbReconnectAfter"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>Stockholm Production	stockholm</string>
  <string>Intranet	brs</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection VaultsToConventions {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["VaultsToConventions"]));
            }
            set {
                this["VaultsToConventions"] = value;
            }
        }
    }
}
