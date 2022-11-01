﻿// *************************************************************
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
    using GraphQL.AspNet.Parsing2.Lexing;
    using GraphQL.AspNet.Parsing2.Exceptions;
    using GraphQL.AspNet.Parsing2.Lexing.Source;
    using GraphQL.AspNet.Parsing2.Lexing.Tokens;
    using GraphQL.AspNet.Parsing2;
    using GraphQL.AspNet.Parsing2.NodeBuilders.Inputs;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using NUnit.Framework;
    using GraphQL.AspNet.Tests.Parsing2.Helpers;

    [TestFixture]
    public class StringValueNodeBuilderTests
    {
        [Test]
        public void ValidString_ParsesNodeCorrectly()
        {
            var text = "\"TestValue\"";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            StringValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.ScalarValue,
                    "\"TestValue\"",
                    ScalarValueType.String));
            tree.Release();
        }

        [Test]
        public void TripleQuoteString_ParsesNodeCorrectly()
        {
            var text = "\"\"\"Tes\nt\"Va\r\nlue\"\"\", secondValue: 123";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            StringValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
              stream.Source,
              tree,
              docNode,
              new SynNodeTestCase(
                SynNodeType.ScalarValue,
                "\"\"\"Tes\nt\"Va\r\nlue\"\"\"",
                ScalarValueType.String));

            Assert.AreEqual(TokenType.Name, stream.TokenType);
            Assert.AreEqual("secondValue", stream.ActiveTokenText.ToString());
            tree.Release();
        }

        [Test]
        public void NotPointingAtAString_ResultsInException()
        {
            var text = "nameToken(arg1: 123, arg2: \"stringValue\")";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
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
                tree.Release();
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void PointingAtNull_ParsesToNullScalar()
        {
            var text = "null, arg2: \"stringValue\")";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            StringValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.ScalarValue,
                    "null",
                    ScalarValueType.String));
            tree.Release();
        }
    }
}