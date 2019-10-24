// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Lexing.NodeMakers
{
    using System;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.NodeMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using NUnit.Framework;

    /// <summary>
    /// Tests related to the construction of <see cref="DirectiveNode"/>.
    /// </summary>
    [TestFixture]
    public class DirectiveNodeMakerTests
    {
        [Test]
        public void DirectiveNodeMaker_SimpleDirective_ParsesCorrectly()
        {
            var text = "@skip(if: true){}";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = DirectiveNodeMaker.Instance.MakeNode(stream) as DirectiveNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("skip", node.DirectiveName.ToString());
            Assert.AreEqual(1, node.Children.Count);

            var inputCollection = node.Children[0] as InputItemCollectionNode;
            Assert.IsNotNull(inputCollection);
            Assert.AreEqual(1, inputCollection.Children.Count);

            var child = inputCollection.Children[0] as InputItemNode;
            Assert.IsNotNull(child);
            Assert.AreEqual(1, child.Children.Count);

            var value = child.Children[0] as ScalarValueNode;
            Assert.IsNotNull(value);
            Assert.AreEqual(ScalarValueType.Boolean, value.ValueType);
            Assert.AreEqual("true", value.Value.ToString());
        }

        [Test]
        public void DirectiveNodeMaker_WithFieldsAfter_ParsesCorrectly()
        {
            var text = "@skip(if: true){field1, field2}";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = DirectiveNodeMaker.Instance.MakeNode(stream) as DirectiveNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("skip", node.DirectiveName.ToString());
            Assert.AreEqual(1, node.Children.Count);

            Assert.IsTrue(stream.Match(TokenType.CurlyBraceLeft));
        }

        [Test]
        public void DirectiveNodeMaker_NoAtSign_ThrowsException()
        {
            var text = "skip(if: true)";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() => { DirectiveNodeMaker.Instance.MakeNode(stream); });
        }

        [Test]
        public void DirectiveNodeMaker_NoDirectiveName_ThrowsException()
        {
            var text = "@(if: true)";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() => { DirectiveNodeMaker.Instance.MakeNode(stream); });
        }

        [Test]
        public void DirectiveNodeMaker_NoAtSignAndNoName_ThrowsException()
        {
            var text = "(if: true)";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() => { DirectiveNodeMaker.Instance.MakeNode(stream); });
        }

        [Test]
        public void DirectiveNodeMaker_NotAtADirective_ThrowsException()
        {
            var text = "query someQuery{field1, field2}";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() => { DirectiveNodeMaker.Instance.MakeNode(stream); });
        }
    }
}