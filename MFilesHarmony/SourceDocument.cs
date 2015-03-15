using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyInterfaces;
using MFilesAPI;

namespace MFilesHarmony
{
    public class SourceDocument : ISourceDocument
    {
        private readonly Vault _vault;
        private readonly ObjectVersion _objVer;

        private readonly Dictionary<string, PropertyValue> _propertieValues;
        private DateTime _modifiedDate;
        private DateTime _createdDate;

        public SourceDocument(Vault vault, ObjectVersion objVer)
        {
            _vault = vault;
            _objVer = objVer;

            _propertieValues = new Dictionary<string, PropertyValue>();
    
            UnNumber = GetStringValue(Vault.UnNumberKey) ?? GetStringValue(Vault.NameKey);
            Language = GetStringValue(Vault.LanguageKey);
        }
        public Guid Guid
        {
            get { return Guid.Parse(_objVer.ObjectGUID); }
        }

        public string UnNumber { get; private set; }

        public IRepository Repository
        {
            get { return _vault; }
        }

        public string Language { get; private set; }

        public DateTime ModifiedDate
        {
            get { return _objVer.LastModifiedUtc; }
        }

        public DateTime CreatedDate
        {
            get { return _objVer.CreatedUtc; }
        }

        private PropertyValue GetPropertyValue(string key)
        {
            if (!_propertieValues.ContainsKey(key))
            {
                _propertieValues[key] = _vault.GetPropertyValue(_objVer.ObjVer, key);
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
            var typedValue = GetTypedValue(key);
            if (null != typedValue && !typedValue.IsNULL())
            {
                return typedValue.Value;
            }
            return null;
        }

    }
}
