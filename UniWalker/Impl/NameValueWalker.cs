using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace UniWalker.Impl
{
    public class NameValueWalker : IUniWalkerParser
    {
        [SuppressMessage("ReSharper", "StringIndexOfIsCultureSpecific.1")]
        public bool TryParse(string s, out UniWalker walker)
        {
            if (s.IndexOf("\n") < 0 && s.IndexOf("=") > 0 && s.IndexOf("&") > 0)
                s = s.Replace("&", "\r\n");

            var sr = new StringReader(s);
            var nameValues = new Dictionary<string, string>();
            while (true)
            {
                string l = sr.ReadLine()?.TrimStart().TrimEnd('\r');
                if (l == null)
                    break;

                if (String.IsNullOrWhiteSpace(l))
                    continue;

                int p = l.IndexOf("=");
                if (p < 1)
                {
                    walker = null;
                    return false;
                }

                nameValues[l.Substring(0, p)] = l.Substring(p + 1);
            }

            walker = new DynamicNameValue(nameValues);
            return true;
        }
    }
}
