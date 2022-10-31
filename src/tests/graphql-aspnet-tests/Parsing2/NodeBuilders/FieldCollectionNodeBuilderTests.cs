namespace GraphQL.AspNet.Tests.Parsing2.NodeBuilders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing2;
    using GraphQL.AspNet.Parsing2.NodeBuilders;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class FieldCollectionNodeBuilderTests
    {
        [Test]
        public void EmptyCollection_ParsesNoChildren()
        {
            var text = "{  }";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.FieldCollection,
                    SynNodeTestCase.NoChildren));
        }

        [Test]
        public void NoPointingtoStartofCollection_ThrowsException()
        {
            var text = "query testQuery{  }";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                FieldCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void NoEndingCurlyBrace_ThrowsException()
        {
            var text = "{field1, field2, field3,field4";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                FieldCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void InvalidFieldName_ThrowsException()
        {
            var text = "{$field2}";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                FieldCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void AssignedFieldAlias_IsSetCorrectly()
        {
            var text = "{field1, fieldA: field2}";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                SynNodeType.FieldCollection,
                new SynNodeTestCase(
                    SynNodeType.Field,
                    "field1",
                    "field1"),
                new SynNodeTestCase(
                    SynNodeType.Field,
                    "field2",
                    "fieldA")));
        }

        [Test]
        public void SimpleCollection_ParsesCorrectly()
        {
            var text = "{field1, field2, field3}";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                SynNodeType.FieldCollection,
                new SynNodeTestCase(
                    SynNodeType.Field,
                    "field1",
                    "field1"),
                new SynNodeTestCase(
                    SynNodeType.Field,
                    "field2",
                    "field2"),
                new SynNodeTestCase(
                    SynNodeType.Field,
                    "field3",
                    "field3")));
        }

        [Test]
        public void WithFragments_ParsesCorrectly()
        {
            var text = "{field1, ...on User{fieldA, fieldQ}, ...someFragment }";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            FieldCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                SynNodeType.FieldCollection,
                new SynNodeTestCase(
                    SynNodeType.Field,
                    "field1",
                    "field1"),
                new SynNodeTestCase(
                    SynNodeType.InlineFragment,
                    "User",
                    new SynNodeTestCase(
                        SynNodeType.FieldCollection,
                        new SynNodeTestCase(
                            SynNodeType.Field,
                            "fieldA",
                            "fieldA"),
                        new SynNodeTestCase(
                            SynNodeType.Field,
                            "fieldQ",
                            "fieldQ"))),
                new SynNodeTestCase(
                    SynNodeType.FragmentSpread,
                    "someFragment")));
        }
    }
}
