using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Conventions.MFiles.Models;
using MFilesAPI;

namespace MFilesSync
{
    public class MFilesVault
    {
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
            "Meeting Acronym",
            "Meeting Acronym (list)"
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
            "Term-ScientificTechnicalPublications",
            "Keyphrases",
            "Keyword"
        };

        public static readonly string[] DescriptionKeys =
        {
            "Description",
            "Short description (in English)"
        };


        public static readonly string TitleKey = "Title";
        public static readonly string PlayerKey = "Player";
        public static readonly string CopyrightKey = "Photo Credits/Source";
        public static readonly string TransmissionDateKey = "TransmissionDate";
        public static readonly string DateIssuanceSignatureKey = "Date Issuance-Signature";
        public static readonly string DateOfCorrespondesKey = "Date of correspondence";
        public static readonly string DateStartKey = "Date Start";
        public static readonly string PublicationDateDisplayKey = "PublicationDateDisplay";
        public static readonly string PublicationDateMonthKey = "PublicationDate-Month";
        public static readonly string PublicationDateYearKey = "PublicationDate-Year";
        public static readonly string PeriodBienniumKey = "Period (Biennium or Year)";

        private Dictionary<string, PropertyDef> _definitions;


        public MFilesVault(string name, IVault vault)
        {
            Vault = vault;
            Name = name;
        }

        public string Name { get; private set; }


        public Dictionary<string, PropertyDef> PropertyDefinitions
        {
            get
            {
                if (_definitions == null)
                {
                    _definitions = new Dictionary<string, PropertyDef>();
                    foreach (PropertyDef pdef in Vault.PropertyDefOperations.GetPropertyDefs())
                    {
                        _definitions.Add(pdef.Name, pdef);
                    }
                }
                return _definitions;
            }
        }

        public IVault Vault { get; private set; }

        public PropertyDef GetPropertyDef(int propertyDef)
        {
            return Vault.PropertyDefOperations.GetPropertyDef(propertyDef);
        }

        public PropertyValue GetPropertyValue(ObjVer objVer, string key)
        {
            if (PropertyDefinitions.ContainsKey(key))
            {
                try
                {
                    return Vault.ObjectPropertyOperations.GetProperty(objVer, PropertyDefinitions[key].ID);
                }
                catch (COMException)
                {
                }
            }
            return null;
        }

        public ObjectFile GetObjectFile(ObjVer objVer)
        {
            return Vault.ObjectFileOperations.GetFiles(objVer)[1];
        }

