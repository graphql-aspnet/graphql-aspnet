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
    using GraphQL.AspNet.Execution.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Execution.Parsing.NodeBuilders.Inputs;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Execution.Parsing.Helpers;
    using NUnit.Framework;

    [TestFixture]
    public class StringValueNodeBuilderTests
    {
        [Test]
        public void ValidString_ParsesNodeCorrectly()
        {
            var text = "\"TestValue\"";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.WithDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            StringValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.ScalarValue,
                    "\"TestValue\"",
                    ScalarValueType.String));
            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void TripleQuoteString_ParsesNodeCorrectly()
        {
            var text = "\"\"\"Tes\nt\"Va\r\nlue\"\"\", secondValue: 123";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.WithDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            StringValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
              stream.Source,
              tree,
              docNode,
              new SynNodeTestCase(
                SyntaxNodeType.ScalarValue,
                "\"\"\"Tes\nt\"Va\r\nlue\"\"\"",
                ScalarValueType.String));

            Assert.AreEqual(TokenType.Name, stream.TokenType);
            Assert.AreEqual("secondValue", stream.ActiveTokenText.ToString());
            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void NotPointingAtAString_ResultsInException()
        {
            var text = "nameToken(arg1: 123, arg2: \"stringValue\")";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.WithDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();

            try
            {
                StringValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
        public void PointingAtNull_ParsesToNullScalar()
        {
            var text = "null, arg2: \"stringValue\")";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.WithDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            StringValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.ScalarValue,
                    "null",
                    ScalarValueType.String));
            SyntaxTreeOperations.Release(ref tree);
        }
    }
}