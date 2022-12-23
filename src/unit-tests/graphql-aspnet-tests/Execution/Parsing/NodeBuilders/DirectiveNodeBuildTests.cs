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
    using GraphQL.AspNet.Execution.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Execution.Parsing.NodeBuilders;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Execution.Parsing.Helpers;
    using NUnit.Framework;

    [TestFixture]
    public class DirectiveNodeBuildTests
    {
        [Test]
        public void DirectiveNodeBuilder_SimpleDirective_NoArgs_ParsesCorrectly()
        {
            var text = "@skip";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.WithDocumentRoot();
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

            var tree = SyntaxTree.WithDocumentRoot();
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

            var tree = SyntaxTree.WithDocumentRoot();
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
            Assert.IsTrue((bool)stream.Match(TokenType.CurlyBraceLeft));
            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void DirectiveNodeMaker_NoAtSign_ThrowsException()
        {
            var text = "skip(if: true)";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.WithDocumentRoot();
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

            var tree = SyntaxTree.WithDocumentRoot();
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

            var tree = SyntaxTree.WithDocumentRoot();
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

            var tree = SyntaxTree.WithDocumentRoot();
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