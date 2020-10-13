using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace UniWalker.Impl
{
    public class DynamicNameValue : UniWalker
    {
        private readonly Dictionary<string, string> _nameValues;
        private readonly bool _autoType;

        public DynamicNameValue(Dictionary<string, string> nameValues, bool autoType = true, bool nullOnNotFound = true, bool fuzzyNames = true) : base(nullOnNotFound, fuzzyNames)
        {
            _nameValues = nameValues;
            _autoType = autoType;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string name = binder.Name;

            foreach (string n in PossibleNames(name, FuzzyNames, true))
            {
                if (_nameValues.TryGetValue(n, out string v))
                {
                    result = !_autoType ? v : GetObjectFromString(v);
                    return true;
                }
            }

            result = null;
            return NullOnNotFound;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            string name = binder.Name;

            if (name == "AsEnumerable")
            {
                result = _nameValues.ToList();
                return true;
            }

            result = null;
            return false;
        }
    }
}
