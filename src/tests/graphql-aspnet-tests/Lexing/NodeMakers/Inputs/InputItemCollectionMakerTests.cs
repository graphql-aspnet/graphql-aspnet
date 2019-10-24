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
    using GraphQL.AspNet.Parsing.NodeMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using NUnit.Framework;

    /// <summary>
    /// Tests related to the assembly of the collection node for input values
    /// on a field.
    /// </summary>
    [TestFixture]
    public class InputItemCollectionMakerTests
    {
        [Test]
        public void InputValueCollection_ValidCollection_ParsesCorrectly()
        {
            var text = "(arg1: 123, arg2: \"test\") { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = InputItemCollectionNodeMaker.Instance.MakeNode(stream) as InputItemCollectionNode;
            Assert.IsNotNull(node);
            Assert.AreEqual(2, node.Children.Count);

            var firstItem = node.Children[0] as InputItemNode;
            Assert.IsNotNull(firstItem);
            Assert.AreEqual("arg1", firstItem.InputName.ToString());
            Assert.AreEqual(1, firstItem.Children.Count);

            var value = firstItem.Children.FirstOrDefault<ScalarValueNode>();
            Assert.IsNotNull(value);
            Assert.AreEqual(ScalarValueType.Number, value.ValueType);
            Assert.AreEqual("123", value.Value.ToString());

            var secondItem = node.Children[1] as InputItemNode;
            Assert.IsNotNull(secondItem);
            Assert.AreEqual("arg2", secondItem.InputName.ToString());
            Assert.AreEqual(1, secondItem.Children.Count);

            var value2 = secondItem.Children.FirstOrDefault<ScalarValueNode>();
            Assert.IsNotNull(value2);
            Assert.AreEqual(ScalarValueType.String, value2.ValueType);
            Assert.AreEqual("\"test\"", value2.Value.ToString());

            // ensure stream focus
            Assert.IsTrue(stream.Match(TokenType.CurlyBraceLeft));
        }

        [Test]
        public void InputValueCollection_MissingValue_ThrowsException()
        {
            var text = "(arg1: , arg2: \"test\") { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() =>
            {
                InputItemCollectionNodeMaker.Instance.MakeNode(stream);
            });
        }

        [Test]
        public void InputValueCollection_MissingCloseParen_ThrowsException()
        {
            var text = "(arg1: 123, arg2: \"test\" { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() =>
            {
                InputItemCollectionNodeMaker.Instance.MakeNode(stream);
            });
        }

        [Test]
        public void InputValueCollection_NotPointingAtCollection_ThrowsException()
        {
            var text = "someField(arg1: , arg2: \"test\") { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() =>
            {
                InputItemCollectionNodeMaker.Instance.MakeNode(stream);
            });
        }

        [Test]
        public void InputValueCollection_VariableReference_ParsesCorrectly()
        {
            var text = "(arg1: $variable1, arg2: \"test\") { ";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = InputItemCollectionNodeMaker.Instance.MakeNode(stream) as InputItemCollectionNode;
            Assert.IsNotNull(node);
            Assert.AreEqual(2, node.Children.Count);

            var firstItem = node.Children[0] as InputItemNode;
            Assert.IsNotNull(firstItem);
            Assert.AreEqual("arg1", firstItem.InputName.ToString());
            Assert.AreEqual(1, firstItem.Children.Count);

            var value = firstItem.Children.FirstOrDefault<VariableValueNode>();
            Assert.IsNotNull(value);
            Assert.AreEqual("variable1", value.Value.ToString());

            var secondItem = node.Children[1] as InputItemNode;
            Assert.IsNotNull(secondItem);
            Assert.AreEqual("arg2", secondItem.InputName.ToString());
            Assert.AreEqual(1, secondItem.Children.Count);

            var value2 = secondItem.Children.FirstOrDefault<ScalarValueNode>();
            Assert.IsNotNull(value2);
            Assert.AreEqual(ScalarValueType.String, value2.ValueType);
            Assert.AreEqual("\"test\"", value2.Value.ToString());

            // ensure stream focus
            Assert.IsTrue(stream.Match(TokenType.CurlyBraceLeft));
        }
    }
}