using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace UniWalker
{
    public abstract class UniWalker : DynamicObject
    {
        protected bool NullOnNotFound;
        protected bool FuzzyNames;

        protected UniWalker(bool nullOnNotFound = true, bool fuzzyNames = true)
        {
            FuzzyNames = fuzzyNames;
            NullOnNotFound = nullOnNotFound;
        }

        protected static IEnumerable<string> PossibleNames(string s, bool fuzzyNames, bool dashNames)
        {
            if (!fuzzyNames)
                return new[] { s };

            var list = new List<string>()
            {
                s,
                s.ToLower()
            };

            var regEx = new Regex("([a-z0-9])([A-Z])");
            if (regEx.IsMatch(s))
            {
                list.Add(regEx.Replace(s, (e) => e.Groups[1].Value + "_" + e.Groups[2].Value).ToLower());

                if (dashNames)
                    list.Add(regEx.Replace(s, (e) => e.Groups[1].Value + "-" + e.Groups[2].Value).ToLower());
            }

            if (dashNames && s.IndexOf('_') > 0)
            {
                string l = s.Replace("_", "-");
                list.Add(l);
                list.Add(l.ToLower());
            }

            string c = Regex.Replace(s, "^[A-Z]", m => m.Value.ToLower());
            if (s != c)
                list.Add(c);

            return list.Distinct();
        }

        protected static object GetNumber(string s)
        {
            // ReSharper disable once StringIndexOfIsCultureSpecific.1
            if (s.IndexOf(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) >= 0)
                return Double.Parse(s);

            long l = Int64.Parse(s);
            if (l > Int32.MaxValue || l < Int32.MinValue)
                return l;

            return (int)l;
        }

        protected static object GetObjectFromString(string s)
        {
            string sl = s?.ToLower();

            if (sl == "true")
                return true;

            if (sl == "false")
                return false;

            if (Double.TryParse(s, out _))
            {
                return GetNumber(s);
            }

            if (DateTime.TryParse(s, out DateTime dt))
                return dt;

            return s;
        }
    }
}
