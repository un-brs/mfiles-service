using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MFilesSync
{
    internal class CultureUtil
    {
        private static IEnumerable<RegionInfo> _regionsInfo;
        private static IEnumerable<CultureInfo> _culturesInfo;

        public static string GetLangTwoLetterCode(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                return null;
            }

            if (null == _culturesInfo)
            {
                _culturesInfo = CultureInfo.GetCultures(CultureTypes.NeutralCultures);
            }

            CultureInfo culture = _culturesInfo.FirstOrDefault(c => c.EnglishName == name);
            return culture != null ? culture.TwoLetterISOLanguageName : null;
        }

        public static string GetCountryTwoLetterCode(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                return null;
            }
            if (null == _regionsInfo)
            {
                _regionsInfo =
                    CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(x => new RegionInfo(x.LCID));
            }

            RegionInfo region = _regionsInfo.FirstOrDefault(r => r.EnglishName.Contains(name));

            return null != region ? region.TwoLetterISORegionName : null;
        }
    }
}