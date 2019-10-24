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
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using NUnit.Framework;

    /// <summary>
    /// Defines tests concerning only with <see cref="EnumValueNodeMaker"/>
    /// ability to read from a token stream.
    /// </summary>
    [TestFixture]
    public class EnumValueNodeMakerTests
    {
        [Test]
        public void EnumValueNodeMaker_PointingAtAName_ReturnsEnum()
        {
            var text = "JEDI, name = 343";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            var result = EnumValueNodeMaker.Instance.MakeNode(stream) as EnumValueNode;
            Assert.IsNotNull(result);
            Assert.AreEqual("JEDI", result.Value.ToString());

            // ensure stream is pointing at "name"
            Assert.AreEqual(TokenType.Name, stream.TokenType);
        }

        [Test]
        public void EnumValueNodeMaker_WillInterpreteABoolAsEnum_IfInvoked()
        {
            var text = "true, name = 343";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            var result = EnumValueNodeMaker.Instance.MakeNode(stream) as EnumValueNode;
            Assert.IsNotNull(result);
            Assert.AreEqual("true", result.Value.ToString());

            // ensure stream is pointing at "name"
            Assert.AreEqual(TokenType.Name, stream.TokenType);
        }

        [Test]
        public void EnumValueNodeMaker_NotPointingAtAName_ResultsInException()
        {
            var text = "123, arg2: \"stringValue\")";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            Assert.Throws<GraphQLSyntaxException>(() =>
            {
                EnumValueNodeMaker.Instance.MakeNode(stream);
            });
        }
    }
}