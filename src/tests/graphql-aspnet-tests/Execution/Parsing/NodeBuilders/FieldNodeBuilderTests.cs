// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.Parsing.NodeBuilders
{
    using System;
    using GraphQL.AspNet.Execution.Parsing;
    using GraphQL.AspNet.Execution.Parsing.Exceptions;
    using GraphQL.AspNet.Execution.Parsing.Lexing;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;
    using GraphQL.AspNet.Execution.Parsing.NodeBuilders;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Execution.Parsing.Helpers;
    using NUnit.Framework;

    [TestFixture]
    public class FieldNodeBuilderTests
    {
        [Test]
        public void InvalidFieldName_ThrowsException()
        {
            var text = "$field1,";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
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
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                FieldNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }
            finally
            {
                SyntaxTreeOperations.Release(ref tree);
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void InvalidFieldNameWithValidAlias_ThrowsException()
        {
            var text = "fieldA: $field1,";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                FieldNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }
            finally
            {
                SyntaxTreeOperations.Release(ref tree);
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void SingleFieldNoInputs_ParsesCorrectly()
        {
            var text = "field1,";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.Field,
                    "field1",
                    "field1"));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void FieldWithAliasNoInputs_ParsesCorrectly()
        {
            var text = "fieldA: field1,";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.Field,
                    "field1",
                    "fieldA"));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void FieldWithEmptyFieldSet_ParsesEmptyFieldCollletionNode()
        {
            var text = "fieldA: field1{ },";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.Field,
                    "field1",
                    "fieldA",
                    new SynNodeTestCase(
                        SyntaxNodeType.FieldCollection,
                        SynNodeTestCase.NoChildren)));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void FieldWith1SimpleField_ParsesCorrectly()
        {
            var text = "field1{ field2, field3 },";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.Field,
                    "field1",
                    "field1",
                    new SynNodeTestCase(
                        SyntaxNodeType.FieldCollection,
                        new SynNodeTestCase(
                            SyntaxNodeType.Field,
                            "field2",
                            "field2"),
                        new SynNodeTestCase(
                            SyntaxNodeType.Field,
                            "field3",
                            "field3"))));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void FieldWithChildFieldWithAlias_ParsesCorrectly()
        {
            var text = "field1{ fieldA: field2 },";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.Field,
                    "field1",
                    "field1",
                    new SynNodeTestCase(
                        SyntaxNodeType.FieldCollection,
                        new SynNodeTestCase(
                            SyntaxNodeType.Field,
                            "field2",
                            "fieldA"))));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void WithInputValues_ParsesCorrectly()
        {
            var text = "field1(id: \"bob\", age: 123), ";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.Field,
                    "field1",
                    "field1",
                    new SynNodeTestCase(
                        SyntaxNodeType.InputItemCollection,
                        new SynNodeTestCase(
                            SyntaxNodeType.InputItem,
                            "id",
                            new SynNodeTestCase(
                                SyntaxNodeType.ScalarValue,
                                "\"bob\"",
                                ScalarValueType.String)),
                        new SynNodeTestCase(
                            SyntaxNodeType.InputItem,
                            "age",
                            new SynNodeTestCase(
                                SyntaxNodeType.ScalarValue,
                                "123",
                                ScalarValueType.Number)))));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void WithSingleDirective_ParsesCorrectly()
        {
            var text = "field1(id: \"bob\", age: 123) @include(if: true), ";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.Field,
                    "field1",
                    "field1",
                    new SynNodeTestCase(
                        SyntaxNodeType.InputItemCollection),
                    new SynNodeTestCase(
                        SyntaxNodeType.Directive,
                        "include")));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void WithMultipleDirective_ParsesCorrectly()
        {
            var text = "field1(id: \"bob\", age: 123) @include(if: true) @skip(if: false), ";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.Field,
                    "field1",
                    "field1",
                    new SynNodeTestCase(
                        SyntaxNodeType.InputItemCollection),
                    new SynNodeTestCase(
                        SyntaxNodeType.Directive,
                        "include"),
                    new SynNodeTestCase(
                        SyntaxNodeType.Directive,
                        "skip")));

            SyntaxTreeOperations.Release(ref tree);
        }
    }
}