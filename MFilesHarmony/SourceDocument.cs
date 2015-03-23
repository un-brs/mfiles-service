using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyInterfaces;
using MFilesAPI;
using Utils;

namespace MFilesHarmony
{
    public class ListProperty : IListProperty
    {
        private readonly Guid? _guid;
        private readonly string _value;
        private readonly bool _isStringProperty;
        public ListProperty(Lookup lookup)
        {
            _isStringProperty = false;
            if (lookup.DisplayIDAvailable)
            {
                Guid guid;
                if (System.Guid.TryParse(lookup.DisplayID, out guid))
                {
                    _guid = guid;
                }
            }
            _value = lookup.DisplayValue;
        }

        public ListProperty(string value)
        {
            _isStringProperty = true;
            _value = value;
        }
        public Guid? Guid
        {
            get {return  _guid; }
        }

        public string Value
        {
            get { return _value; }
        }

        public bool IsStringProperty
        {
            get {  return _isStringProperty;}
        }
    }
    public class SourceDocument : ISourceDocument
    {
        private readonly Vault _vault;
        private readonly ObjectVersion _objVer;

        private readonly Dictionary<string, PropertyValue> _propertieValues;
        private readonly File _file;

        public SourceDocument(Vault vault, ObjectVersion objVer)
        {
            _vault = vault;
            _objVer = objVer;
            _file = new File(this, _vault.MfVault.ObjectFileOperations.GetFiles(_objVer.ObjVer)[1]); 
  
            _propertieValues = new Dictionary<string, PropertyValue>();
  ;
        }
        public Guid Guid { get { return Guid.Parse(_objVer.ObjectGUID); }}
        public string UnNumber { get { return GetStringValue(Vault.UnNumberKey); }}
        public string Name { get { return GetStringValue(Vault.NameKey); } }
        public string Title { get { return GetStringValue(Vault.TitleKey); } }
        public string Description { get { return GetStringValue(Vault.DescriptionKeys); } }

     
        public string Country
        {
            get
            {
                var player = GetStringValue(Vault.PlayerKey);

                if (player == null) {
                    return null;
                }

                return CultureUtils.GetCountryTwoLetterCode(player) != null ? player : null;
            }
        }

        public string Copyright
        {
            get
            {
                return GetStringValue(Vault.CopyrightKey);
            }
        }

        public string SourceUrl
        {
            get
            {
                var val = GetStringValue(Vault.SourceKey);
                if (!String.IsNullOrWhiteSpace(val))
                {
                    val = val.Trim();
                    if (Uri.IsWellFormedUriString(val, UriKind.Absolute))
                    {
                        return val;
                    }
                }
                return null;
            }
        }

        public bool CanBeSynchronized
        {
            get
            {
                if (Repository.Name == "Intranet")
                {
                    return !string.IsNullOrEmpty(SourceUrl);
                }
                return true;
            }
        }

        public DateTime PublicationDate
        {
            get
            {

                var d = GetDateTimeValue(Vault.TransmissionDateKey);
                if (d.HasValue) {
                    return d.Value;
                }
                
                d = GetDateTimeValue(Vault.DateIssuanceKey);
                if (d.HasValue) {
                    return d.Value;
                }

                d = GetDateTimeValue(Vault.DateIssuanceSignatureKey);
                if (d.HasValue) {
                    return d.Value;
                }
                d = GetDateTimeValue(Vault.DateOfCorrespondesKey);
                if (d.HasValue) {
                    return d.Value;
                }

                d = GetDateTimeValue(Vault.DateStartKey);
                if (d.HasValue) {
                    return d.Value;
                }

                var sd = GetStringValue(Vault.PublicationDateDisplayKey);
                try {
                    var sdDate = DateTime.ParseExact(sd, "MMMM yyyy", CultureInfo.CurrentCulture);
                    return sdDate;
                } catch (FormatException) {

                }

                var pubMonth = GetStringValue(Vault.PublicationDateMonthKey);
                var pubYear = GetStringValue(Vault.PublicationDateYearKey);

                if (!(String.IsNullOrWhiteSpace(pubMonth) || String.IsNullOrWhiteSpace(pubYear))) {
                    try {
                        var sdDate = DateTime.ParseExact(pubMonth + " " + pubYear, "MMMM yyyy", CultureInfo.CurrentCulture);
                        return sdDate;
                    } catch (FormatException) {

                    }
                }

                return CreatedDate;
            }
        }

