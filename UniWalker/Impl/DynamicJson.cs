using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;

namespace UniWalker.Impl
{
    public class DynamicJson : UniWalker
    {
        private readonly JsonElement _element;

        public DynamicJson(JsonElement element, bool nullOnNotFound = true, bool fuzzyNames = true) : base(nullOnNotFound, fuzzyNames)
        {
            _element = element;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string name = binder.Name;

            if (_element.TryGetProperty(name, out JsonElement element))
            {
                result = GetResultFromElement(element);
                return true;
            }

            if (_element.ValueKind == JsonValueKind.Array && (name == "Length" || name == "Count"))
            {
                result = _element.EnumerateArray().Count();
                return true;
            }

            foreach (string n in PossibleNames(name, FuzzyNames, false))
            {
                if (_element.TryGetProperty(n, out JsonElement childElement))
                {
                    result = GetResultFromElement(childElement);
                    return true;
                }
            }

            result = null;
            return NullOnNotFound;
        }

        public override bool TryInvokeMember(System.Dynamic.InvokeMemberBinder binder, object[] args, out object result)
        {
            string name = binder.Name;

            if (name == "AsEnumerable" && _element.ValueKind == JsonValueKind.Array)
            {
                var elements = new List<object>();

                foreach (JsonElement element in _element.EnumerateArray())
                {
                    elements.Add(GetResultFromElement(element));
                }

                result = elements;
                return true;
            }

            result = null;
            return false;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (_element.ValueKind != JsonValueKind.Array)
            {
                result = null;
                return NullOnNotFound;
            }

            int index = (int)indexes[0];
            int i = 0;

            foreach (JsonElement element in _element.EnumerateArray())
            {
                if (i == index)
                {
                    result = GetResultFromElement(element);
                    return true;
                }

                i++;
            }

            result = null;
            return NullOnNotFound;
        }

        private static object GetResultFromElement(JsonElement element)
        {
            object result = element.ValueKind switch
            {
                JsonValueKind.Array => new DynamicJson(element),
                JsonValueKind.Object => new DynamicJson(element),
                JsonValueKind.Number => GetNumber(element.GetRawText()),
                JsonValueKind.False => false,
                JsonValueKind.True => true,
                JsonValueKind.String => element.GetString(),
                _ => null
            };

            return result;
        }
    }
}
