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
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class FieldSourceCollectionTests
    {
        [Test]
        public void AllowedFieldIsAdded_CanBeRetrieved()
        {
            var path = new SchemaItemPath(SchemaItemCollections.Subscription, "path1/path2");
            var mock = Substitute.For<IGraphField>();
            mock.Route.Returns(path);
            mock.FieldSource.Returns(GraphFieldSource.Action);

            var o = new object();
            var collection = new FieldSourceCollection(GraphFieldSource.Action);
            collection.AddSource(mock, o);

            var found = collection.TryRetrieveSource(mock, out var result);

            Assert.IsTrue(collection.ContainsKey(mock));
            Assert.AreEqual(1, collection.Count);

            Assert.IsTrue(found);
            Assert.IsNotNull(result);
            Assert.AreEqual(o, result);
        }

        [Test]
        public void UpdatedFieldIsAdded_CanBeRetrieved()
        {
            var path = new SchemaItemPath(SchemaItemCollections.Subscription, "path1/path2");
            var mock = Substitute.For<IGraphField>();
            mock.Route.Returns(path);
            mock.FieldSource.Returns(GraphFieldSource.Action);

            var o = new object();
            var o1 = new object();

            var collection = new FieldSourceCollection(GraphFieldSource.Action);

            // add then update the source
            collection.AddSource(mock, o);
            collection.AddSource(mock, o1);

            var found = collection.TryRetrieveSource(mock, out var result);

            Assert.IsTrue(collection.ContainsKey(mock));
            Assert.AreEqual(1, collection.Count);

            Assert.IsTrue(found);
            Assert.IsNotNull(result);

            // ensure retrieved result is the second object added
            Assert.AreEqual(o1, result);
        }

        [Test]
        public void DisallowedFieldIsNotAdded_CanNotBeRetrieved()
        {
            var path = new SchemaItemPath(SchemaItemCollections.Subscription, "path1/path2");
            var mock = Substitute.For<IGraphField>();
            mock.Route.Returns(path);
            mock.FieldSource.Returns(GraphFieldSource.Method);

            var o = new object();
            var collection = new FieldSourceCollection();
            collection.AddSource(mock, o);

            var found = collection.TryRetrieveSource(mock, out var result);

            Assert.IsFalse(collection.ContainsKey(mock));
            Assert.AreEqual(0, collection.Count);

            Assert.IsFalse(found);
            Assert.IsNull(result);
        }

        [Test]
        public void UnFoundField_IsNotReturned()
        {
            var path = new SchemaItemPath(SchemaItemCollections.Subscription, "path1/path2");
            var mock = Substitute.For<IGraphField>();
            mock.Route.Returns(path);
            mock.FieldSource.Returns(GraphFieldSource.Action);

            var path1 = new SchemaItemPath(SchemaItemCollections.Subscription, "path1/path3");
            var mock1 = Substitute.For<IGraphField>();
            mock1.Route.Returns(path1);
            mock1.FieldSource.Returns(GraphFieldSource.Action);

            var o = new object();
            var collection = new FieldSourceCollection();
            collection.AddSource(mock, o);
            Assert.AreEqual(1, collection.Count);

            // retreive for an object def. not in the collection
            var found = collection.TryRetrieveSource(mock1, out var result);

            Assert.IsFalse(found);
            Assert.IsNull(result);
        }

        [Test]
        public void WhenMultipleAllowedSources_AllCanBeRetrieved()
        {
            var path = new SchemaItemPath(SchemaItemCollections.Subscription, "path1/path2");
            var mock = Substitute.For<IGraphField>();
            mock.Route.Returns(path);
            mock.FieldSource.Returns(GraphFieldSource.Method);

            var path1 = new SchemaItemPath(SchemaItemCollections.Subscription, "path1/path3");
            var mock1 = Substitute.For<IGraphField>();
            mock1.Route.Returns(path1);
            mock1.FieldSource.Returns(GraphFieldSource.Action);

            var o = new object();
            var o1 = new object();
            var collection = new FieldSourceCollection(GraphFieldSource.Action | GraphFieldSource.Method);
            collection.AddSource(mock, o);
            collection.AddSource(mock1, o1);

            var found = collection.TryRetrieveSource(mock, out var result);
            var found1 = collection.TryRetrieveSource(mock1, out var result1);

            Assert.IsTrue(collection.ContainsKey(mock));
            Assert.IsTrue(collection.ContainsKey(mock1));
            Assert.AreEqual(2, collection.Count);

            Assert.IsTrue(found);
            Assert.IsNotNull(result);
            Assert.AreEqual(o, result);

            Assert.IsTrue(found1);
            Assert.IsNotNull(result1);
            Assert.AreEqual(o1, result1);
        }
    }
}