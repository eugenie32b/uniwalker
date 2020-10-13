using System.Text.RegularExpressions;
using System.Xml;

namespace UniWalker.Impl
{
    public class XmlWalker : IUniWalkerParser
    {
        private static readonly Regex _startsWith = new Regex("^\\s*<");
        private static readonly Regex _endsWith = new Regex(">\\s*$");

        public bool TryParse(string s, out UniWalker walker)
        {
            if (_startsWith.IsMatch(s) && _endsWith.IsMatch(s))
            {
                walker = ParseXml(s);

                return true;
            }

            walker = null;

            return false;
        }

        public static UniWalker ParseXml(string xml)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            return new DynamicXml(xmlDoc.DocumentElement);
        }
    }
}
