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
        public void IteratorTest()
        {
            var collection = new GraphMessageCollection();
            collection.Critical("test message 1", "test code");
            collection.Critical("test message 2", "test code");

            var i = 0;
            foreach (var item in collection)
            {
                i++;
            }

            Assert.AreEqual(2, i);
        }

        [Test]
        public void Clear_EmptiesCollection()
        {
            var collection = new GraphMessageCollection();
            collection.Critical("test message", "test code");

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, collection.Severity);

            collection.Clear();
            Assert.AreEqual(0, collection.Count);
            Assert.AreEqual(GraphMessageSeverity.Trace, collection.Severity);
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
    }
}