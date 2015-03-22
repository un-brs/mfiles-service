using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyInterfaces;
using MFilesAPI;


namespace MFilesHarmony
{
    public class Vault : IRepository
    {
        private readonly string _name;
        private readonly IVault _vault;
        private readonly IView _view;

        private Dictionary<string, PropertyDef> _definitions;

        public static string[] DocTypeKeys =
        {
            "Class",
            "Additional classes",
            "Class group"
        };

        public static readonly string[] ChemicalKeys =
        {
            "Chemical",
            "Chemicals",
            "All Chemicals",
            "AIII Category",
            "Annex III - Chemical"
        };

        public static readonly string[] MeetingKeys =
        {
            "CRM-Meeting",
            "Meeting Acronym",
            "Meeting Acronym (list)"
        };

        public static readonly string[] MeetingTypeKeys =
        {
            "CRM-Meeting-type"
        };

        public static readonly string[] AuthorKeys =
        {
            "Corporate Author",
            "Author(s)"
        };

        public static readonly string[] ProgramKeys =
        {
            "Programme/Subject Matter"
        };

        public static readonly string[] TermKeys =
        {
            "Term-ScientificTechnicalPublications"
        };

        public static readonly string[] TagsKeys =
        {
            "Term-ScientificTechnicalPublications",
            "Keyphrases",
            "Keywords",
            "Keyword"
        };

        public static readonly string[] DescriptionKeys =
        {
            "Description",
            "Short description (in English)"
        };

        public static readonly string NameKey = "Name or title";
        public static readonly string UnNumberKey = "UN-number";
        public static readonly string LanguageKey = "Language";
        public static readonly string TitleKey = "Title";
        public static readonly string PlayerKey = "Player";
        public static readonly string CopyrightKey = "Photo Credits/Source";
        public static readonly string TransmissionDateKey = "TransmissionDate";
        public static readonly string DateIssuanceSignatureKey = "Date Issuance-Signature";
        public static readonly string DateIssuanceKey = "Date Issuance";
        public static readonly string DateOfCorrespondesKey = "Date of correspondence";
        public static readonly string DateStartKey = "Date Start";
        public static readonly string PublicationDateDisplayKey = "PublicationDateDisplay";
        public static readonly string PublicationDateMonthKey = "PublicationDate-Month";
        public static readonly string PublicationDateYearKey = "PublicationDate-Year";
        public static readonly string PeriodBienniumKey = "Period (Biennium or Year)";
        public static readonly string SourceKey = "Source";

        public Vault(string name, IVault vault, IView view)
        {
            _name = name;
            _vault = vault;
            _view = view;
        }

        public IView MfView
        {
            get { return _view; }
        }

        public IVault MfVault
        {
            get { return _vault; }
        }

   
        public string Name
        {
            get{ return _name;}
        }

        public Guid Guid
        {
            get { return Guid.Parse(_vault.GetGUID()); }
        }

        public Dictionary<string, PropertyDef> PropertyDefinitions
        {
            get
            {
                if (_definitions == null)
                {
                    _definitions = new Dictionary<string, PropertyDef>();
                    foreach (PropertyDef pdef in MfVault.PropertyDefOperations.GetPropertyDefs())
                    {
                        _definitions.Add(pdef.Name, pdef);
                    }
                }
                return _definitions;
            }
        }
        public PropertyDef GetPropertyDef(int propertyDef)
        {
            return MfVault.PropertyDefOperations.GetPropertyDef(propertyDef);
        }

        public PropertyValue GetPropertyValue(ObjVer objVer, string key)
        {
            if (PropertyDefinitions.ContainsKey(key))
            {
                try
                {
                    return MfVault.ObjectPropertyOperations.GetProperty(objVer, PropertyDefinitions[key].ID);
                }
                catch (COMException)
                {
                }
            }
            return null;
        }
        public ObjectFile GetObjectFile(ObjVer objVer)
        {
            return MfVault.ObjectFileOperations.GetFiles(objVer)[1];
        }

        public string[] GetListValues(string key)
        {
            var result = new List<string>();
            if (PropertyDefinitions.ContainsKey(key))
            {
                PropertyDef pdef = PropertyDefinitions[key];
                if (pdef.ValueList > 0)
                {
                    ValueListItems items = MfVault.ValueListItemOperations.GetValueListItems(pdef.ValueList);
                    result.AddRange(from ValueListItem item in items select item.Name);
                }
            }
            return result.ToArray();
        }

        public string[] GetListValues(string[] keys)
        {
            var result = new List<string>();
            foreach (string key in keys)
            {
                result.AddRange(GetListValues(key));
            }
            return result.ToArray();
        }
    }
}
