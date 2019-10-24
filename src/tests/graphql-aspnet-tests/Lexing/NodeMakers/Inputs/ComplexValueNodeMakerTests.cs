// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Lexing.NodeMakers.Inputs
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.NodeMakers.ValueMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using NUnit.Framework;

    /// <summary>
    /// Defines tests concerning only with <see cref="ComplexValueNodeMaker"/>
    /// ability to read from a token stream.
    /// </summary>
    [TestFixture]
    public class ComplexValueNodeMakerTests
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
            var complexMaker = ComplexValueNodeMaker.Instance;

            var node = complexMaker.MakeNode(stream) as ComplexValueNode;
            Assert.IsNotNull(node);
            Assert.AreEqual(1, node.Children.Count);

            var collection = node.Children.FirstOrDefault<InputItemCollectionNode>();
            Assert.IsNotNull(collection);
            Assert.AreEqual(1, collection.Children.Count);

            var childnode = collection.Children.FirstOrDefault<InputItemNode>();
            Assert.IsNotNull(childnode);
            Assert.AreEqual("test", childnode.InputName.ToString());

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
            var complexMaker = ComplexValueNodeMaker.Instance;

            var node = complexMaker.MakeNode(stream) as ComplexValueNode;
            Assert.IsNotNull(node);

            // three children childArg1, childArg2, childArg3
            var collection = node.Children.FirstOrDefault<InputItemCollectionNode>();
            Assert.IsNotNull(collection);
            Assert.AreEqual(3, collection.Children.Count);

            var childArg1 = collection.Children.OfType<InputItemNode>().SingleOrDefault(x => x.InputName.ToString() == "childArg1");
            var childArg2 = collection.Children.OfType<InputItemNode>().SingleOrDefault(x => x.InputName.ToString() == "childArg2");
            var childArg3 = collection.Children.OfType<InputItemNode>().SingleOrDefault(x => x.InputName.ToString() == "childArg3");

            Assert.IsNotNull(childArg1); // bob
            Assert.IsNotNull(childArg1.Children.FirstOrDefault<ScalarValueNode>());

            Assert.IsNotNull(childArg2); // {firstname: "jane", lastName: "doe" ...}
            Assert.IsNotNull(childArg2.Children.FirstOrDefault<ComplexValueNode>());

            Assert.IsNotNull(childArg3); // [123,321]
            Assert.IsNotNull(childArg3.Children.FirstOrDefault<EnumValueNode>());

            // ensure final stream position
            Assert.AreEqual(TokenType.Name, stream.TokenType);
            Assert.AreEqual("arg2", stream.ActiveToken.Text.ToString());
            Assert.IsFalse(stream.EndOfStream);
        }

        [Test]
        public void ComplexValueNodeMaker_InvalidClosure_YieldsException()
        {
            var text = @"{test: {test:""bob""},nextNode = 123";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            Assert.Throws<GraphQLSyntaxException>(() =>
            {
                var complexMaker = ComplexValueNodeMaker.Instance;
                var node = complexMaker.MakeNode(stream) as ComplexValueNode;
            });
        }

        [Test]
        public void ComplexValueNodeMaker_InvalidStreamStart_YieldsException()
        {
            // stream must point at left curly to start reading a complex value correctly
            var text = @"janeNode(arg1: {test:""bob""}, arg2 = 123){}";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            Assert.Throws<GraphQLSyntaxException>(() =>
            {
                var complexMaker = ComplexValueNodeMaker.Instance;
                var node = complexMaker.MakeNode(stream) as ComplexValueNode;
            });
        }
    }
}