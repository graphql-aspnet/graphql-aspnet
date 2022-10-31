// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Parsing2.NodeBuilders.Inputs
{
    using System;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing2;
    using GraphQL.AspNet.Parsing2.NodeBuilders.Inputs;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class EnumValueNodeBuilderTests
    {
        [Test]
        public void EnumValueNodeMaker_PointingAtAName_ReturnsEnum()
        {
            var text = "JEDI, name = 343";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            EnumValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.EnumValue,
                    "JEDI"));

            // ensure stream is pointing at "name"
            Assert.AreEqual(TokenType.Name, stream.TokenType);
        }

        [Test]
        public void EnumValueNodeMaker_WillInterpreteABoolAsEnum_IfInvoked()
        {
            var text = "true, name = 343";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            EnumValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.EnumValue,
                    "true"));

            // ensure stream is pointing at "name"
            Assert.AreEqual(TokenType.Name, stream.TokenType);
        }

        [Test]
        public void EnumValueNodeMaker_NotPointingAtAName_ResultsInException()
        {
            var text = "123, arg2: \"stringValue\")";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;
            try
            {
                EnumValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expection syntax exception");
        }
    }
}