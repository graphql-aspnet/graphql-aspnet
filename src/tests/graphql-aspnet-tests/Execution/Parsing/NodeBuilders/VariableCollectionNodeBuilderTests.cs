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
    public class VariableCollectionNodeBuilderTests
    {
        [Test]
        public void VariableNode_WithMultipleVariables_SetsNameCorrectly()
        {
            var text = @"($episode: Episode = JEDI, $hero: Hero, $droid:Droid = ""R2-D2"")";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            VariableCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.VariableCollection,
                    new SynNodeTestCase(
                        SyntaxNodeType.Variable,
                        "episode",
                        "Episode",
                        new SynNodeTestCase(
                            SyntaxNodeType.EnumValue,
                            "JEDI")),
                    new SynNodeTestCase(
                        SyntaxNodeType.Variable,
                        "hero",
                        "Hero"),
                    new SynNodeTestCase(
                        SyntaxNodeType.Variable,
                        "droid",
                        "Droid",
                        new SynNodeTestCase(
                            SyntaxNodeType.ScalarValue,
                            "\"R2-D2\"",
                            ScalarValueType.String))));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void VariableNode_NoClosingParen_ThrowsExceptions()
        {
            var text = @"($episode: Episode = JEDI, $hero: Hero, $droid:Droid = ""R2-D2""";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                VariableCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
        public void VariableNode_NotPointingAtACollection_ThrowsExceptions()
        {
            var text = @"someField($episode: Episode = JEDI, $hero: Hero, $droid:Droid = ""R2-D2"")";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                VariableCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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