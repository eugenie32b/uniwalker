using System;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace UniWalker.Impl
{
    public class JsonWalker : IUniWalkerParser
    {
        private static readonly Regex _startsWith = new Regex("^\\s*[{\\[]");
        private static readonly Regex _endsWith = new Regex("[}\\]]\\s*$");

        public bool TryParse(string s, out UniWalker walker)
        {
            if (_startsWith.IsMatch(s) && _endsWith.IsMatch(s))
            {
                var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(s));

                if (JsonDocument.TryParseValue(ref reader, out JsonDocument document))
                {
                    walker = new DynamicJson(document.RootElement);
                    return true;
                }
            }

            walker = null;
            return false;
        }

        public static UniWalker ParseJson(string json)
        {
            var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));

            if (JsonDocument.TryParseValue(ref reader, out JsonDocument document))
                return new DynamicJson(document.RootElement);

            throw (new Exception("Cannot parse provided data as json"));
        }
    }
}
