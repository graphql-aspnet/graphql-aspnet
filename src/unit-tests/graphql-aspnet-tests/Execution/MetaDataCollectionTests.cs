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
        public void CloneCollections_AllKeysIncluded()
        {
            var collection = new MetaDataCollection();

            collection.TryAdd("key1", "value1");
            collection.TryAdd("key2", "value2");

            var collection2 = collection.Clone();

            Assert.AreEqual(2, collection2.Count);
            Assert.AreEqual("value1", collection2["key1"]);
            Assert.AreEqual("value2", collection2["key2"]);
        }

        [Test]
        public void CloneIsDisconnectedFromSource()
        {
            var collection = new MetaDataCollection();

            collection.TryAdd("key1", "value1");
            collection.TryAdd("key2", "value2");

            var collection2 = collection.Clone();

            // add a key after cloning
            collection.TryAdd("key3", "value2");

            // collection2 does not get the key
            Assert.AreEqual(2, collection2.Count);
        }
    }
}