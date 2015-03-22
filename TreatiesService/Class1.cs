using System;
using System.Collections.Generic;
using HarmonyInterfaces;
using TreatiesService.Service;

namespace TreatiesService
{
    public class CountriesClient : ICountries
    {
        private readonly TreatyProfileContext _ctx;
        private Dictionary<string, string> _isoCodes2;

        public CountriesClient(string serviceUri)
        {
            _ctx = new TreatyProfileContext(new Uri(serviceUri));
        }

        private Dictionary<string, string> IsoCodes2
        {
            get
            {
                if (_isoCodes2 == null)
                {
                    _isoCodes2 = new Dictionary<string, string>();
                    foreach (var country in _ctx.countryNames)
                    {
                        if (!String.IsNullOrEmpty(country.NameEn) && !_isoCodes2.ContainsKey(country.NameEn.ToLower()))
                        {
                            _isoCodes2.Add(country.NameEn.ToLower(), country.IsoCode2d);
                        }
                        if (!String.IsNullOrEmpty(country.NameFr) && !_isoCodes2.ContainsKey(country.NameFr.ToLower()))
                        {
                            _isoCodes2.Add(country.NameFr.ToLower(), country.IsoCode2d);
                        }
                        if (!String.IsNullOrEmpty(country.NameEs) && !_isoCodes2.ContainsKey(country.NameEs.ToLower()))
                        {
                            _isoCodes2.Add(country.NameEs.ToLower(), country.IsoCode2d);
                        }
                    }
                }
                return _isoCodes2;
            }
        }

        public string GetCountryIsoCode2(string countryName)
        {
            if (countryName == null)
            {
                return null;
            }
            var key = countryName.ToLower();
            return IsoCodes2.ContainsKey(key) ? IsoCodes2[key] : null;
        }
    }
}