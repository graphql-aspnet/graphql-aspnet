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
    public class NumberValueNodeBuilderTests
    {
        [Test]
        public void NumberValueNodeMaker_Float_ParsesNodeCorrectly()
        {
            var text = "1234.567";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            NumberValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.ScalarValue,
                    "1234.567",
                    ScalarValueType.Number));

            Assert.IsTrue(stream.EndOfStream);
            tree.Release();
        }

        [Test]
        public void NumberValueNodeMaker_Int_ParsesNodeCorrectly()
        {
            var text = "1234, name = 343";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            NumberValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.ScalarValue,
                    "1234",
                    ScalarValueType.Number));

            Assert.AreEqual(TokenType.Name, stream.TokenType);
            tree.Release();
        }

        [Test]
        public void NumberValueNodeMaker_PointingAtNull_ParsesNodeCorrectly()
        {
            var text = "null, name = 343";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            NumberValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.ScalarValue,
                    "null",
                    ScalarValueType.Number));

            Assert.AreEqual(TokenType.Name, stream.TokenType);
            tree.Release();
        }

        [Test]
        public void NumberValueNodeMaker_NotPointingAtANumber_ResultsInException()
        {
            var text = "nameToken(arg1: 123, arg2: \"stringValue\")";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            try
            {
                NumberValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
    }
}