// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution
{
    using GraphQL.AspNet.Execution;
    using NUnit.Framework;

    [TestFixture]
    public class MetaDataCollectionTests
    {
        [Test]
        public void MergingCollectionsWithNonSharedKeys_AllKeysIncluded()
        {
            var collection = new MetaDataCollection();
            var collection2 = new MetaDataCollection();

            collection.TryAdd("key1", "value1");
            collection2.TryAdd("key2", "value2");

            collection.Merge(collection2);

            Assert.AreEqual(2, collection.Count);
            Assert.AreEqual("value1", collection["key1"]);
            Assert.AreEqual("value2", collection["key2"]);

            Assert.IsFalse(collection2.ContainsKey("key1"));
        }

        [Test]
        public void MergingCollectionsWithSharedKeys_KeysInCollectionAreUpdated()
        {
            var collection = new MetaDataCollection();
            var collection2 = new MetaDataCollection();

            collection.TryAdd("key1", "value1");
            collection2.TryAdd("key1", "value2");

            Assert.AreEqual("value1", collection["key1"]);

            collection.Merge(collection2);

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual("value2", collection["key1"]);
        }

        [Test]
        public void MergingCollectionsWithNull_HasNoEffect()
        {
            var collection = new MetaDataCollection();

            collection.TryAdd("key1", "value1");

            collection.Merge(null);

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual("value1", collection["key1"]);
        }
    }
}