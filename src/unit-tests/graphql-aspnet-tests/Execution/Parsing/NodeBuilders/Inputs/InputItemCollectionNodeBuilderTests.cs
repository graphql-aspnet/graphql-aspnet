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
    public class InputItemCollectionNodeBuilderTests
    {
        [Test]
        public void InputValueCollection_ValidCollection_ParsesCorrectly()
        {
            var text = "(arg1: 123, arg2: \"test\") { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            InputItemCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                  stream.Source,
                  tree,
                  docNode,
                  new SynNodeTestCase(
                      SyntaxNodeType.InputItemCollection,
                      new SynNodeTestCase(
                          SyntaxNodeType.InputItem,
                          "arg1",
                          new SynNodeTestCase(
                              SyntaxNodeType.ScalarValue,
                              "123",
                              ScalarValueType.Number)),
                      new SynNodeTestCase(
                          SyntaxNodeType.InputItem,
                          "arg2",
                          new SynNodeTestCase(
                              SyntaxNodeType.ScalarValue,
                              "\"test\"",
                              ScalarValueType.String))));

            // ensure stream focus
            Assert.IsTrue(stream.Match(TokenType.CurlyBraceLeft));
            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void InputValueCollection_MissingValue_ThrowsException()
        {
            var text = "(arg1: , arg2: \"test\") { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
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
            finally
            {
                SyntaxTreeOperations.Release(ref tree);
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void InputValueCollection_MissingCloseParen_ThrowsException()
        {
            var text = "(arg1: 123, arg2: \"test\" { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
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
            finally
            {
                SyntaxTreeOperations.Release(ref tree);
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void InputValueCollection_NotPointingAtCollection_ThrowsException()
        {
            var text = "someField(arg1: , arg2: \"test\") { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
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
            finally
            {
                SyntaxTreeOperations.Release(ref tree);
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void InputValueCollection_VariableReference_ParsesCorrectly()
        {
            var text = "(arg1: $variable1, arg2: \"test\") { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            InputItemCollectionNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                  stream.Source,
                  tree,
                  docNode,
                  new SynNodeTestCase(
                      SyntaxNodeType.InputItemCollection,
                      new SynNodeTestCase(
                          SyntaxNodeType.InputItem,
                          "arg1",
                          new SynNodeTestCase(
                              SyntaxNodeType.VariableValue,
                              "variable1")),
                      new SynNodeTestCase(
                          SyntaxNodeType.InputItem,
                          "arg2",
                          new SynNodeTestCase(
                              SyntaxNodeType.ScalarValue,
                              "\"test\"",
                              ScalarValueType.String))));

            // ensure stream focus
            Assert.IsTrue(stream.Match(TokenType.CurlyBraceLeft));
            SyntaxTreeOperations.Release(ref tree);
        }
    }
}