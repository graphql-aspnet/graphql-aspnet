// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas
{
    using System;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GraphFieldCollectionTests
    {
        [Test]
        public void AddField_DuplicateFIeldNameThrowsException()
        {
            var owner = new Mock<IGraphType>();
            owner.Setup(x => x.Name).Returns("graphType");

            var field1 = new Mock<IGraphField>();
            field1.Setup(x => x.Name).Returns("field1");
            field1.Setup(x => x.TypeExpression).Returns(GraphTypeExpression.FromDeclaration("Bob!"));

            var field2 = new Mock<IGraphField>();
            field2.Setup(x => x.Name).Returns("field1");
            field2.Setup(x => x.TypeExpression).Returns(GraphTypeExpression.FromDeclaration("Bob!"));

            var collection = new GraphFieldCollection(owner.Object);
            collection.AddField(field1.Object);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                collection.AddField(field2.Object);
            });
        }

        [TestCase("field1", true)]
        [TestCase(null, false)]
        [TestCase("wrongName", false)]
        [TestCase("    ", false)] // white space
        [TestCase("FIELD1", false)] // wrong case
        public void FindField(string fieldName, bool shouldBeFound)
        {
            var owner = new Mock<IGraphType>();
            owner.Setup(x => x.Name).Returns("graphType");

            var field1 = new Mock<IGraphField>();
            field1.Setup(x => x.Name).Returns("field1");
            field1.Setup(x => x.TypeExpression).Returns(GraphTypeExpression.FromDeclaration("Bob!"));

            var collection = new GraphFieldCollection(owner.Object);
            collection.AddField(field1.Object);

            var result = collection.FindField(fieldName);

            if (shouldBeFound)
                Assert.AreEqual(field1.Object, result);
            else
                Assert.IsNull(result);
        }

        [TestCase("field1", true)]
        [TestCase(null, false)]
        [TestCase("wrongName", false)]
        [TestCase("    ", false)] // white space
        [TestCase("FIELD1", false)] // wrong case
        public void ContainsKey(string fieldName, bool shouldBeFound)
        {
            var owner = new Mock<IGraphType>();
            owner.Setup(x => x.Name).Returns("graphType");

            var field1 = new Mock<IGraphField>();
            field1.Setup(x => x.Name).Returns("field1");
            field1.Setup(x => x.TypeExpression).Returns(GraphTypeExpression.FromDeclaration("Bob!"));

            var collection = new GraphFieldCollection(owner.Object);
            collection.AddField(field1.Object);

            var result = collection.ContainsKey(fieldName);

            Assert.AreEqual(shouldBeFound, result);
        }

        [TestCase("field1", true)]
        [TestCase(null, false)]
        [TestCase("wrongName", false)]
        [TestCase("    ", false)] // white space
        [TestCase("FIELD1", false)] // wrong case
        public void ThisByName(string fieldName, bool shouldBeFound)
        {
            var owner = new Mock<IGraphType>();
            owner.Setup(x => x.Name).Returns("graphType");

            var field1 = new Mock<IGraphField>();
            field1.Setup(x => x.Name).Returns("field1");
            field1.Setup(x => x.TypeExpression).Returns(GraphTypeExpression.FromDeclaration("Bob!"));

            var collection = new GraphFieldCollection(owner.Object);
            collection.AddField(field1.Object);

            if (shouldBeFound)
            {
                var result = collection[fieldName];
                Assert.AreEqual(field1.Object, result);
            }
            else
            {
                Assert.Throws(
                    Is.InstanceOf<Exception>(),
                    () =>
                    {
                        var item = collection[fieldName];
                    });
            }
        }

        [Test]
        public void Contains_ForReferencedField_IsFound()
        {
            var owner = new Mock<IGraphType>();
            owner.Setup(x => x.Name).Returns("graphType");

            var field1 = new Mock<IGraphField>();
            field1.Setup(x => x.Name).Returns("field1");
            field1.Setup(x => x.TypeExpression).Returns(GraphTypeExpression.FromDeclaration("Bob!"));

            var collection = new GraphFieldCollection(owner.Object);
            collection.AddField(field1.Object);

            var result = collection.Contains(field1.Object);
            Assert.IsTrue(result);
        }

        [Test]
        public void Contains_ForUnReferencedField_IsNotFound()
        {
            var owner = new Mock<IGraphType>();
            owner.Setup(x => x.Name).Returns("graphType");

            var field1 = new Mock<IGraphField>();
            field1.Setup(x => x.Name).Returns("field1");
            field1.Setup(x => x.TypeExpression).Returns(GraphTypeExpression.FromDeclaration("Bob!"));

            var field1Other = new Mock<IGraphField>();
            field1Other.Setup(x => x.Name).Returns("field1");
            field1Other.Setup(x => x.TypeExpression).Returns(GraphTypeExpression.FromDeclaration("Bob!"));

            var collection = new GraphFieldCollection(owner.Object);
            collection.AddField(field1.Object);

            var result = collection.Contains(field1Other.Object);
            Assert.IsFalse(result);
        }
    }
}