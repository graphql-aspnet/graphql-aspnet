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
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.NodeMakers.ValueMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using NUnit.Framework;

    /// <summary>
    /// Defines tests concerning only with <see cref="StringValueNodeMaker"/>
    /// ability to read from a token stream.
    /// </summary>
    [TestFixture]
    public class NumberValueNodeMakerTests
    {
        [Test]
        public void NumberValueNodeMaker_Float_ParsesNodeCorrectly()
        {
            var text = "1234.567";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            var result = NumberValueNodeMaker.Instance.MakeNode(stream) as ScalarValueNode;
            Assert.IsNotNull(result);
            Assert.AreEqual(result.ValueType, ScalarValueType.Number);
            Assert.AreEqual("1234.567", result.Value.ToString());
            Assert.IsTrue(stream.EndOfStream);
        }

        [Test]
        public void NumberValueNodeMaker_Int_ParsesNodeCorrectly()
        {
            var text = "1234, name = 343";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            var result = NumberValueNodeMaker.Instance.MakeNode(stream) as ScalarValueNode;
            Assert.IsNotNull(result);
            Assert.AreEqual(result.ValueType, ScalarValueType.Number);
            Assert.AreEqual("1234", result.Value.ToString());
            Assert.AreEqual(TokenType.Name, stream.TokenType);
        }

        [Test]
        public void NumberValueNodeMaker_PointingAtNull_ParsesNodeCorrectly()
        {
            var text = "null, name = 343";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            var result = NumberValueNodeMaker.Instance.MakeNode(stream) as ScalarValueNode;
            Assert.IsNotNull(result);
            Assert.AreEqual(result.ValueType, ScalarValueType.Number);
            Assert.AreEqual("null", result.Value.ToString());
            Assert.AreEqual(TokenType.Name, stream.TokenType);
        }

        [Test]
        public void NumberValueNodeMaker_NotPointingAtANumber_ResultsInException()
        {
            var text = "nameToken(arg1: 123, arg2: \"stringValue\")";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            Assert.Throws<GraphQLSyntaxException>(() => { NumberValueNodeMaker.Instance.MakeNode(stream); });
        }
    }
}