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
    using GraphQL.AspNet.Parsing2.Lexing;
    using GraphQL.AspNet.Parsing2.Exceptions;
    using GraphQL.AspNet.Parsing2.Lexing.Source;
    using GraphQL.AspNet.Parsing2;
    using GraphQL.AspNet.Parsing2.NodeBuilders;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using NUnit.Framework;
    using GraphQL.AspNet.Tests.Parsing2.Helpers;

    [TestFixture]
    public class NamedFragmentNodeBuilderTests
    {
        [Test]
        public void FragmentRootNodeMaker_FragmentKeyWord_ParsesCorrectly()
        {
            var text = "fragment someFragment on User{}";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            NamedFragmentNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.NamedFragment,
                    "someFragment",
                    "User"));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void FragmentRootNodeMaker_WithDirective_ParsesCorrectly()
        {
            var text = "fragment someFragment on User @skip(if: true){}";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            NamedFragmentNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.NamedFragment,
                    "someFragment",
                    "User",
                    new SynNodeTestCase(
                        SyntaxNodeType.Directive,
                        "skip")));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void FragmentRootNodeMaker_NotAtFragmentKeyword_ThrowsException()
        {
            var text = "query someFragment on User{}";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                NamedFragmentNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
        public void FragmentRootNodeMaker_NoOnKeyword_ThrowsException()
        {
            var text = "fragment someFragment in User{}";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                NamedFragmentNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
        public void FragmentRootNodeMaker_NoTargetType_ThrowsException()
        {
            var text = "fragment someFragment on{}";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                NamedFragmentNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
        public void FragmentRootNodeMaker_NoFragmentName_ThrowsException()
        {
            var text = "fragment on User{}";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                NamedFragmentNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
        public void FragmentRootNodeMaker_NameIsANotAName_ThrowsException()
        {
            var text = "fragment 123 on User{}";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                NamedFragmentNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
        public void FragmentRootNodeMaker_TargetTypeNotAName_ThrowsException()
        {
            var text = "fragment someFragment on \"User\"{}";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                NamedFragmentNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
        public void FragmentRootNodeMaker_WithAFieldSet_ParsesCorrectly()
        {
            var text = "fragment someFragment on User{field1, field2}";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            NamedFragmentNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.NamedFragment,
                    "someFragment",
                    "User",
                    new SynNodeTestCase(
                        SyntaxNodeType.FieldCollection)));

            SyntaxTreeOperations.Release(ref tree);
        }
    }
}