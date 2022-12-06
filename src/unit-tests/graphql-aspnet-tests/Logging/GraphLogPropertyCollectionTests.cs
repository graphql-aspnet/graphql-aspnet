// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Logging
{
    using System;
    using System.Collections;
    using System.Linq;
    using GraphQL.AspNet.Logging.Common;
    using NUnit.Framework;

    [TestFixture]
    public class GraphLogPropertyCollectionTests
    {
        [Test]
        public void AddNewProperty_AddedToEnumerableCollection()
        {
            var props = new GraphLogPropertyCollection();
            var beforeNewProp = props.Count();

            props.AddProperty("key1", "value1");
            Assert.AreEqual(beforeNewProp + 1, props.Count());
        }

        [Test]
        public void AddNewProperty_WhenKeyExists_KeysDoNotClash()
        {
            var props = new GraphLogPropertyCollection();
            var beforeNewProp = props.Count();

            props.AddProperty("key1", "value1");
            props.AddProperty("key1", "value2");

            Assert.AreEqual(beforeNewProp + 2, props.Count());
            Assert.AreEqual(2, props.Count(x => x.Key.StartsWith("key1")));
            Assert.IsTrue(props.ContainsKey("key1"));
            Assert.IsTrue(props.ContainsKey("key1_1"));
        }

        [Test]
        public void ContainsKey()
        {
            var props = new GraphLogPropertyCollection();

            props.AddProperty("key1", "value1");
            Assert.IsTrue(props.ContainsKey("key1"));
            Assert.IsFalse(props.ContainsKey("key2"));
        }

        [Test]
        public void RetrieveByKey_ReturnsValue()
        {
            var props = new GraphLogPropertyCollection();

            props.AddProperty("key1", "value1");
            Assert.AreEqual("value1", props["key1"]);
        }

        [Test]
        public void RetrieveProperty_ReturnsValueWhenCastable()
        {
            var props = new GraphLogPropertyCollection();

            props.AddProperty("key1", "value1");
            Assert.AreEqual("value1", props.RetrieveProperty("key1"));
        }

        [Test]
        public void RetrieveProperty_ReturnsDefaultWhenNull()
        {
            var props = new GraphLogPropertyCollection();
            Assert.AreEqual(default(int), props.RetrieveProperty<int>("key1"));
        }

        [Test]
        public void RetrieveProperty_ExceptionWhenNotCastable()
        {
            var props = new GraphLogPropertyCollection();
            Assert.Throws<InvalidCastException>(() =>
            {
                props.AddProperty("key1", "string value");
                Assert.AreEqual(default(int), props.RetrieveProperty<int>("key1"));
            });
        }

        [Test]
        public void IEnumerable_IteratesAllProps()
        {
            var props = new GraphLogPropertyCollection();
            props.AddProperty("key1", "string value");
            var count = props.Count();

            // yes this is a dumb test but we're aiming for 100% code coverage on
            // logging related items...its too important.
            var enumerableCount = 0;
            foreach (var item in (IEnumerable)props)
                enumerableCount++;

            Assert.AreEqual(count, enumerableCount);
        }

        [Test]
        public void FlattenKeyList_YieldsCorrectKeyNames()
        {
            var entry = new GraphLogPropertyCollection();
            var subKeySet = new GraphLogPropertyCollection();
            subKeySet.AddProperty("key1", "value1");

            entry.AddProperty("subKeys", subKeySet);
            entry.AddProperty("key1", "value1");

            var flat = entry.FlattenProperties();

            Assert.AreEqual(2, flat.Count());
            Assert.IsTrue(flat.Any(x => x.Key == "subKeys_key1"));
            Assert.IsTrue(flat.Any(x => x.Key == "key1"));
        }
    }
}