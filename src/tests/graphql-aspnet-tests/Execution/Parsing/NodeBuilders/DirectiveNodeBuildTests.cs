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
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Parsing2.Lexing;
    using GraphQL.AspNet.Parsing2.Exceptions;
    using GraphQL.AspNet.Parsing2.Lexing.Source;
    using GraphQL.AspNet.Parsing2.Lexing.Tokens;
    using GraphQL.AspNet.Parsing2;
    using GraphQL.AspNet.Parsing2.NodeBuilders;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using NUnit.Framework;
    using GraphQL.AspNet.Tests.Parsing2.Helpers;

    [TestFixture]
    public class DirectiveNodeBuildTests
    {
        [Test]
        public void DirectiveNodeBuilder_SimpleDirective_NoArgs_ParsesCorrectly()
        {
            var text = "@skip";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            DirectiveNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.Directive,
                    "skip"));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void DirectiveNodeBuilder_DirectiveWithArgument_ParsesCorrectly()
        {
            var text = "@skip(if: true)";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            DirectiveNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.Directive,
                    "skip",
                    new SynNodeTestCase(
                        SyntaxNodeType.InputItemCollection,
                        new SynNodeTestCase(
                            SyntaxNodeType.InputItem,
                            "if",
                            new SynNodeTestCase(
                                SyntaxNodeType.ScalarValue,
                                "true",
                                ScalarValueType.Boolean)))));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void DirectiveNodeBuilder_WithArguments_WithFieldsAfter_ParsesCorrectly()
        {
            var text = "@skip(if: true){field1, field2}";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            DirectiveNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.Directive,
                    "skip",
                    new SynNodeTestCase(
                        SyntaxNodeType.InputItemCollection,
                        new SynNodeTestCase(
                            SyntaxNodeType.InputItem,
                            "if",
                            new SynNodeTestCase(
                                SyntaxNodeType.ScalarValue,
                                "true",
                                ScalarValueType.Boolean)))));

            // stream sits at curly brace for field colleciton
            Assert.IsTrue(stream.Match(TokenType.CurlyBraceLeft));
            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void DirectiveNodeMaker_NoAtSign_ThrowsException()
        {
            var text = "skip(if: true)";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                DirectiveNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
        public void DirectiveNodeBuilder_NoDirectiveName_ThrowsException()
        {
            var text = "@(if: true)";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                DirectiveNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
        public void DirectiveNodeBuilder_NoAtSignAndNoName_ThrowsException()
        {
            var text = "(if: true)";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                DirectiveNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
        public void DirectiveNodeMaker_NotAtADirective_ThrowsException()
        {
            var text = "query someQuery{field1, field2}";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                DirectiveNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
    }
}