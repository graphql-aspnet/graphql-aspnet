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
    using GraphQL.AspNet.Parsing2;
    using GraphQL.AspNet.Parsing2.NodeBuilders;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class FragmentSpreadNodeBuilderTests
    {
        [Test]
        public void FragmentName_ParsesCorrectly()
        {
            var text = "...someFragmentA,";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FragmentSpreadNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.FragmentSpread,
                    "someFragmentA"));
        }

        [Test]
        public void WithDirective_ParsesCorrectly()
        {
            var text = "...someFragmentA @skip(if: true),";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FragmentSpreadNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.FragmentSpread,
                    "someFragmentA",
                    new SynNodeTestCase(
                        SynNodeType.Directive,
                        "skip",
                        new SynNodeTestCase(
                            SynNodeType.InputItemCollection))));
        }

        [Test]
        public void NoName_ThrowsEceptions()
        {
            var text = "...,";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                FragmentSpreadNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void InvalidName_ThrowsEceptions()
        {
            var text = "...123A,";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                FragmentSpreadNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void FragmentNode_ValidInlineFragment_NoFields_ParsesCorrectly()
        {
            var text = "...on User{},";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FragmentSpreadNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.InlineFragment,
                    "User",
                    new SynNodeTestCase(
                        SynNodeType.FieldCollection)));
        }

        [Test]
        public void FragmentNode_ValidInlineFragment_NoFields_WithDirective_ParsesCorrectly()
        {
            var text = "...on User @skip(if: true) {},";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FragmentSpreadNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.InlineFragment,
                    "User",
                    new SynNodeTestCase(
                        SynNodeType.Directive,
                        "skip"),
                    new SynNodeTestCase(
                        SynNodeType.FieldCollection)));
        }

        [Test]
        public void FragmentNode_ValidInlineFragment_WithFields_ParsesCorrectly()
        {
            var text = "...on User{field1, field2},";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FragmentSpreadNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.InlineFragment,
                    "User",
                    new SynNodeTestCase(
                        SynNodeType.FieldCollection,
                        new SynNodeTestCase(
                            SynNodeType.Field,
                            "field1",
                            "field1"),
                        new SynNodeTestCase(
                            SynNodeType.Field,
                            "field2",
                            "field2"))));
        }
    }
}