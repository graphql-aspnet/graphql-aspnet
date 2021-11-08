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
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DefaultFieldSourceCollectionTests
    {
        [Test]
        public void AllowedFieldIsAdded_CanBeRetrieved()
        {
            var path = new GraphFieldPath(GraphCollection.Subscription, "path1/path2");
            var mock = new Mock<IGraphField>();
            mock.Setup(x => x.Route).Returns(path);
            mock.Setup(x => x.FieldSource).Returns(GraphFieldSource.Action);

            var o = new object();
            var collection = new DefaultFieldSourceCollection(GraphFieldSource.Action);
            collection.AddSource(mock.Object, o);

            var found = collection.TryRetrieveSource(mock.Object, out var result);

            Assert.IsTrue(collection.ContainsKey(mock.Object));
            Assert.AreEqual(1, collection.Count);

            Assert.IsTrue(found);
            Assert.IsNotNull(result);
            Assert.AreEqual(o, result);
        }

        [Test]
        public void UpdatedFieldIsAdded_CanBeRetrieved()
        {
            var path = new GraphFieldPath(GraphCollection.Subscription, "path1/path2");
            var mock = new Mock<IGraphField>();
            mock.Setup(x => x.Route).Returns(path);
            mock.Setup(x => x.FieldSource).Returns(GraphFieldSource.Action);

            var o = new object();
            var o1 = new object();

            var collection = new DefaultFieldSourceCollection(GraphFieldSource.Action);

            // add then update the source
            collection.AddSource(mock.Object, o);
            collection.AddSource(mock.Object, o1);

            var found = collection.TryRetrieveSource(mock.Object, out var result);

            Assert.IsTrue(collection.ContainsKey(mock.Object));
            Assert.AreEqual(1, collection.Count);

            Assert.IsTrue(found);
            Assert.IsNotNull(result);

            // ensure retrieved result is the second object added
            Assert.AreEqual(o1, result);
        }

        [Test]
        public void DisallowedFieldIsNotAdded_CanNotBeRetrieved()
        {
            var path = new GraphFieldPath(GraphCollection.Subscription, "path1/path2");
            var mock = new Mock<IGraphField>();
            mock.Setup(x => x.Route).Returns(path);
            mock.Setup(x => x.FieldSource).Returns(GraphFieldSource.Method);

            var o = new object();
            var collection = new DefaultFieldSourceCollection();
            collection.AddSource(mock.Object, o);

            var found = collection.TryRetrieveSource(mock.Object, out var result);

            Assert.IsFalse(collection.ContainsKey(mock.Object));
            Assert.AreEqual(0, collection.Count);

            Assert.IsFalse(found);
            Assert.IsNull(result);
        }

        [Test]
        public void UnFoundField_IsNotReturned()
        {
            var path = new GraphFieldPath(GraphCollection.Subscription, "path1/path2");
            var mock = new Mock<IGraphField>();
            mock.Setup(x => x.Route).Returns(path);
            mock.Setup(x => x.FieldSource).Returns(GraphFieldSource.Action);

            var path1 = new GraphFieldPath(GraphCollection.Subscription, "path1/path3");
            var mock1 = new Mock<IGraphField>();
            mock1.Setup(x => x.Route).Returns(path1);
            mock1.Setup(x => x.FieldSource).Returns(GraphFieldSource.Action);

            var o = new object();
            var collection = new DefaultFieldSourceCollection();
            collection.AddSource(mock.Object, o);
            Assert.AreEqual(1, collection.Count);

            // retreive for an object def. not in the collection
            var found = collection.TryRetrieveSource(mock1.Object, out var result);

            Assert.IsFalse(found);
            Assert.IsNull(result);
        }

        [Test]
        public void WhenMultipleAllowedSources_AllCanBeRetrieved()
        {
            var path = new GraphFieldPath(GraphCollection.Subscription, "path1/path2");
            var mock = new Mock<IGraphField>();
            mock.Setup(x => x.Route).Returns(path);
            mock.Setup(x => x.FieldSource).Returns(GraphFieldSource.Method);

            var path1 = new GraphFieldPath(GraphCollection.Subscription, "path1/path3");
            var mock1 = new Mock<IGraphField>();
            mock1.Setup(x => x.Route).Returns(path1);
            mock1.Setup(x => x.FieldSource).Returns(GraphFieldSource.Action);

            var o = new object();
            var o1 = new object();
            var collection = new DefaultFieldSourceCollection(GraphFieldSource.Action | GraphFieldSource.Method);
            collection.AddSource(mock.Object, o);
            collection.AddSource(mock1.Object, o1);

            var found = collection.TryRetrieveSource(mock.Object, out var result);
            var found1 = collection.TryRetrieveSource(mock1.Object, out var result1);

            Assert.IsTrue(collection.ContainsKey(mock.Object));
            Assert.IsTrue(collection.ContainsKey(mock1.Object));
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