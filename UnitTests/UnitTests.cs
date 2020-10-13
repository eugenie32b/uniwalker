using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniWalker.Impl;

namespace UniWalker
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void TestJson()
        {
            string json = "{ "
                          + "\"IntValue\": 123, "
                          + "\"ArrayValue\": [ \"item1\", \"item2\", \"item3\" ], "
                          + "\"ObjValue\": { \"snake_case1\": 234, \"camelCase2\": true }"
                          + "}";

            dynamic walker = DataWalker.Parse(json);
            Assert.IsTrue(walker is DynamicJson);

            Assert.AreEqual(walker.IntValue, 123);
            Assert.AreEqual(walker.ObjValue.SnakeCase1, 234);
            Assert.AreEqual(walker.ObjValue.CamelCase2, true);
            Assert.AreEqual(walker.ArrayValue[0], "item1");
            Assert.AreEqual(walker.ArrayValue[1], "item2");
            Assert.AreEqual(walker.ArrayValue[2], "item3");
            Assert.IsNull(walker.NonExisting);

            IEnumerable<object> elements = walker.ArrayValue.AsEnumerable();
            Assert.AreEqual(elements.Count(), 3);
            Assert.AreEqual(elements.First(), "item1");
            Assert.AreEqual(elements.Last(), "item3");
        }

        [TestMethod]
        public void TestXml()
        {
            string xml = "<data>"
                         + "<IntValue>123</IntValue>"
                         + "<ObjValue obj-item1=\"234\" camel_case2=\"true\"></ObjValue>"
                         + "<arrays>"
                         + "  <array>item1</array>"
                         + "  <array attr=\"v1\">item2</array>"
                         + "  <array attr=\"v2\">item3</array>"
                         + "</arrays>"
                         + "<appSettings>"
                         + "  <add key=\"snake_case\" value=\"false\" />"
                         + "  <add key=\"camelCase\" value=\"567\" />"
                         + "</appSettings>"
                         + "<connectionStrings>"
                         + "  <add name=\"snake_case2\" connectionString=\"false\" />"
                         + "  <add name=\"camelCase2\" connectionString=\"789\" />"
                         + "</connectionStrings>"
                         + "</data>";

            dynamic walker = DataWalker.Parse(xml);
            Assert.IsTrue(walker is DynamicXml);

            Assert.AreEqual(walker.IntValue, 123);
            Assert.AreEqual(walker.ObjValue.ObjItem1Attr, 234);
            Assert.AreEqual(walker.ObjValue.CamelCase2Attr, true);
            Assert.AreEqual(walker.Arrays.Array[0].Text, "item1");
            Assert.AreEqual(walker.Arrays.Array[1].Value, "item2");
            Assert.AreEqual(walker.Arrays.Array[2].Text, "item3");
            Assert.AreEqual(walker.AppSettings.SnakeCase, false);
            Assert.AreEqual(walker.AppSettings.CamelCase, 567);
            Assert.AreEqual(walker.ConnectionStrings.SnakeCase2, "false");
            Assert.AreEqual(walker.ConnectionStrings.CamelCase2, "789");
            Assert.IsNull(walker.NonExisting);

            IEnumerable<object> elements = walker.Arrays.Array.AsEnumerable();
            Assert.AreEqual(elements.Count(), 3);
            Assert.AreEqual(((dynamic)elements.First()).Text, "item1");
            Assert.AreEqual(((dynamic)elements.Last()).Text, "item3");
        }

        [TestMethod]
        public void TestNameValues()
        {
            string nameValues = "Item1=value1\r\n"
                + "\r\n"
                + "item2=true\r\n"
                + "camelCase=123\r\n"
                + "snake_case=value4\r\n";

            dynamic walker = DataWalker.Parse(nameValues);
            Assert.IsTrue(walker is DynamicNameValue);

            Assert.AreEqual(walker.Item1, "value1");
            Assert.AreEqual(walker.Item2, true);
            Assert.AreEqual(walker.CamelCase, 123);
            Assert.AreEqual(walker.SnakeCase, "value4");
            Assert.IsNull(walker.NonExisting);

            List<KeyValuePair<string, string>> elements = walker.AsEnumerable();
            Assert.AreEqual(elements.Count(), 4);

            Assert.IsTrue(elements.Any(w => w.Key == "Item1" && w.Value == "value1"));
            Assert.IsTrue(elements.Any(w => w.Key == "snake_case" && w.Value == "value4"));
        }

        [TestMethod]
        public void TestNameValuesAmpersand()
        {
            string nameValues = "Item1=value1&item2=123&camelCase=false&snake_case=value4";

            dynamic walker = DataWalker.Parse(nameValues);
            Assert.IsTrue(walker is DynamicNameValue);

            Assert.AreEqual(walker.Item1, "value1");
            Assert.AreEqual(walker.Item2, 123);
            Assert.AreEqual(walker.CamelCase, false);
            Assert.AreEqual(walker.SnakeCase, "value4");
            Assert.IsNull(walker.NonExisting);

            List<KeyValuePair<string, string>> elements = walker.AsEnumerable();
            Assert.AreEqual(elements.Count(), 4);

            Assert.IsTrue(elements.Any(w => w.Key == "Item1" && w.Value == "value1"));
            Assert.IsTrue(elements.Any(w => w.Key == "snake_case" && w.Value == "value4"));
        }
    }
}
