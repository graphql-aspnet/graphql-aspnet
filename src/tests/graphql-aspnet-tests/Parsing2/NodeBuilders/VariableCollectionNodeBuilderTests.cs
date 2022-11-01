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

    [TestFixture]
    public class VariableCollectionNodeBuilderTests
    {
        [Test]
        public void VariableNode_WithMultipleVariables_SetsNameCorrectly()
        {
            var text = @"($episode: Episode = JEDI, $hero: Hero, $droid:Droid = ""R2-D2"")";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            VariableCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.VariableCollection,
                    new SynNodeTestCase(
                        SynNodeType.Variable,
                        "episode",
                        "Episode",
                        new SynNodeTestCase(
                            SynNodeType.EnumValue,
                            "JEDI")),
                    new SynNodeTestCase(
                        SynNodeType.Variable,
                        "hero",
                        "Hero"),
                    new SynNodeTestCase(
                        SynNodeType.Variable,
                        "droid",
                        "Droid",
                        new SynNodeTestCase(
                            SynNodeType.ScalarValue,
                            "\"R2-D2\"",
                            ScalarValueType.String))));
        }

        [Test]
        public void VariableNode_NoClosingParen_ThrowsExceptions()
        {
            var text = @"($episode: Episode = JEDI, $hero: Hero, $droid:Droid = ""R2-D2""";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                VariableCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void VariableNode_NotPointingAtACollection_ThrowsExceptions()
        {
            var text = @"someField($episode: Episode = JEDI, $hero: Hero, $droid:Droid = ""R2-D2"")";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                VariableCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expection syntax exception");
        }
    }
}