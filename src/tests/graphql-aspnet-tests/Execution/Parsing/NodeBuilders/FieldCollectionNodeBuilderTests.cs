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
    public class FieldCollectionNodeBuilderTests
    {
        [Test]
        public void EmptyCollection_ParsesNoChildren()
        {
            var text = "{  }";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.FieldCollection,
                    SynNodeTestCase.NoChildren));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void NoPointingtoStartofCollection_ThrowsException()
        {
            var text = "query testQuery{  }";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                FieldCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
        public void NoEndingCurlyBrace_ThrowsException()
        {
            var text = "{field1, field2, field3,field4";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                FieldCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
        public void InvalidFieldName_ThrowsException()
        {
            var text = "{$field2}";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                FieldCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
        public void AssignedFieldAlias_IsSetCorrectly()
        {
            var text = "{field1, fieldA: field2}";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                SyntaxNodeType.FieldCollection,
                new SynNodeTestCase(
                    SyntaxNodeType.Field,
                    "field1",
                    "field1"),
                new SynNodeTestCase(
                    SyntaxNodeType.Field,
                    "field2",
                    "fieldA")));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void SimpleCollection_ParsesCorrectly()
        {
            var text = "{field1, field2, field3}";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                SyntaxNodeType.FieldCollection,
                new SynNodeTestCase(
                    SyntaxNodeType.Field,
                    "field1",
                    "field1"),
                new SynNodeTestCase(
                    SyntaxNodeType.Field,
                    "field2",
                    "field2"),
                new SynNodeTestCase(
                    SyntaxNodeType.Field,
                    "field3",
                    "field3")));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void WithFragments_ParsesCorrectly()
        {
            var text = "{field1, ...on User{fieldA, fieldQ}, ...someFragment }";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                SyntaxNodeType.FieldCollection,
                new SynNodeTestCase(
                    SyntaxNodeType.Field,
                    "field1",
                    "field1"),
                new SynNodeTestCase(
                    SyntaxNodeType.InlineFragment,
                    "User",
                    new SynNodeTestCase(
                        SyntaxNodeType.FieldCollection,
                        new SynNodeTestCase(
                            SyntaxNodeType.Field,
                            "fieldA",
                            "fieldA"),
                        new SynNodeTestCase(
                            SyntaxNodeType.Field,
                            "fieldQ",
                            "fieldQ"))),
                new SynNodeTestCase(
                    SyntaxNodeType.FragmentSpread,
                    "someFragment")));

            SyntaxTreeOperations.Release(ref tree);
        }
    }
}