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
    public class ListValueNodeBuilderTests
    {
        [Test]
        public void ListValueNodeMaker_SimpleNumberList_ParsesCorrectly()
        {
            var text = "[1234, 5678, 91011], arg2: VALUE)";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.WithDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            ListValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                   SyntaxNodeType.ListValue,
                   new SynNodeTestCase(
                       SyntaxNodeType.ScalarValue,
                       "1234",
                       ScalarValueType.Number),
                   new SynNodeTestCase(
                       SyntaxNodeType.ScalarValue,
                       "5678",
                       ScalarValueType.Number),
                   new SynNodeTestCase(
                       SyntaxNodeType.ScalarValue,
                       "91011",
                       ScalarValueType.Number)));

            // ensure stream is pointing beyond the end of the list
            Assert.AreEqual(TokenType.Name, stream.TokenType);
            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void ListValueNodeMaker_SimpleStringList_ParsesCorrectly()
        {
            // use both types of string delimiters
            var text = "[\"bob\", \"\"\"Robert\"\"\"], arg2: VALUE)";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.WithDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            ListValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                  SyntaxNodeType.ListValue,
                  new SynNodeTestCase(
                      SyntaxNodeType.ScalarValue,
                      "\"bob\"",
                      ScalarValueType.String),
                  new SynNodeTestCase(
                      SyntaxNodeType.ScalarValue,
                      "\"\"\"Robert\"\"\"",
                      ScalarValueType.String)));

            // ensure stream is pointing beyond the end of the list
            Assert.AreEqual(TokenType.Name, stream.TokenType);
            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void ListValueNodeMaker_ComplexValueList_ParsesCorrectly()
        {
            var text = "[{arg1: 123, arg2: [456, 1234]}," +
                       "{arg1: 345, arg2: [982, 1231]}," +
                       "{arg3: 812, arg2: [13,31]}], arg2: VALUE)";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.WithDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            ListValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                  SyntaxNodeType.ListValue,
                  new SynNodeTestCase(
                      SyntaxNodeType.ComplexValue,
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
                                  SyntaxNodeType.ListValue,
                                  new SynNodeTestCase(
                                      SyntaxNodeType.ScalarValue,
                                      "456",
                                      ScalarValueType.Number),
                                  new SynNodeTestCase(
                                      SyntaxNodeType.ScalarValue,
                                      "1234",
                                      ScalarValueType.Number))))),
                  new SynNodeTestCase(
                      SyntaxNodeType.ComplexValue,
                      new SynNodeTestCase(
                          SyntaxNodeType.InputItemCollection,
                          new SynNodeTestCase(
                              SyntaxNodeType.InputItem,
                              "arg1",
                              new SynNodeTestCase(
                                  SyntaxNodeType.ScalarValue,
                                  "345",
                                  ScalarValueType.Number)),
                          new SynNodeTestCase(
                              SyntaxNodeType.InputItem,
                              "arg2",
                              new SynNodeTestCase(
                                  SyntaxNodeType.ListValue,
                                  new SynNodeTestCase(
                                      SyntaxNodeType.ScalarValue,
                                      "982",
                                      ScalarValueType.Number),
                                  new SynNodeTestCase(
                                      SyntaxNodeType.ScalarValue,
                                      "1231",
                                      ScalarValueType.Number))))),
                  new SynNodeTestCase(
                      SyntaxNodeType.ComplexValue,
                      new SynNodeTestCase(
                          SyntaxNodeType.InputItemCollection,
                          new SynNodeTestCase(
                              SyntaxNodeType.InputItem,
                              "arg3",
                              new SynNodeTestCase(
                                  SyntaxNodeType.ScalarValue,
                                  "812",
                                  ScalarValueType.Number)),
                          new SynNodeTestCase(
                              SyntaxNodeType.InputItem,
                              "arg2",
                              new SynNodeTestCase(
                                  SyntaxNodeType.ListValue,
                                  new SynNodeTestCase(
                                      SyntaxNodeType.ScalarValue,
                                      "13",
                                      ScalarValueType.Number),
                                  new SynNodeTestCase(
                                      SyntaxNodeType.ScalarValue,
                                      "31",
                                      ScalarValueType.Number)))))));

            // ensure stream is pointing beyond the end of the list
            Assert.AreEqual(TokenType.Name, stream.TokenType);
            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void ListValueNodeMaker_SmallComplexValueList_ParsesCorrectly()
        {
            var text = "[{arg1: 123, arg2: [456, 1234]}])";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.WithDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            ListValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                  SyntaxNodeType.ListValue,
                  new SynNodeTestCase(
                      SyntaxNodeType.ComplexValue,
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
                                  SyntaxNodeType.ListValue,
                                  new SynNodeTestCase(
                                      SyntaxNodeType.ScalarValue,
                                      "456",
                                      ScalarValueType.Number),
                                  new SynNodeTestCase(
                                      SyntaxNodeType.ScalarValue,
                                      "1234",
                                      ScalarValueType.Number)))))));

            // ensure stream is pointing beyond the end of the list
            Assert.AreEqual(TokenType.ParenRight, stream.TokenType);
            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void ListValueNodeMaker_ListOfLists_ParsesCorrectly()
        {
            var text = "[[456, 1234],[982, 1231],[13,99]], arg2: VALUE)";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.WithDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            ListValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                  SyntaxNodeType.ListValue,
                  new SynNodeTestCase(
                      SyntaxNodeType.ListValue,
                      new SynNodeTestCase(
                          SyntaxNodeType.ScalarValue,
                          "456",
                          ScalarValueType.Number),
                      new SynNodeTestCase(
                          SyntaxNodeType.ScalarValue,
                          "1234",
                          ScalarValueType.Number)),
                  new SynNodeTestCase(
                      SyntaxNodeType.ListValue,
                      new SynNodeTestCase(
                          SyntaxNodeType.ScalarValue,
                          "982",
                          ScalarValueType.Number),
                      new SynNodeTestCase(
                          SyntaxNodeType.ScalarValue,
                          "1231",
                          ScalarValueType.Number)),
                  new SynNodeTestCase(
                      SyntaxNodeType.ListValue,
                      new SynNodeTestCase(
                          SyntaxNodeType.ScalarValue,
                          "13",
                          ScalarValueType.Number),
                      new SynNodeTestCase(
                          SyntaxNodeType.ScalarValue,
                          "99",
                          ScalarValueType.Number))));
            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void ListValueNodeMaker_MixedValueList_AssignsCorrectScalarTypes()
        {
            var text = "[123, \"\"\"Robert\"\"\"], arg2: VALUE)";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.WithDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            ListValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                  SyntaxNodeType.ListValue,
                  new SynNodeTestCase(
                      SyntaxNodeType.ScalarValue,
                      "123",
                      ScalarValueType.Number),
                  new SynNodeTestCase(
                      SyntaxNodeType.ScalarValue,
                      "\"\"\"Robert\"\"\"",
                      ScalarValueType.String)));
            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void ListValueNodeMaker_SameValueListWithNullInList_ParasesFine()
        {
            var text = "[123, null, 456], arg2: VALUE)";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.WithDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            ListValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                  SyntaxNodeType.ListValue,
                  new SynNodeTestCase(
                      SyntaxNodeType.ScalarValue,
                      "123",
                      ScalarValueType.Number),
                  new SynNodeTestCase(
                      SyntaxNodeType.NullValue),
                  new SynNodeTestCase(
                      SyntaxNodeType.ScalarValue,
                      "456",
                      ScalarValueType.Number)));
            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void ListValueNodeMaker_NotPointingAtAList_ThrowsException()
        {
            var text = "someName(arg1: [123, 456], arg2: \"jane\")";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.WithDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            try
            {
                ListValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
        public void ListValueNodeMaker_UnclosedNestedListOfLists_ThrowsException()
        {
            var text = "[[456, 1234],[982, 1231],[13,99], arg2: VALUE)";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.WithDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            try
            {
                ListValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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