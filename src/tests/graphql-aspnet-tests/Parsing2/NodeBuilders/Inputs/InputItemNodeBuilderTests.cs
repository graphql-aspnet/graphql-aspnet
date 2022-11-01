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
    using GraphQL.AspNet.Parsing2;
    using GraphQL.AspNet.Parsing2.NodeBuilders.Inputs;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class InputItemNodeBuilderTests
    {
        [Test]
        public void InputItemNodeMaker_SimpleValue_ParsesCorrectly()
        {
            var text = "arg1: 123) { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            InputItemNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
              ref tree,
              docNode,
              new SynNodeTestCase(
                    SynNodeType.InputItem,
                    "arg1",
                    new SynNodeTestCase(
                        SynNodeType.ScalarValue,
                        "123",
                        ScalarValueType.Number)));
        }

        [Test]
        public void InputItemNodeMaker_WithDirective_ParsesCorrectly()
        {
            var text = "arg1: 5 @someDirective(if: true) ) { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            InputItemNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
              ref tree,
              docNode,
              new SynNodeTestCase(
                    SynNodeType.InputItem,
                    "arg1",
                    new SynNodeTestCase(
                        SynNodeType.ScalarValue,
                        "5",
                        ScalarValueType.Number),
                    new SynNodeTestCase(
                        SynNodeType.Directive,
                        "someDirective")));
        }

        [Test]
        public void InputItemNodeMaker_VariablePointer_ParsesCorrectly()
        {
            var text = "arg1: $variable1) { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            InputItemNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
              ref tree,
              docNode,
              new SynNodeTestCase(
                    SynNodeType.InputItem,
                    "arg1",
                    new SynNodeTestCase(
                        SynNodeType.VariableValue,
                        "variable1")));
        }

        [Test]
        public void InputItemNodeMaker_MissingColon_ThrowsException()
        {
            var text = "arg1 123) { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                InputItemNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void InputItemNodeMaker_NotPointingAtANamedToken_ThrowsException()
        {
            var text = "12Bob: 123) { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                InputItemNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void InputItemNodeMaker_NoValue_ThrowsException()
        {
            var text = "arg1:, ) { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                InputItemNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void InputItemNodeMaker_MissingEverything_ThrowsException()
        {
            var text = "arg1 , arg2: \"string\") { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                InputItemNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expection syntax exception");
        }
    }
}