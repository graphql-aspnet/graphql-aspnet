// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

// ReSharper disable All
namespace GraphQL.AspNet.Tests.Lexing.NodeMakers.Inputs
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.NodeMakers.ValueMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using NUnit.Framework;

    /// <summary>
    /// Defines tests concerning only with <see cref="ListValueNodeMaker"/>
    /// ability to read from a token stream.
    /// </summary>
    [TestFixture]
    public class ListValueNodeMakerTests
    {
        [Test]
        public void ListValueNodeMaker_SimpleNumberList_ParsesCorrectly()
        {
            var text = "[1234, 5678, 91011], arg2: VALUE)";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            var result = ListValueNodeMaker.Instance.MakeNode(stream) as ListValueNode;
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Children.Count);
            Assert.IsTrue(result.Children.All(x => x is ScalarValueNode svn && svn.ValueType == ScalarValueType.Number));
            Assert.AreEqual("1234", ((ScalarValueNode)result.Children.ElementAt(0)).Value.ToString());
            Assert.AreEqual("5678", ((ScalarValueNode)result.Children.ElementAt(1)).Value.ToString());
            Assert.AreEqual("91011", ((ScalarValueNode)result.Children.ElementAt(2)).Value.ToString());

            // ensure stream is pointing beyond the end of the list
            Assert.AreEqual(TokenType.Name, stream.TokenType);
        }

        [Test]
        public void ListValueNodeMaker_SimpleStringList_ParsesCorrectly()
        {
            // use both types of string delimiters
            var text = "[\"bob\", \"\"\"Robert\"\"\"], arg2: VALUE)";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            var result = ListValueNodeMaker.Instance.MakeNode(stream) as ListValueNode;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Children.All(x => x is ScalarValueNode svn && svn.ValueType == ScalarValueType.String));
            Assert.AreEqual("\"bob\"", ((ScalarValueNode)result.Children[0]).Value.ToString());
            Assert.AreEqual("\"\"\"Robert\"\"\"", ((ScalarValueNode)result.Children[1]).Value.ToString());

            // ensure stream is pointing beyond the end of the list
            Assert.AreEqual(TokenType.Name, stream.TokenType);
        }

        [Test]
        public void ListValueNodeMaker_ComplexValueList_ParsesCorrectly()
        {
            var text = "[{arg1: 123, arg2: [456, 1234]}," +
                       "{arg1: 345, arg2: [982, 1231]}," +
                       "{arg3: 812, arg2: [13,31]}], arg2: VALUE)";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            var result = ListValueNodeMaker.Instance.MakeNode(stream) as ListValueNode;
            Assert.IsNotNull(result);

            Assert.AreEqual(3, result.Children.Count);
            Assert.IsTrue(result.Children.All(x => x is ComplexValueNode));
            foreach (var node in result.Children.OfType<ComplexValueNode>())
            {
                Assert.AreEqual(1, node.Children.Count);
                Assert.IsNotNull(node.Children[0] is InputItemCollectionNode);
                Assert.AreEqual(2, node.Children[0].Children.Count);
            }

            // ensure stream is pointing beyond the end of the list
            Assert.AreEqual(TokenType.Name, stream.TokenType);
        }

        [Test]
        public void ListValueNodeMaker_ListOfLists_ParsesCorrectly()
        {
            var text = "[[456, 1234],[982, 1231],[13,99]], arg2: VALUE)";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            var result = ListValueNodeMaker.Instance.MakeNode(stream) as ListValueNode;
            Assert.IsNotNull(result);

            // ensure all children were parsed to lists
            Assert.IsTrue(result.Children.All(x => x is ListValueNode));
        }

        [Test]
        public void ListValueNodeMaker_MixedValueList_ThrowsException()
        {
            var text = "[123, \"\"\"Robert\"\"\"], arg2: VALUE)";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            var result = ListValueNodeMaker.Instance.MakeNode(stream) as ListValueNode;
            Assert.AreEqual(2, result.Children.Count);
            Assert.IsTrue(result.Children[0] is ScalarValueNode child1 && child1.ValueType == ScalarValueType.Number);
            Assert.IsTrue(result.Children[1] is ScalarValueNode child2 && child2.ValueType == ScalarValueType.String);
        }

        [Test]
        public void ListValueNodeMaker_SameValueListWithNullInList_ParasesFine()
        {
            var text = "[123, null, 456], arg2: VALUE)";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            var result = ListValueNodeMaker.Instance.MakeNode(stream) as ListValueNode;
            Assert.IsNotNull(result);

            Assert.AreEqual(3, result.Children.Count);

            Assert.IsTrue(result.Children[0] is ScalarValueNode child1 && child1.ValueType == ScalarValueType.Number);
            Assert.IsTrue(result.Children[1] is NullValueNode);
            Assert.IsTrue(result.Children[2] is ScalarValueNode child2 && child2.ValueType == ScalarValueType.Number);
        }

        [Test]
        public void ListValueNodeMaker_NotPointingAtAList_ThrowsException()
        {
            var text = "someName(arg1: [123, 456], arg2: \"jane\")";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            Assert.Throws<GraphQLSyntaxException>(() => { ListValueNodeMaker.Instance.MakeNode(stream); });
        }

        [Test]
        public void ListValueNodeMaker_UnclosedNestedListOfLists_ThrowsException()
        {
            var text = "[[456, 1234],[982, 1231],[13,99], arg2: VALUE)";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));

            stream.Prime();
            Assert.Throws<GraphQLSyntaxException>(() => { ListValueNodeMaker.Instance.MakeNode(stream); });
        }
    }
}