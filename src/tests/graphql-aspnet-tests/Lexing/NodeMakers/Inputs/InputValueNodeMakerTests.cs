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
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using NUnit.Framework;

    /// <summary>
    /// Specialized tests for testing the processing of input values
    /// from a token stream. These tests process the meta-maker that is <see cref="InputValueNodeMaker"/>
    /// which farms off requests for input values to more specialized handlers. See those handler tests
    /// for specifics on various input value types
    /// maker.
    /// </summary>
    [TestFixture]
    public class InputValueNodeMakerTests
    {
        [Test]
        public void InputValueNodeMaker_KnownTokenValueType_ParsesCorrectly()
        {
            // assume the stream is pointing at a string value
            var text = "\"SomeValue\", arg2: 123)";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();

            var node = InputValueNodeMaker.Instance.MakeNode(stream) as ScalarValueNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("\"SomeValue\"", node.Value.ToString());
        }

        [Test]
        public void InputValueNodeMaker_UnknownTokenValueType_ThrowsException()
        {
            // assume the stream is pointing at an invalid close curly brace
            var text = "}, arg1: \"SomeValue\", arg2: 123)";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() =>
            {
                InputValueNodeMaker.Instance.MakeNode(stream);
            });
        }
    }
}