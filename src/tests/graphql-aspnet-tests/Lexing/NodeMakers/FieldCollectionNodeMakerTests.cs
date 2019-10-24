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
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing.NodeMakers.FieldMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using NUnit.Framework;

    /// <summary>
    /// Defines tests relating to the parsing of a <see cref="FieldCollectionNode"/> from
    /// a token stream.
    /// </summary>
    [TestFixture]
    public class FieldCollectionNodeMakerTests
    {
        [Test]
        public void FieldCollectionNodeMaker_EmptyCollection_ParsesNoChildren()
        {
            var text = "{  }";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = FieldCollectionNodeMaker.Instance.MakeNode(stream) as FieldCollectionNode;
            Assert.IsNotNull(node);
            Assert.AreEqual(0, node.Children.Count);
        }

        [Test]
        public void FieldCollectionNodeMaker_NoPointingtoStartofCollection_ThrowsException()
        {
            var text = "query testQuery{  }";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() => { FieldCollectionNodeMaker.Instance.MakeNode(stream); });
        }

        [Test]
        public void FieldCollectionNodeMaker_NoEndingCurlyBrace_ThrowsException()
        {
            var text = "{field1, field2, field3,field4";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() => { FieldCollectionNodeMaker.Instance.MakeNode(stream); });
        }

        [Test]
        public void FieldCollectionNodeMaker_InvalidFieldName_ThrowsException()
        {
            var text = "{$field2}";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() => { FieldCollectionNodeMaker.Instance.MakeNode(stream); });
        }

        [Test]
        public void FieldCollectionNodeMaker_AssignedFieldAlias_IsSetCorrectly()
        {
            var text = "{field1, fieldA: field2}";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = FieldCollectionNodeMaker.Instance.MakeNode(stream) as FieldCollectionNode;
            Assert.IsNotNull(node);
            Assert.AreEqual(2, node.Children.Count);

            var field = node.Children[1] as FieldNode;
            Assert.IsNotNull(field);
            Assert.AreEqual("field2", field.FieldName.ToString());
            Assert.AreEqual("fieldA", field.FieldAlias.ToString());
        }

        [Test]
        public void FieldCollectionNodeMaker_SimpleCollection_ParsesCorrectly()
        {
            var text = "{field1, field2, field3}";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = FieldCollectionNodeMaker.Instance.MakeNode(stream) as FieldCollectionNode;
            Assert.IsNotNull(node);
            Assert.AreEqual(3, node.Children.Count);

            var field = node.Children[0] as FieldNode;
            Assert.IsNotNull(field);
            Assert.AreEqual("field1", field.FieldName.ToString());

            field = node.Children[1] as FieldNode;
            Assert.IsNotNull(field);
            Assert.AreEqual("field2", field.FieldName.ToString());

            field = node.Children[2] as FieldNode;
            Assert.IsNotNull(field);
            Assert.AreEqual("field3", field.FieldName.ToString());
        }

        [Test]
        public void FieldCollectionNodeMaker_WithFragments_ParsesCorrectly()
        {
            var text = "{field1, ...on User{fieldA, fieldQ}, ...someFragment }";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = FieldCollectionNodeMaker.Instance.MakeNode(stream) as FieldCollectionNode;
            Assert.IsNotNull(node);
            Assert.AreEqual(3, node.Children.Count);

            var field = node.Children[0] as FieldNode;
            Assert.IsNotNull(field);
            Assert.AreEqual("field1", field.FieldName.ToString());

            var frag = node.Children[1] as FragmentNode;
            Assert.IsNotNull(frag);
            Assert.AreEqual("User", frag.TargetType.ToString());
            Assert.AreEqual(1, frag.Children.Count); // has a field collection
            Assert.AreEqual(2, frag.Children[0].Children.Count); // collection has 2 fields

            var pointer = node.Children[2] as FragmentSpreadNode;
            Assert.IsNotNull(pointer);
            Assert.AreEqual("someFragment", pointer.PointsToFragmentName.ToString());
            Assert.AreEqual(0, pointer.Children.Count);
        }
    }
}