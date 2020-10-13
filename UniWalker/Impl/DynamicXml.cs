using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Xml;

namespace UniWalker.Impl
{
    public class DynamicXml : UniWalker
    {
        private readonly XmlElement _element;
        private readonly bool _autoType;

        public DynamicXml(XmlElement element, bool autoType = true, bool nullOnNotFound = true, bool fuzzyNames = true) : base(nullOnNotFound, fuzzyNames)
        {
            _element = element;
            _autoType = autoType;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string name = binder.Name;

            XmlNode firstNode = _element.SelectSingleNode(name);
            if (firstNode != null && firstNode.NodeType == XmlNodeType.Element)
            {
                result = GetResultFromElement(firstNode as XmlElement, _autoType);
                return true;
            }

            switch (name)
            {
                case "Text":
                case "Value":
                    result = _element.InnerText;
                    return true;

                case "Name":
                case "Tag":
                    result = _element.Name;
                    return true;

                case "Count":
                case "Length":
                    result = GetElements(_element.ParentNode, _element.Name).Count();
                    return true;

                default:
                    if (name.EndsWith("Attr"))
                    {
                        foreach (string n in PossibleNames(name.Substring(0, name.Length - 4), FuzzyNames, true))
                        {
                            if (_element.HasAttribute(n))
                            {
                                string s = _element.GetAttribute(n);
                                result = !_autoType ? s : GetObjectFromString(s);
                                return true;
                            }
                        }
                    }

                    switch (_element.Name)
                    {
                        // special case to support appSettings section in app/web.configs
                        case "appSettings":
                            {
                                foreach (string n in PossibleNames(name, FuzzyNames, true))
                                {
                                    // no need to sanitize xpath to prevent injection,
                                    // because n is value of variable name and
                                    // it cannot contain characters required for injection
                                    XmlNode node = _element.SelectSingleNode($"add[@key='{n}']");

                                    if (node != null && node.NodeType == XmlNodeType.Element)
                                    {
                                        string s = (node as XmlElement)?.GetAttribute("value");
                                        result = !_autoType ? s : GetObjectFromString(s);
                                        return true;
                                    }
                                }

                                break;
                            }

                        // special case to support connectionStrings section in app/web.configs
                        case "connectionStrings":
                            {
                                foreach (string n in PossibleNames(name, FuzzyNames, true))
                                {
                                    // no need to sanitize xpath to prevent injection,
                                    // because n is value of variable name and
                                    // it cannot contain characters required for injection
                                    XmlNode node = _element.SelectSingleNode($"add[@name='{n}']");

                                    if (node != null && node.NodeType == XmlNodeType.Element)
                                    {
                                        result = (node as XmlElement)?.GetAttribute("connectionString");
                                        return true;
                                    }
                                }

                                break;
                            }
                    }

                    foreach (string n in PossibleNames(name, FuzzyNames, true))
                    {
                        XmlNode node = _element.SelectSingleNode(n);
                        if (node != null && node.NodeType == XmlNodeType.Element)
                        {
                            result = GetResultFromElement(node as XmlElement, _autoType);
                            return true;
                        }
                    }
                    break;
            }

            result = null;
            return NullOnNotFound;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            string name = binder.Name;

            if (name == "AsEnumerable")
            {
                var elements = new List<object>();

                string elementName = _element.Name;
                foreach (XmlElement element in (_element.ParentNode ?? _element).ChildNodes.OfType<XmlElement>().Where(w => w.Name == elementName))
                {
                    elements.Add(GetResultFromElement(element, _autoType));
                }

                result = elements;
                return true;
            }

            result = null;
            return false;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            XmlElement[] elements = GetElements(_element.ParentNode ?? _element, _element.Name).ToArray();
            int index = (int)indexes[0];
            if (index >= 0 && index < elements.Length)
            {
                result = GetResultFromElement(elements[index], _autoType);
                return true;
            }

            result = null;
            return NullOnNotFound;
        }

        private static object GetResultFromElement(XmlElement element, bool autoType)
        {
            if (!element.HasAttributes && !element.ChildNodes.OfType<XmlElement>().Any() && (element.ParentNode ?? element).ChildNodes.OfType<XmlElement>().Count(w => w.Name == element.Name) == 1)
            {
                if (!autoType)
                    return element.InnerText;

                return GetObjectFromString(element.InnerText);
            }

            return new DynamicXml(element);
        }

        private static IEnumerable<XmlElement> GetElements(XmlNode parentNode, string name)
        {
            foreach (XmlElement element in parentNode.ChildNodes.OfType<XmlElement>())
            {
                if (element.Name == name)
                    yield return element;
            }
        }
    }
}
