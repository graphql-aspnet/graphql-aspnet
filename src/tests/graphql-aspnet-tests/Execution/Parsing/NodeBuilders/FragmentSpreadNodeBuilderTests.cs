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
    public class FragmentSpreadNodeBuilderTests
    {
        [Test]
        public void FragmentName_ParsesCorrectly()
        {
            var text = "...someFragmentA,";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FragmentSpreadNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.FragmentSpread,
                    "someFragmentA"));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void WithDirective_ParsesCorrectly()
        {
            var text = "...someFragmentA @skip(if: true),";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FragmentSpreadNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.FragmentSpread,
                    "someFragmentA",
                    new SynNodeTestCase(
                        SyntaxNodeType.Directive,
                        "skip",
                        new SynNodeTestCase(
                            SyntaxNodeType.InputItemCollection))));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void NoName_ThrowsEceptions()
        {
            var text = "...,";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                FragmentSpreadNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
        public void InvalidName_ThrowsEceptions()
        {
            var text = "...123A,";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                FragmentSpreadNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
        public void FragmentNode_ValidInlineFragment_NoFields_ParsesCorrectly()
        {
            var text = "...on User{},";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FragmentSpreadNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.InlineFragment,
                    "User",
                    new SynNodeTestCase(
                        SyntaxNodeType.FieldCollection)));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void FragmentNode_ValidInlineFragment_NoFields_WithDirective_ParsesCorrectly()
        {
            var text = "...on User @skip(if: true) {},";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FragmentSpreadNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.InlineFragment,
                    "User",
                    new SynNodeTestCase(
                        SyntaxNodeType.Directive,
                        "skip"),
                    new SynNodeTestCase(
                        SyntaxNodeType.FieldCollection)));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void FragmentNode_ValidInlineFragment_WithFields_ParsesCorrectly()
        {
            var text = "...on User{field1, field2},";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FragmentSpreadNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.InlineFragment,
                    "User",
                    new SynNodeTestCase(
                        SyntaxNodeType.FieldCollection,
                        new SynNodeTestCase(
                            SyntaxNodeType.Field,
                            "field1",
                            "field1"),
                        new SynNodeTestCase(
                            SyntaxNodeType.Field,
                            "field2",
                            "field2"))));

            SyntaxTreeOperations.Release(ref tree);
        }
    }
}