        public Tuple<DateTime, DateTime> GetPeriod()
        {
            var periods = GetStringValues(Vault.PeriodBienniumKey);

            var iperiods = new List<int>();
            foreach (var period in periods) {
                var startAndEnd = period.Split('-');
                foreach (var p in startAndEnd) {
                    var year = 0;
                    if (int.TryParse(p, out year)) {
                        iperiods.Add(year);
                    }
                }
            }

            DateTime? periodStartDate = null;
            DateTime? periodEndDate = null;
            if (iperiods.Count > 0) {
                periodStartDate = new DateTime(iperiods[0], 1, 1);
            }

            if (iperiods.Count > 1) {
                periodEndDate = new DateTime(iperiods[iperiods.Count - 1], 1, 1);
            }

            if (periodStartDate != null && periodEndDate != null)
            {
                return new Tuple<DateTime, DateTime>(periodStartDate.Value, periodEndDate.Value);
            }
            return null;
        }

        public IList<IListProperty> Types
        {
            get { return GetListValues(Vault.DocTypeKeys); }
        }

        public IList<IListProperty> Chemicals
        {
            get { return GetListValues(Vault.ChemicalKeys); }
        }

        public IList<IListProperty> Programs
        {
            get { return GetListValues(Vault.ProgramKeys); }
        }

        public IList<IListProperty> Terms
        {
            get { return GetListValues(Vault.TermKeys); }
        }

        public IList<IListProperty> Tags
        {
            get { return GetListValues(Vault.TagsKeys); }
        }

        public IList<IListProperty> Meetings
        {
            get { return GetListValues(Vault.MeetingKeys); }
        }

        public IList<IListProperty> MeetingsTypes
        {
            get { return GetListValues(Vault.MeetingTypeKeys); }
        }


        public IRepository Repository
        {
            get { return _vault; }
        }

        public string Language { get { return GetStringValue(Vault.LanguageKey); } }

        public string Author
        {
            get
            {
                var player = GetStringValue(Vault.PlayerKey);
                var author = GetStringValue(Vault.AuthorKeys);
                author = string.IsNullOrWhiteSpace(author) ? null : author;
                if (string.IsNullOrWhiteSpace(player))
                {
                    return author;

                }
                var country = CultureUtils.GetCountryTwoLetterCode(player);
                if (country == null && author == null)
                {
                    return player;
                }
                else
                {
                    return author;
                }
            }
        }

        public DateTime ModifiedDate
        {
            get { return _objVer.LastModifiedUtc; }
        }

        public DateTime CreatedDate
        {
            get { return _objVer.CreatedUtc; }
        }

        public IFile File
        {
            get
            {
                return _file;
                
            }
        }

        private PropertyValue GetPropertyValue(string key)
        {
            if (!_propertieValues.ContainsKey(key))
            {
                _propertieValues[key] = _vault.GetPropertyValue(_objVer.ObjVer, key);
            }
            return _propertieValues[key];
        }

        public IList<IListProperty> GetListValues(string key)
        {
            var result = new List<IListProperty>();

            var propertyValue = GetPropertyValue(key);
            if (null != propertyValue)
            {
                var propertyDef = _vault.PropertyDefinitions[key];

                if (propertyDef.ValueList > 0)
                {
                    result.AddRange(from Lookup lookup in propertyValue.Value.GetValueAsLookups() 
                                    where !String.IsNullOrWhiteSpace(lookup.DisplayValue)
                                    group lookup by lookup.DisplayValue into g
                                    select new ListProperty(g.First()));
                }
                else
                {
                    if (!String.IsNullOrEmpty(propertyValue.Value.DisplayValue))
                    {
                        result.Add(new ListProperty(propertyValue.Value.DisplayValue));
                    }
                }
            }

            return result;
        }

        public IList<IListProperty> GetListValues(string[] keys)
        {
            var result = new List<IListProperty>();
            foreach (var k in keys)
            {
                result.AddRange(GetListValues(k));
            }
            return result;
        }

        public IListProperty GetListValue(string key)
        {
            var values = GetListValues(key);
            return values.Count > 0 ? values[0] : null;
        }

        public IListProperty GetListValue(string[] keys)
        {
            return keys.Select(GetListValue).FirstOrDefault(value => value != null);
        }

        public string GetStringValue(string key)
        {
            var values = GetListValues(key);
            return StringUtils.Concatenate(values, (IListProperty p) => p.Value, ',');
        }

        private string GetStringValue(string[] keys)
        {
            var values = GetListValues(keys);
            foreach (var v in values)
            {
                return v.Value;
            }
            return "";
        }

        private IList<string> GetStringValues(string key)
        {
            return GetListValues(key).Select(v => v.Value).ToList();
        }

        private IList<string> GetStringValues(string[] keys)
        {
            var result = new List<string>();
            foreach (var key in keys)
            {
                result.AddRange(GetStringValues(key));
            }
            return result;
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
            var typedValue = GetTypedValue(key);
            if (null != typedValue && !typedValue.IsNULL())
            {
                return typedValue.Value;
            }
            return null;
        }

    }
}
