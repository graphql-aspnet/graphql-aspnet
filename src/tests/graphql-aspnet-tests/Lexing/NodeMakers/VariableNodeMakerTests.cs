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
    using GraphQL.AspNet.Parsing.NodeMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using NUnit.Framework;

    /// <summary>
    /// A set of tests dealing with the parsing of a defined variable collection and the generation of a
    /// <see cref="VariableNode"/>.
    /// </summary>
    [TestFixture]
    public class VariableNodeMakerTests
    {
        [Test]
        public void VariableNode_ImproperNameDeclaration_ThrowsException()
        {
            // no leading $ on the variable name
            var text = @"episode: Episode";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();
            Assert.Throws<GraphQLSyntaxException>(() => { VariableNodeMaker.Instance.MakeNode(stream); });
        }

        [Test]
        public void VariableNode_NoTypeDeclarationNoDefault_ThrowsException()
        {
            // no Type name
            var text = @"$episode";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() => { VariableNodeMaker.Instance.MakeNode(stream); });
        }

        [Test]
        public void VariableNode_NoTypeDeclarationWithDefault_ThrowsException()
        {
            // no Type name but with a default, still fails
            var text = @"$episode = JEDI";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();
            Assert.Throws<GraphQLSyntaxException>(() => { VariableNodeMaker.Instance.MakeNode(stream); });
        }

        [Test]
        public void VariableNode_WithDefault_SetsNameCorrectly()
        {
            var text = @"$episode: Episode = JEDI";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = VariableNodeMaker.Instance.MakeNode(stream) as VariableNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("episode", node.Name.ToString());
            Assert.AreEqual("Episode", node.TypeExpression.ToString());

            // has no default value set
            var defaultValue = node.Children.FirstOrDefault<InputValueNode>() as EnumValueNode;
            Assert.IsNotNull(defaultValue);
            Assert.AreEqual("JEDI", defaultValue.Value.ToString());
        }

        [Test]
        public void VariableNode_WithNotNull_SetsFlagCorrectly()
        {
            var text = @"$episode: Episode! = JEDI";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = VariableNodeMaker.Instance.MakeNode(stream) as VariableNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("episode", node.Name.ToString());
            Assert.AreEqual("Episode!", node.TypeExpression.ToString());

            var defaultValue = node.Children.FirstOrDefault<InputValueNode>() as EnumValueNode;
            Assert.IsNotNull(defaultValue);
            Assert.AreEqual("JEDI", defaultValue.Value.ToString());
        }

        [Test]
        public void VariableNode_WithNoDefault_SetsNameCorrectly()
        {
            var text = @"$episode: Episode";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = VariableNodeMaker.Instance.MakeNode(stream) as VariableNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("episode", node.Name.ToString());
            Assert.AreEqual("Episode", node.TypeExpression.ToString());

            // has no default value set
            var defaultValue = node.Children.FirstOrDefault<InputValueNode>();
            Assert.IsNull(defaultValue);
        }

        [Test]
        public void VariableNode_WithComplexTypeExpression_SetsNameCorrectly()
        {
            var text = @"$episode: [[Episode!]!]! = JEDI";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = VariableNodeMaker.Instance.MakeNode(stream) as VariableNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("episode", node.Name.ToString());
            Assert.AreEqual("[[Episode!]!]!", node.TypeExpression.ToString());

            var defaultValue = node.Children.FirstOrDefault<InputValueNode>() as EnumValueNode;
            Assert.IsNotNull(defaultValue);
            Assert.AreEqual("JEDI", defaultValue.Value.ToString());
        }
    }
}