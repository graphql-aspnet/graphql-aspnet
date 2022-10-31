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
    using GraphQL.AspNet.Tests.CommonHelpers;
    using System;
    using NUnit.Framework;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing2;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing2.NodeBuilders.Inputs;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;

    [TestFixture]
    public class NullValueNodeBuilderTests
    {
        [Test]
        public void Null_ParsesNodeCorrectly()
        {
            var text = "null";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            NullValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.NullValue));

            Assert.IsTrue(stream.EndOfStream);
        }

        [Test]
        public void NotPointingAtNull_ResultsInException()
        {
            var text = "nameToken(arg1: 123, arg2: \"stringValue\")";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            try
            {
                NullValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expected syntax exception");
        }
    }
}