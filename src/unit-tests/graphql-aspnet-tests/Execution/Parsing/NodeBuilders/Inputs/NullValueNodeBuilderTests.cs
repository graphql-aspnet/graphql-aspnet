// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.Parsing.NodeBuilders.Inputs
{
    using System;
    using GraphQL.AspNet.Execution.Parsing;
    using GraphQL.AspNet.Execution.Parsing.Exceptions;
    using GraphQL.AspNet.Execution.Parsing.Lexing;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;
    using GraphQL.AspNet.Execution.Parsing.NodeBuilders.Inputs;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Execution.Parsing.Helpers;
    using NUnit.Framework;

    [TestFixture]
    public class NullValueNodeBuilderTests
    {
        [Test]
        public void Null_ParsesNodeCorrectly()
        {
            var text = "null";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            NullValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.NullValue));

            Assert.IsTrue((bool)stream.EndOfStream);
            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void NotPointingAtNull_ResultsInException()
        {
            var text = "nameToken(arg1: 123, arg2: \"stringValue\")";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
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
            finally
            {
                SyntaxTreeOperations.Release(ref tree);
            }

            Assert.Fail("Expected syntax exception");
        }
    }
}