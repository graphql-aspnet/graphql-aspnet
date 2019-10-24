// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Lexing.NodeMakers
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing.NodeMakers.FieldMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using NUnit.Framework;

    /// <summary>
    /// Tests centered around creation of a single field node within a collection.
    /// </summary>
    [TestFixture]
    public class FieldNodeMakerTests
    {
        [Test]
        public void FieldNodeMaker_InvalidFieldName_ThrowsException()
        {
            var text = "$field1,";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() =>
            {
               FieldNodeMaker.Instance.MakeNode(stream);
            });
        }

        [Test]
        public void FieldNodeMaker_FieldNameIsNumber_ThrowsException()
        {
            var text = "1234,";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() =>
            {
                FieldNodeMaker.Instance.MakeNode(stream);
            });
        }

        [Test]
        public void FieldNodeMaker_InvalidFieldNameWithValidAlias_ThrowsException()
        {
            var text = "fieldA: $field1,";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() =>
            {
                FieldNodeMaker.Instance.MakeNode(stream);
            });
        }

        [Test]
        public void FieldNodeMaker_SingleFieldNoInputs_ParsesCorrectly()
        {
            var text = "field1,";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = FieldNodeMaker.Instance.MakeNode(stream) as FieldNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("field1", node.FieldAlias.ToString());
            Assert.AreEqual("field1", node.FieldName.ToString());
            Assert.AreEqual(0, node.Children.Count);
        }

        [Test]
        public void FieldNodeMaker_FieldWithAliasNoInputs_ParsesCorrectly()
        {
            var text = "fieldA: field1,";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = FieldNodeMaker.Instance.MakeNode(stream) as FieldNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("fieldA", node.FieldAlias.ToString());
            Assert.AreEqual("field1", node.FieldName.ToString());
            Assert.AreEqual(0, node.Children.Count);
        }

        [Test]
        public void FieldNodeMaker_FieldWithEmptyFieldSet_ParsesNoFieldCollletionNode()
        {
            var text = "fieldA: field1{ },";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = FieldNodeMaker.Instance.MakeNode(stream) as FieldNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("fieldA", node.FieldAlias.ToString());
            Assert.AreEqual("field1", node.FieldName.ToString());
            Assert.AreEqual(0, node.Children.Count);
        }

        [Test]
        public void FieldNodeMaker_FieldWith1SimpleField_ParsesCorrectly()
        {
            var text = "field1{ field2, field3 },";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = FieldNodeMaker.Instance.MakeNode(stream) as FieldNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("field1", node.FieldName.ToString());
            Assert.AreEqual(1, node.Children.Count);

            var collection = node.Children[0] as FieldCollectionNode;
            Assert.IsNotNull(collection);
            Assert.AreEqual(2, collection.Children.Count);

            var childField1 = collection.Children[0] as FieldNode;
            Assert.IsNotNull(childField1);
            Assert.AreEqual("field2", childField1.FieldName.ToArray());

            var childField2 = collection.Children[1] as FieldNode;
            Assert.IsNotNull(childField2);
            Assert.AreEqual("field3", childField2.FieldName.ToString());
        }

        [Test]
        public void FieldNodeMaker_FieldWithChildFieldWithAlias_ParsesCorrectly()
        {
            var text = "field1{ fieldA: field2 },";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = FieldNodeMaker.Instance.MakeNode(stream) as FieldNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("field1", node.FieldName.ToString());
            Assert.AreEqual(1, node.Children.Count);

            var collection = node.Children[0] as FieldCollectionNode;
            Assert.IsNotNull(collection);
            Assert.AreEqual(1, collection.Children.Count);

            var childField1 = collection.Children[0] as FieldNode;
            Assert.IsNotNull(childField1);
            Assert.AreEqual("field2", childField1.FieldName.ToArray());
            Assert.AreEqual("fieldA", childField1.FieldAlias.ToArray());
        }

        [Test]
        public void FieldNodeMaker_WithInputValues_ParsesCorrectly()
        {
            var text = "field1(id: \"bob\", age: 123), ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = FieldNodeMaker.Instance.MakeNode(stream) as FieldNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("field1", node.FieldAlias.ToString());
            Assert.AreEqual("field1", node.FieldName.ToString());
            Assert.AreEqual(1, node.Children.Count);

            var items = node.Children[0] as InputItemCollectionNode;
            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.Children.Count);

            var item1 = items.Children[0] as InputItemNode;
            Assert.IsNotNull(item1);
            Assert.AreEqual("id", item1.InputName.ToString());
            Assert.AreEqual(1, item1.Children.Count);

            var value1 = item1.Children[0] as ScalarValueNode;
            Assert.IsNotNull(value1);
            Assert.AreEqual(ScalarValueType.String, value1.ValueType);
            Assert.AreEqual("\"bob\"", value1.Value.ToString());

            var item2 = items.Children[1] as InputItemNode;
            Assert.IsNotNull(item2);
            Assert.AreEqual("age", item2.InputName.ToString());
            Assert.AreEqual(1, item2.Children.Count);

            var value2 = item2.Children[0] as ScalarValueNode;
            Assert.IsNotNull(value2);
            Assert.AreEqual(ScalarValueType.Number, value2.ValueType);
            Assert.AreEqual("123", value2.Value.ToString());
        }

        [Test]
        public void FieldNodeMaker_WithSingleDirective_ParsesCorrectly()
        {
            var text = "field1(id: \"bob\", age: 123) @include(if: true), ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = FieldNodeMaker.Instance.MakeNode(stream) as FieldNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("field1", node.FieldName.ToString());
            Assert.AreEqual(2, node.Children.Count); // directive + input collection

            var item = node.Children.FirstOrDefault<DirectiveNode>();
            Assert.IsNotNull(item);
            Assert.AreEqual("include", item.DirectiveName.ToString());
        }

        [Test]
        public void FieldNodeMaker_WithMultipleDirective_ParsesCorrectly()
        {
            var text = "field1(id: \"bob\", age: 123) @include(if: true) @skip(if: false), ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = FieldNodeMaker.Instance.MakeNode(stream) as FieldNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("field1", node.FieldName.ToString());
            Assert.AreEqual(3, node.Children.Count); // input-directive, skip-directive + input collection

            var directives = node.Children.OfType<DirectiveNode>();
            Assert.AreEqual(2, directives.Count());

            Assert.IsTrue(directives.Any(x => x.DirectiveName.ToString() == "include"));
            Assert.IsTrue(directives.Any(x => x.DirectiveName.ToString() == "skip"));
        }
    }
}