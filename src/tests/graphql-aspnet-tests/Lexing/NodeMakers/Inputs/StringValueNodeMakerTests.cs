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
    public class StringValueNodeMakerTests
    {
        [Test]
        public void StringValueNodeMaker_ValidString_ParsesNodeCorrectly()
        {
            var text = "\"TestValue\"";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            var result = StringValueNodeMaker.Instance.MakeNode(stream) as ScalarValueNode;
            Assert.IsNotNull(result);
            Assert.AreEqual(ScalarValueType.String, result.ValueType);
            Assert.AreEqual("\"TestValue\"", result.Value.ToString());
        }

        [Test]
        public void StringValueNodeMaker_TripleQuoteString_ParsesNodeCorrectly()
        {
            var text = "\"\"\"Tes\nt\"Va\r\nlue\"\"\", secondValue: 123";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            var result = StringValueNodeMaker.Instance.MakeNode(stream) as ScalarValueNode;
            Assert.IsNotNull(result);
            Assert.AreEqual(ScalarValueType.String, result.ValueType);
            Assert.AreEqual("\"\"\"Tes\nt\"Va\r\nlue\"\"\"", result.Value.ToString());
            Assert.AreEqual(TokenType.Name, stream.TokenType);
            Assert.AreEqual("secondValue", stream.ActiveToken.Text.ToString());
        }

        [Test]
        public void StringValueNodeMaker_NotPointingAtAString_ResultsInException()
        {
            var text = "nameToken(arg1: 123, arg2: \"stringValue\")";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            Assert.Throws<GraphQLSyntaxException>(() =>
            {
                StringValueNodeMaker.Instance.MakeNode(stream);
            });
        }

        [Test]
        public void StringValueNodeMaker_PointingAtNull_ParsesToNullScalar()
        {
            var text = "null, arg2: \"stringValue\")";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            var node = StringValueNodeMaker.Instance.MakeNode(stream) as ScalarValueNode;
            Assert.IsNotNull(node);
            Assert.AreEqual(ScalarValueType.String, node.ValueType);
            Assert.AreEqual("null", node.Value.ToString());
        }
    }
}