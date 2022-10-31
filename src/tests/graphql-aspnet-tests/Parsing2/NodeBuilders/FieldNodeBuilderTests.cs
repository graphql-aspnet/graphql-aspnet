// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Parsing2.NodeBuilders
{
    using System;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing2;
    using GraphQL.AspNet.Parsing2.NodeBuilders;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class FieldNodeBuilderTests
    {
        [Test]
        public void InvalidFieldName_ThrowsException()
        {
            var text = "$field1,";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                FieldNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void FieldNameIsNumber_ThrowsException()
        {
            var text = "1234,";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                FieldNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void InvalidFieldNameWithValidAlias_ThrowsException()
        {
            var text = "fieldA: $field1,";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                FieldNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void SingleFieldNoInputs_ParsesCorrectly()
        {
            var text = "field1,";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.Field,
                    "field1",
                    "field1"));
        }

        [Test]
        public void FieldWithAliasNoInputs_ParsesCorrectly()
        {
            var text = "fieldA: field1,";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.Field,
                    "field1",
                    "fieldA"));
        }

        [Test]
        public void FieldWithEmptyFieldSet_ParsesEmptyFieldCollletionNode()
        {
            var text = "fieldA: field1{ },";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.Field,
                    "field1",
                    "fieldA",
                    new SynNodeTestCase(
                        SynNodeType.FieldCollection,
                        SynNodeTestCase.NoChildren)));
        }

        [Test]
        public void FieldWith1SimpleField_ParsesCorrectly()
        {
            var text = "field1{ field2, field3 },";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.Field,
                    "field1",
                    "field1",
                    new SynNodeTestCase(
                        SynNodeType.FieldCollection,
                        new SynNodeTestCase(
                            SynNodeType.Field,
                            "field2",
                            "field2"),
                        new SynNodeTestCase(
                            SynNodeType.Field,
                            "field3",
                            "field3"))));
        }

        [Test]
        public void FieldWithChildFieldWithAlias_ParsesCorrectly()
        {
            var text = "field1{ fieldA: field2 },";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.Field,
                    "field1",
                    "field1",
                    new SynNodeTestCase(
                        SynNodeType.FieldCollection,
                        new SynNodeTestCase(
                            SynNodeType.Field,
                            "field2",
                            "fieldA"))));
        }

        [Test]
        public void WithInputValues_ParsesCorrectly()
        {
            var text = "field1(id: \"bob\", age: 123), ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.Field,
                    "field1",
                    "field1",
                    new SynNodeTestCase(
                        SynNodeType.InputItemCollection,
                        new SynNodeTestCase(
                            SynNodeType.InputItem,
                            "id",
                            new SynNodeTestCase(
                                SynNodeType.ScalarValue,
                                "\"bob\"",
                                ScalarValueType.String)),
                        new SynNodeTestCase(
                            SynNodeType.InputItem,
                            "age",
                            new SynNodeTestCase(
                                SynNodeType.ScalarValue,
                                "123",
                                ScalarValueType.Number)))));
        }

        [Test]
        public void WithSingleDirective_ParsesCorrectly()
        {
            var text = "field1(id: \"bob\", age: 123) @include(if: true), ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.Field,
                    "field1",
                    "field1",
                    new SynNodeTestCase(
                        SynNodeType.InputItemCollection),
                    new SynNodeTestCase(
                        SynNodeType.Directive,
                        "include")));
        }

        [Test]
        public void WithMultipleDirective_ParsesCorrectly()
        {
            var text = "field1(id: \"bob\", age: 123) @include(if: true) @skip(if: false), ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.Field,
                    "field1",
                    "field1",
                    new SynNodeTestCase(
                        SynNodeType.InputItemCollection),
                    new SynNodeTestCase(
                        SynNodeType.Directive,
                        "include"),
                    new SynNodeTestCase(
                        SynNodeType.Directive,
                        "skip")));
        }
    }
}