        public string[] GetListValues(string key)
        {
            var result = new List<string>();
            if (PropertyDefinitions.ContainsKey(key))
            {
                PropertyDef pdef = PropertyDefinitions[key];
                if (pdef.ValueList > 0)
                {
                    ValueListItems items = Vault.ValueListItemOperations.GetValueListItems(pdef.ValueList);
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


        public string[] GetDocumentTypes()
        {
            return GetListValues(DocTypeKeys);
        }
    }


    public class MFilesInternalDocument
    {
        private readonly DateTime _createdDate;
        private readonly ObjectFile _file;
        private readonly string _language;
        private readonly DateTime _modifiedDate;
        private readonly Guid _objectGuid;


        private readonly ObjectVersion _objectVersion;
        private readonly Dictionary<string, PropertyValue> _propertieValues;
        private readonly string _unNumber;
        private readonly MFilesVault _vault;
        private readonly string _vaultName;
        private readonly Guid _versionGuid;
        private MFilesDocument _mfilesDocument;

        public MFilesInternalDocument(MFilesVault vault, ObjectVersion objectVersion)
        {
            _vault = vault;
            _objectVersion = objectVersion;
            _propertieValues = new Dictionary<string, PropertyValue>();

            _objectGuid = Guid.Parse(objectVersion.ObjectGUID);
            _versionGuid = Guid.Parse(objectVersion.VersionGUID);
            _vaultName = vault.Name;
            // Fill mandatory fields
            _unNumber = GetStringValue("UN-number");
            _language = GetStringValue("Language");
            // ReSharper disable once PossibleInvalidOperationException
            _modifiedDate = GetDateTimeValue("Last modified").Value;
            // ReSharper disable once PossibleInvalidOperationException
            _createdDate = GetDateTimeValue("Created").Value;

            _file = _vault.GetObjectFile(_objectVersion.ObjVer);

            DerivedDocuments = new List<MFilesInternalDocument>();
        }

        public IList<MFilesInternalDocument> DerivedDocuments { get; private set; }


        public Guid ObjectGuid
        {
            get { return _objectGuid; }
        }

        public Guid VersionGuid
        {
            get { return _versionGuid; }
        }

        public string UnNumber
        {
            get { return _unNumber; }
        }

        public string Language
        {
            get { return _language; }
        }

        public string VaultName
        {
            get { return _vaultName; }
        }

        public DateTime ModifiedDate
        {
            get { return _modifiedDate; }
        }

        public DateTime CreatedDate
        {
            get { return _createdDate; }
        }

        public ObjectFile File
        {
            get { return _file; }
        }

        public MFilesDocument MFilesDocument
        {
            get
            {
                if (_mfilesDocument == null)
                {
                    _mfilesDocument = new MFilesDocument();
                    _mfilesDocument.MFilesDocumentGuid = ObjectGuid;
                    _mfilesDocument.CreatedDate = CreatedDate;
                    _mfilesDocument.ModifiedDate = ModifiedDate;
                }
                return _mfilesDocument;
            }
        }


        private PropertyValue GetPropertyValue(string key)
        {
            if (!_propertieValues.ContainsKey(key))
            {
                _propertieValues[key] = _vault.GetPropertyValue(_objectVersion.ObjVer, key);
            }
            return _propertieValues[key];
        }

        public string[] GetStringValues(string key)
        {
            var result = new List<string>();

            PropertyValue propertyValue = GetPropertyValue(key);
            if (null != propertyValue)
            {
                PropertyDef propertyDef = _vault.PropertyDefinitions[key];

                if (propertyDef.ValueList > 0)
                {
                    result.AddRange(from Lookup lookup in propertyValue.Value.GetValueAsLookups()
                        select lookup.DisplayValue);
                }
                else
                {
                    result.Add(propertyValue.Value.DisplayValue);
                }
            }

            return result.ToArray();
        }

        public string[] GetStringValues(string[] keys)
        {
            var result = new List<string>();
            foreach (string k in keys)
            {
                result.AddRange(GetStringValues(k));
            }
            return result.ToArray();
        }

        public string GetStringValue(string key)
        {
            string[] values = GetStringValues(key);
            return string.Join(", ", values);
        }

        private TypedValue GetTypedValue(string key)
        {
            PropertyValue propertyValue = GetPropertyValue(key);
            if (null != propertyValue)
            {
                return propertyValue.TypedValue;
            }
            return null;
        }

        public int? GetIntegerValue(string key)
        {
            TypedValue typedValue = GetTypedValue(key);
            if (null != typedValue)
            {
                return typedValue.Value;
            }
            return null;
        }

        public DateTime? GetDateTimeValue(string key)
        {
            TypedValue typedValue = GetTypedValue(key);
            if (null != typedValue && !typedValue.IsNULL())
            {
                return typedValue.Value;
            }
            return null;
        }


        public string[] GetChemicals()
        {
            return GetStringValues(MFilesVault.ChemicalKeys);
        }

        public string[] GetDocumentTypes()
        {
            return GetStringValues(MFilesVault.DocTypeKeys);
        }

        public string[] GetProgrammes()
        {
            return GetStringValues(MFilesVault.ProgramKeys);
        }

        public string[] GetTerms()
        {
            return GetStringValues(MFilesVault.TermKeys);
        }

        public string GetMeeting()
        {
            return GetStringValueFromKeys(MFilesVault.MeetingKeys);
        }

        private string GetStringValueFromKeys(string[] keys)
        {
            string[] values = GetStringValues(keys);
            foreach (string v in values)
            {
                if (!String.IsNullOrEmpty(v))
                {
                    return v;
                }
            }
            return "";
        }

        public string GetDescription()
        {
            return GetStringValueFromKeys(MFilesVault.DescriptionKeys);
        }

        public string GetAuthor()
        {
            return GetStringValueFromKeys(MFilesVault.AuthorKeys);
        }

        public string GetTitle()
        {
            return GetStringValue(MFilesVault.TitleKey);
        }

        public string GetPlayer()
        {
            return GetStringValue(MFilesVault.PlayerKey);
        }


        public string GetCopyright()
        {
            return GetStringValue(MFilesVault.CopyrightKey);
        }

        public DateTime? GetTransmissionDate()
        {
            return GetDateTimeValue(MFilesVault.TransmissionDateKey);
        }

        public DateTime? GetDateIssuanceSignature()
        {
            return GetDateTimeValue(MFilesVault.DateIssuanceSignatureKey);
        }

        public DateTime? GetDateOfCorrespondence()
        {
            return GetDateTimeValue(MFilesVault.DateOfCorrespondesKey);
        }

        public DateTime? GetDateStart()
        {
            return GetDateTimeValue(MFilesVault.DateStartKey);
        }

        public static readonly string PublicationDateDisplayKey = "PublicationDateDisplay";
        public static readonly string PublicationDateMonthKey = "PublicationDate-Month";
        public static readonly string PublicationDateYearKey = "PublicationDate-Year";
        public static readonly string PeriodBienniumKey = "Period (Biennium or Year)";

        public string GetPublicationDateDisplay()
        {
            return GetStringValue(MFilesVault.PublicationDateDisplayKey);
        }

        public string GetPublicationDateMonth()
        {
            return GetStringValue(MFilesVault.PublicationDateMonthKey);
        }

        public string GetPublicationDateYear()
        {
            return GetStringValue(MFilesVault.PublicationDateYearKey);
        }

        public string GetPeriodBiennium()
        {
            return GetStringValue(MFilesVault.PeriodBienniumKey);
        }
    }
}