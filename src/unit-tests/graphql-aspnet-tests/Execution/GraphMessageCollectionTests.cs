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
    public class GraphMessageCollectionTests
    {
        [Test]
        public void GetEnumerator_EnumeratesList()
        {
            var collection = new GraphMessageCollection();
            collection.Critical("test message 1", "test code");
            collection.Critical("test message 2", "test code");

            var i = 0;
            foreach (var item in collection)
                i++;

            Assert.AreEqual(2, i);
        }

        [Test]
        public void AddTrace_YieldsMessage()
        {
            var collection = new GraphMessageCollection();
            collection.Trace("test message", "test code");

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(GraphMessageSeverity.Trace, collection[0].Severity);
            Assert.AreEqual("test message", collection[0].Message);
            Assert.AreEqual("test code", collection[0].Code);
        }

        [Test]
        public void AddDebug_YieldsMessage()
        {
            var collection = new GraphMessageCollection();
            collection.Debug("test message", "test code");

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(GraphMessageSeverity.Debug, collection[0].Severity);
            Assert.AreEqual("test message", collection[0].Message);
            Assert.AreEqual("test code", collection[0].Code);
        }

        [Test]
        public void AddInfo_YieldsMessage()
        {
            var collection = new GraphMessageCollection();
            collection.Info("test message", "test code");

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(GraphMessageSeverity.Information, collection[0].Severity);
            Assert.AreEqual("test message", collection[0].Message);
            Assert.AreEqual("test code", collection[0].Code);
        }

        [Test]
        public void AddWarning_YieldsMessage()
        {
            var collection = new GraphMessageCollection();
            collection.Warn("test message", "test code");

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(GraphMessageSeverity.Warning, collection[0].Severity);
            Assert.AreEqual("test message", collection[0].Message);
            Assert.AreEqual("test code", collection[0].Code);
        }

        [Test]
        public void AddCritical_YieldsMessage()
        {
            var collection = new GraphMessageCollection();
            collection.Critical("test message", "test code");

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, collection[0].Severity);
            Assert.AreEqual("test message", collection[0].Message);
            Assert.AreEqual("test code", collection[0].Code);
        }

        [Test]
        public void AddRange_WhenCapacityIsLessThanNewNeededSize_CapacityIsIncreased()
        {
            var collection = new GraphMessageCollection(1);
            Assert.AreEqual(1, collection.Capacity);

            var otherCollection = new GraphMessageCollection();
            otherCollection.Critical("1 Message");
            otherCollection.Info("2 message");

            collection.AddRange(otherCollection);

            Assert.AreEqual(2, collection.Capacity);
        }

        [Test]
        public void AddRange_WhenCapacityEqualsNewCount_CapacityIsUnAltered()
        {
            var collection = new GraphMessageCollection(2);
            Assert.AreEqual(2, collection.Capacity);

            var otherCollection = new GraphMessageCollection();
            otherCollection.Critical("1 Message");
            otherCollection.Info("2 message");

            collection.AddRange(otherCollection);

            Assert.AreEqual(2, collection.Capacity);
        }

        [Test]
        public void AddRange_WhenNoCapacityIsSet_CapacityIsIncreased()
        {
            var collection = new GraphMessageCollection();
            Assert.AreEqual(0, collection.Capacity);

            var otherCollection = new GraphMessageCollection();
            otherCollection.Critical("1 Message");
            otherCollection.Info("2 message");

            collection.AddRange(otherCollection);

            Assert.AreEqual(2, collection.Capacity);
        }

        [Test]
        public void AddRange_WhenNoCapacityIsSet_CapacityIsIncreased2()
        {
            var collection = new GraphMessageCollection();
            Assert.AreEqual(0, collection.Capacity);

            var otherCollection = new GraphMessageCollection();
            otherCollection.Info("1 Message");
            otherCollection.Info("2 message");
            otherCollection.Info("3 message");
            otherCollection.Info("4 message");
            otherCollection.Info("5 message");
            otherCollection.Info("6 message");
            otherCollection.Info("7 message");
            otherCollection.Info("8 message");
            otherCollection.Info("9 message");

            collection.AddRange(otherCollection);

            // next power of 2 after 9
            Assert.AreEqual(16, collection.Capacity);
        }

        [Test]
        public void AddRange_WhenCapacityIsGreaterThanNewCount_CapacityIsUnAltered()
        {
            var collection = new GraphMessageCollection(16);
            Assert.AreEqual(16, collection.Capacity);

            var otherCollection = new GraphMessageCollection();
            otherCollection.Critical("1 Message");
            otherCollection.Info("2 message");

            collection.AddRange(otherCollection);

            Assert.AreEqual(16, collection.Capacity);
        }
    }
}