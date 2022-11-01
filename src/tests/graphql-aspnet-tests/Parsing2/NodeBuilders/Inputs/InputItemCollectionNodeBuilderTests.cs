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
    using GraphQL.AspNet.Parsing2.NodeBuilders;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using NUnit.Framework;
    using GraphQL.AspNet.Tests.Parsing2.Helpers;

    [TestFixture]
    public class InputItemCollectionNodeBuilderTests
    {
        [Test]
        public void InputValueCollection_ValidCollection_ParsesCorrectly()
        {
            var text = "(arg1: 123, arg2: \"test\") { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            InputItemCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                  docNode,
                  new SynNodeTestCase(
                      SynNodeType.InputItemCollection,
                      new SynNodeTestCase(
                          SynNodeType.InputItem,
                          "arg1",
                          new SynNodeTestCase(
                              SynNodeType.ScalarValue,
                              "123",
                              ScalarValueType.Number)),
                      new SynNodeTestCase(
                          SynNodeType.InputItem,
                          "arg2",
                          new SynNodeTestCase(
                              SynNodeType.ScalarValue,
                              "\"test\"",
                              ScalarValueType.String))));

            // ensure stream focus
            Assert.IsTrue(stream.Match(TokenType.CurlyBraceLeft));
            tree.Release();
        }

        [Test]
        public void InputValueCollection_MissingValue_ThrowsException()
        {
            var text = "(arg1: , arg2: \"test\") { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();

            try
            {
                InputItemCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }
            finally { tree.Release(); }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void InputValueCollection_MissingCloseParen_ThrowsException()
        {
            var text = "(arg1: 123, arg2: \"test\" { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();

            try
            {
                InputItemCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }
            finally { tree.Release(); }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void InputValueCollection_NotPointingAtCollection_ThrowsException()
        {
            var text = "someField(arg1: , arg2: \"test\") { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();

            try
            {
                InputItemCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }
            finally { tree.Release(); }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void InputValueCollection_VariableReference_ParsesCorrectly()
        {
            var text = "(arg1: $variable1, arg2: \"test\") { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            InputItemCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                  docNode,
                  new SynNodeTestCase(
                      SynNodeType.InputItemCollection,
                      new SynNodeTestCase(
                          SynNodeType.InputItem,
                          "arg1",
                          new SynNodeTestCase(
                              SynNodeType.VariableValue,
                              "variable1")),
                      new SynNodeTestCase(
                          SynNodeType.InputItem,
                          "arg2",
                          new SynNodeTestCase(
                              SynNodeType.ScalarValue,
                              "\"test\"",
                              ScalarValueType.String))));

            // ensure stream focus
            Assert.IsTrue(stream.Match(TokenType.CurlyBraceLeft));
            tree.Release();
        }
    }
}