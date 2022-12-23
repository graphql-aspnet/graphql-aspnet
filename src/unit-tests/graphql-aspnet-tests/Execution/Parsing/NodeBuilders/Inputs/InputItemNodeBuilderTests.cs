﻿// *************************************************************
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
    using GraphQL.AspNet.Tests.CommonHelpers;
    using NUnit.Framework;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Execution.Parsing.NodeBuilders.Inputs;
    using GraphQL.AspNet.Execution.Parsing;
    using GraphQL.AspNet.Execution.Parsing.Lexing;
    using GraphQL.AspNet.Execution.Parsing.Exceptions;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;
    using GraphQL.AspNet.Tests.Execution.Parsing.Helpers;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;

    [TestFixture]
    public class InputItemNodeBuilderTests
    {
        [Test]
        public void InputItemNodeMaker_SimpleValue_ParsesCorrectly()
        {
            var text = "arg1: 123) { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.WithDocumentRoot();
            var docNode = tree.RootNode;

            InputItemNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
              stream.Source,
              tree,
              docNode,
              new SynNodeTestCase(
                    SyntaxNodeType.InputItem,
                    "arg1",
                    new SynNodeTestCase(
                        SyntaxNodeType.ScalarValue,
                        "123",
                        ScalarValueType.Number)));
            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void InputItemNodeMaker_WithDirective_ParsesCorrectly()
        {
            var text = "arg1: 5 @someDirective(if: true) ) { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.WithDocumentRoot();
            var docNode = tree.RootNode;

            InputItemNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
              stream.Source,
              tree,
              docNode,
              new SynNodeTestCase(
                    SyntaxNodeType.InputItem,
                    "arg1",
                    new SynNodeTestCase(
                        SyntaxNodeType.ScalarValue,
                        "5",
                        ScalarValueType.Number),
                    new SynNodeTestCase(
                        SyntaxNodeType.Directive,
                        "someDirective")));
            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void InputItemNodeMaker_VariablePointer_ParsesCorrectly()
        {
            var text = "arg1: $variable1) { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.WithDocumentRoot();
            var docNode = tree.RootNode;

            InputItemNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
              stream.Source,
              tree,
              docNode,
              new SynNodeTestCase(
                    SyntaxNodeType.InputItem,
                    "arg1",
                    new SynNodeTestCase(
                        SyntaxNodeType.VariableValue,
                        "variable1")));
            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void InputItemNodeMaker_MissingColon_ThrowsException()
        {
            var text = "arg1 123) { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.WithDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                InputItemNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
        public void InputItemNodeMaker_NotPointingAtANamedToken_ThrowsException()
        {
            var text = "12Bob: 123) { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.WithDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                InputItemNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
        public void InputItemNodeMaker_NoValue_ThrowsException()
        {
            var text = "arg1:, ) { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.WithDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                InputItemNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
        public void InputItemNodeMaker_MissingEverything_ThrowsException()
        {
            var text = "arg1 , arg2: \"string\") { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.WithDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                InputItemNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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