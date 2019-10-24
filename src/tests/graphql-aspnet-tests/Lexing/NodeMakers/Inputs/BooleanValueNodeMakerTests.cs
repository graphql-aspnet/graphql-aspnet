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
    using GraphQL.AspNet.Parsing.NodeMakers.ValueMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using NUnit.Framework;

    /// <summary>
    /// Defines tests concerning only with <see cref="StringValueNodeMaker"/>
    /// ability to read from a token stream.
    /// </summary>
    [TestFixture]
    public class BooleanValueNodeMakerTests
    {
        [Test]
        public void BooleanValueNodeMakerTests_true_ParsesNodeCorrectly()
        {
            var text = "true";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            var result = BooleanValueNodeMaker.Instance.MakeNode(stream) as ScalarValueNode;
            Assert.IsNotNull(result);
            Assert.AreEqual(ScalarValueType.Boolean, result.ValueType);
            Assert.AreEqual("true", result.Value.ToString());
            Assert.IsTrue(stream.EndOfStream);
        }

        [Test]
        public void BooleanValueNodeMakerTests_false_ParsesNodeCorrectly()
        {
            var text = "false, name = 343";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            var result = BooleanValueNodeMaker.Instance.MakeNode(stream) as ScalarValueNode;
            Assert.IsNotNull(result);
            Assert.AreEqual(ScalarValueType.Boolean, result.ValueType);
            Assert.AreEqual("false", result.Value.ToString());
            Assert.IsFalse(stream.EndOfStream);
        }

        [Test]
        public void BooleanValueNodeMaker_PointingAtNull_ParsesNodeCorrectly()
        {
            var text = "null, name = 343";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            var result = BooleanValueNodeMaker.Instance.MakeNode(stream) as ScalarValueNode;
            Assert.IsNotNull(result);
            Assert.AreEqual(ScalarValueType.Boolean, result.ValueType);
            Assert.AreEqual("null", result.Value.ToString());
            Assert.IsFalse(stream.EndOfStream);
        }

        [Test]
        public void BooleanValueNodeMaker_NotPointingAtBoolean_ResultsInException()
        {
            var text = "nameToken(arg1: 123, arg2: \"stringValue\")";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            Assert.Throws<GraphQLSyntaxException>(() => { BooleanValueNodeMaker.Instance.MakeNode(stream); });
        }
    }
}