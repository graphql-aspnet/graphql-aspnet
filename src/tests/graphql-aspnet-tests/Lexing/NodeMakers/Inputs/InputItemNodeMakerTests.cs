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
    using GraphQL.AspNet.Parsing.NodeMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using NUnit.Framework;

    [TestFixture]
    public class InputItemNodeMakerTests
    {
        [Test]
        public void InputItemNodeMaker_SimpleValue_ParsesCorrectly()
        {
            var text = "arg1: 123) { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = InputItemNodeMaker.Instance.MakeNode(stream) as InputItemNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("arg1", node.InputName.ToString());
            Assert.AreEqual(1, node.Children.Count);

            var value = node.Children.FirstOrDefault<ScalarValueNode>();
            Assert.IsNotNull(value);
            Assert.AreEqual(ScalarValueType.Number, value.ValueType);
            Assert.AreEqual("123", value.Value.ToString());
        }

        [Test]
        public void InputItemNodeMaker_WithDirective_ParsesCorrectly()
        {
            var text = "arg1: 5 @someDirective(if: true) ) { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = InputItemNodeMaker.Instance.MakeNode(stream) as InputItemNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("arg1", node.InputName.ToString());
            Assert.AreEqual(2, node.Children.Count); // inputvalue + directive

            var directive = node.Children.FirstOrDefault<DirectiveNode>();
            Assert.IsNotNull(directive);
            Assert.AreEqual("someDirective", directive.DirectiveName.ToString());
        }

        [Test]
        public void InputItemNodeMaker_VariablePointer_ParsesCorrectly()
        {
            var text = "arg1: $variable1) { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = InputItemNodeMaker.Instance.MakeNode(stream) as InputItemNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("arg1", node.InputName.ToString());
            Assert.AreEqual(1, node.Children.Count);

            var value = node.Children.FirstOrDefault<VariableValueNode>();
            Assert.IsNotNull(value);
            Assert.AreEqual("variable1", value.Value.ToString());
        }

        [Test]
        public void InputItemNodeMaker_MissingColon_ThrowsException()
        {
            var text = "arg1 123) { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() =>
            {
                InputItemNodeMaker.Instance.MakeNode(stream);
            });
        }

        [Test]
        public void InputItemNodeMaker_NotPointingAtANamedToken_ThrowsException()
        {
            var text = "12Bob: 123) { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() =>
            {
                InputItemNodeMaker.Instance.MakeNode(stream);
            });
        }

        [Test]
        public void InputItemNodeMaker_NoValue_ThrowsException()
        {
            var text = "arg1:, ) { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() =>
            {
                InputItemNodeMaker.Instance.MakeNode(stream);
            });
        }

        [Test]
        public void InputItemNodeMaker_MissingEverything_ThrowsException()
        {
            var text = "arg1 , arg2: \"string\") { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() =>
            {
                InputItemNodeMaker.Instance.MakeNode(stream);
            });
        }
    }
}