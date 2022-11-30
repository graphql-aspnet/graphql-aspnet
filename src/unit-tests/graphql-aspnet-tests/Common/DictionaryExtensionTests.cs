// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Common
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Common.Extensions;
    using NUnit.Framework;

    [TestFixture]
    public class DictionaryExtensionTests
    {
        [Test]
        public void DictionaryValuesAreUpdated()
        {
            var dic = new Dictionary<string, string>();
            dic.Add("key1", "value1");
            dic.Add("key2", "value2");

            dic.ForEach((key, value) => value + "1");

            Assert.AreEqual("value11", dic["key1"]);
            Assert.AreEqual("value21", dic["key2"]);
        }

        [Test]
        public void DictionaryValuesAreNotAlteredWhenActionUsed()
        {
            var dic = new Dictionary<string, string>();
            dic.Add("key1", "value1");
            dic.Add("key2", "value2");

            dic.ForEach((key, value) =>
            {
                value = value + "1";
            });

            Assert.AreEqual("value1", dic["key1"]);
            Assert.AreEqual("value2", dic["key2"]);
        }

        [Test]
        public void DictionaryValuesAreNotAlteredWhenActionUsed2()
        {
            var dic = new Dictionary<string, List<int>>();
            dic.Add("key1", new List<int>() { 1, 2, 3 });
            dic.Add("key2", new List<int>() { 1, 2, 3 });

            dic.ForEach((key, value) => value.Clear());

            Assert.IsEmpty(dic["key1"]);
            Assert.IsEmpty(dic["key2"]);
        }
    }
}
