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
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class InputInputGraphFieldCollectionTests
    {
        [Test]
        public void AddField_DuplicateFIeldNameThrowsException()
        {
            var field1 = Substitute.For<IInputGraphField>();
            field1.Name.Returns("field1");
            field1.TypeExpression.Returns(GraphTypeExpression.FromDeclaration("Bob!"));

            var field2 = Substitute.For<IInputGraphField>();
            field2.Name.Returns("field1");
            field2.TypeExpression.Returns(GraphTypeExpression.FromDeclaration("Bob!"));

            var collection = new InputGraphFieldCollection();
            collection.AddField(field1);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                collection.AddField(field2);
            });
        }

        [TestCase("field1", true)]
        [TestCase(null, false)]
        [TestCase("wrongName", false)]
        [TestCase("    ", false)] // white space
        [TestCase("FIELD1", false)] // wrong case
        public void FindField(string fieldName, bool shouldBeFound)
        {
            var field1 = Substitute.For<IInputGraphField>();
            field1.Name.Returns("field1");
            field1.TypeExpression.Returns(GraphTypeExpression.FromDeclaration("Bob!"));

            var collection = new InputGraphFieldCollection();
            collection.AddField(field1);

            var result = collection.FindField(fieldName);

            if (shouldBeFound)
                Assert.AreEqual(field1, result);
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
            var field1 = Substitute.For<IInputGraphField>();
            field1.Name.Returns("field1");
            field1.TypeExpression.Returns(GraphTypeExpression.FromDeclaration("Bob!"));

            var collection = new InputGraphFieldCollection();
            collection.AddField(field1);

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
            var field1 = Substitute.For<IInputGraphField>();
            field1.Name.Returns("field1");
            field1.TypeExpression.Returns(GraphTypeExpression.FromDeclaration("Bob!"));

            var collection = new InputGraphFieldCollection();
            collection.AddField(field1);

            if (shouldBeFound)
            {
                var result = collection[fieldName];
                Assert.AreEqual(field1, result);
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
            var field1 = Substitute.For<IInputGraphField>();
            field1.Name.Returns("field1");
            field1.TypeExpression.Returns(GraphTypeExpression.FromDeclaration("Bob!"));

            var collection = new InputGraphFieldCollection();
            collection.AddField(field1);

            var result = collection.Contains(field1);
            Assert.IsTrue(result);
        }

        [Test]
        public void Contains_ForUnReferencedField_IsNotFound()
        {
            var field1 = Substitute.For<IInputGraphField>();
            field1.Name.Returns("field1");
            field1.TypeExpression.Returns(GraphTypeExpression.FromDeclaration("Bob!"));

            var field1Other = Substitute.For<IInputGraphField>();
            field1Other.Name.Returns("field1");
            field1Other.TypeExpression.Returns(GraphTypeExpression.FromDeclaration("Bob!"));

            var collection = new InputGraphFieldCollection();
            collection.AddField(field1);

            var result = collection.Contains(field1Other);
            Assert.IsFalse(result);
        }
    }
}