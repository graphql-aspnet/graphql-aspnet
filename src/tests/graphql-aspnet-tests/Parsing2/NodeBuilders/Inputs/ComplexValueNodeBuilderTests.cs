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
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing2;
    using GraphQL.AspNet.Parsing2.NodeBuilders.Inputs;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using NUnit.Framework;

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
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            ComplexValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.ComplexValue,
                    new SynNodeTestCase(
                        SynNodeType.InputItemCollection,
                        new SynNodeTestCase(
                            SynNodeType.InputItem,
                            "test",
                            new SynNodeTestCase(
                                SynNodeType.ScalarValue,
                                "\"bob\"",
                                ScalarValueType.String)))));

            // ensure the stream is pointing at the next item
            Assert.AreEqual(TokenType.Name, stream.TokenType);
            Assert.AreEqual("arg2", stream.ActiveToken.Text.ToString());
            Assert.IsFalse(stream.EndOfStream);
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
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            stream.Prime();
            ComplexValueNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
              ref tree,
              docNode,
              new SynNodeTestCase(
                  SynNodeType.ComplexValue,
                  new SynNodeTestCase(
                      SynNodeType.InputItemCollection,
                      new SynNodeTestCase(
                          SynNodeType.InputItem,
                          "childArg1",
                          new SynNodeTestCase(
                              SynNodeType.ScalarValue,
                              "\"bob\"",
                              ScalarValueType.String)),
                      new SynNodeTestCase(
                          SynNodeType.InputItem,
                          "childArg2",
                          new SynNodeTestCase(
                              SynNodeType.ComplexValue,
                              new SynNodeTestCase(
                                  SynNodeType.InputItemCollection,
                                  new SynNodeTestCase(
                                      SynNodeType.InputItem,
                                      "firstName",
                                      new SynNodeTestCase(
                                          SynNodeType.ScalarValue,
                                          "\"jane\"",
                                          ScalarValueType.String)),
                                  new SynNodeTestCase(
                                      SynNodeType.InputItem,
                                      "lastName",
                                      new SynNodeTestCase(
                                          SynNodeType.ScalarValue,
                                          "\"doe\"",
                                          ScalarValueType.String)),
                                  new SynNodeTestCase(
                                      SynNodeType.InputItem,
                                      "address",
                                      new SynNodeTestCase(
                                          SynNodeType.ComplexValue,
                                          new SynNodeTestCase(
                                              SynNodeType.InputItemCollection,
                                              new SynNodeTestCase(
                                                  SynNodeType.InputItem,
                                                  "city",
                                                  new SynNodeTestCase(
                                                      SynNodeType.ScalarValue,
                                                      "\"JanesVille\"",
                                                      ScalarValueType.String)),
                                              new SynNodeTestCase(
                                                  SynNodeType.InputItem,
                                                  "Zip",
                                                  new SynNodeTestCase(
                                                      SynNodeType.ScalarValue,
                                                      "12345",
                                                      ScalarValueType.Number)))))))),
                      new SynNodeTestCase(
                          SynNodeType.InputItem,
                          "childArg3",
                          new SynNodeTestCase(
                              SynNodeType.EnumValue,
                              "ENUMVALUE")))));

            Assert.IsFalse(stream.EndOfStream);
        }

        [Test]
        public void ComplexValueNodeMaker_InvalidClosure_YieldsException()
        {
            var text = @"{test: {test:""bob""},nextNode = 123";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
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

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void ComplexValueNodeMaker_InvalidStreamStart_YieldsException()
        {
            // stream must point at left curly to start reading a complex value correctly
            var text = @"janeNode(arg1: {test:""bob""}, arg2 = 123){}";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
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

            Assert.Fail("Expection syntax exception");
        }
    }
}