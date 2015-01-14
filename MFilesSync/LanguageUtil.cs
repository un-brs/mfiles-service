using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFilesSync
{
    class LanguageUtil
    {
        private static Dictionary<string, string> _twoLetterCodes; 
        public static string GetTwoLetterCode(string name)
        {
            if (null == _twoLetterCodes)
            {
                _twoLetterCodes = new Dictionary<string, string>();

                foreach (var ci in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
                {
                    if (!_twoLetterCodes.ContainsKey(ci.EnglishName)) {
                        _twoLetterCodes.Add(ci.EnglishName, ci.TwoLetterISOLanguageName);
                    }
                }
            }

            if (_twoLetterCodes.ContainsKey(name))
            {
                return _twoLetterCodes[name];
            }

            return "xx";
        }
    }
}
