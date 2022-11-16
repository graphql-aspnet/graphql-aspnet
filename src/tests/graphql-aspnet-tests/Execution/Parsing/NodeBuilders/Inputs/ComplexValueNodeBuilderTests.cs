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
    public class ComplexValueNodeBuilderTests
    {
        [Test]
        public void ComplexValueNodeMaker_SimpleComplexValue_YieldsComplexNode()
        {
            // simulate pointing at the value of a complex node, the input argument name
            // has already been read
            // e.g.   someField(arg1: {test: "bob"} arg2: 123){ fieldListHere }
            var text = @"{test: ""bob""} arg2: 123";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            ComplexValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.ComplexValue,
                    new SynNodeTestCase(
                        SyntaxNodeType.InputItemCollection,
                        new SynNodeTestCase(
                            SyntaxNodeType.InputItem,
                            "test",
                            new SynNodeTestCase(
                                SyntaxNodeType.ScalarValue,
                                "\"bob\"",
                                ScalarValueType.String)))));

            // ensure the stream is pointing at the next item
            Assert.AreEqual(TokenType.Name, stream.TokenType);
            Assert.AreEqual("arg2", stream.ActiveTokenText.ToString());
            Assert.IsFalse(stream.EndOfStream);

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void ComplexValueNodeMaker_NestedComplexValue_YieldsComplexNodeChain()
        {
            var text = @"{
                            childArg1: ""bob"",
                            childArg2: {
                                    firstName: ""jane"",
                                    lastName: ""doe"",
                                    address: {city: ""JanesVille"", Zip: 12345}
                            },
                            childArg3: ENUMVALUE
                         }, arg2 = 123";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            ComplexValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                  SyntaxNodeType.ComplexValue,
                  new SynNodeTestCase(
                      SyntaxNodeType.InputItemCollection,
                      new SynNodeTestCase(
                          SyntaxNodeType.InputItem,
                          "childArg1",
                          new SynNodeTestCase(
                              SyntaxNodeType.ScalarValue,
                              "\"bob\"",
                              ScalarValueType.String)),
                      new SynNodeTestCase(
                          SyntaxNodeType.InputItem,
                          "childArg2",
                          new SynNodeTestCase(
                              SyntaxNodeType.ComplexValue,
                              new SynNodeTestCase(
                                  SyntaxNodeType.InputItemCollection,
                                  new SynNodeTestCase(
                                      SyntaxNodeType.InputItem,
                                      "firstName",
                                      new SynNodeTestCase(
                                          SyntaxNodeType.ScalarValue,
                                          "\"jane\"",
                                          ScalarValueType.String)),
                                  new SynNodeTestCase(
                                      SyntaxNodeType.InputItem,
                                      "lastName",
                                      new SynNodeTestCase(
                                          SyntaxNodeType.ScalarValue,
                                          "\"doe\"",
                                          ScalarValueType.String)),
                                  new SynNodeTestCase(
                                      SyntaxNodeType.InputItem,
                                      "address",
                                      new SynNodeTestCase(
                                          SyntaxNodeType.ComplexValue,
                                          new SynNodeTestCase(
                                              SyntaxNodeType.InputItemCollection,
                                              new SynNodeTestCase(
                                                  SyntaxNodeType.InputItem,
                                                  "city",
                                                  new SynNodeTestCase(
                                                      SyntaxNodeType.ScalarValue,
                                                      "\"JanesVille\"",
                                                      ScalarValueType.String)),
                                              new SynNodeTestCase(
                                                  SyntaxNodeType.InputItem,
                                                  "Zip",
                                                  new SynNodeTestCase(
                                                      SyntaxNodeType.ScalarValue,
                                                      "12345",
                                                      ScalarValueType.Number)))))))),
                      new SynNodeTestCase(
                          SyntaxNodeType.InputItem,
                          "childArg3",
                          new SynNodeTestCase(
                              SyntaxNodeType.EnumValue,
                              "ENUMVALUE")))));

            Assert.IsFalse(stream.EndOfStream);
            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void ComplexValueNodeMaker_InvalidClosure_YieldsException()
        {
            var text = @"{test: {test:""bob""},nextNode = 123";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            try
            {
                ComplexValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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
        public void ComplexValueNodeMaker_InvalidStreamStart_YieldsException()
        {
            // stream must point at left curly to start reading a complex value correctly
            var text = @"janeNode(arg1: {test:""bob""}, arg2 = 123){}";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            try
            {
                ComplexValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
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