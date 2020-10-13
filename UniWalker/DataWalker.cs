using System;
using UniWalker.Impl;

namespace UniWalker
{
    public static class DataWalker
    {
        private static readonly IUniWalkerParser[] _supportedWalkers = {
            new XmlWalker(),
            new JsonWalker(),
            new NameValueWalker(),
        };

        public static UniWalker Parse(string s)
        {
            foreach (IUniWalkerParser walker in _supportedWalkers)
            {
                if (walker.TryParse(s, out UniWalker w))
                    return w;
            }

            throw (new Exception("Cannot determine proper format for the data"));
        }
    }
}
