// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Parsing.NodeMakers
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Parsing;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing.NodeMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
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
            try
            {
                VariableNodeMaker.Instance.MakeNode(new SyntaxTree(), ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void VariableNode_NoTypeDeclarationNoDefault_ThrowsException()
        {
            // no Type name
            var text = @"$episode";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            try
            {
                VariableNodeMaker.Instance.MakeNode(new SyntaxTree(), ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void VariableNode_NoTypeDeclarationWithDefault_ThrowsException()
        {
            // no Type name but with a default, still fails
            var text = @"$episode = JEDI";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();
            try
            {
                VariableNodeMaker.Instance.MakeNode(new SyntaxTree(), ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void VariableNode_WithDefault_SetsNameCorrectly()
        {
            var text = @"$episode: Episode = JEDI";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = VariableNodeMaker.Instance.MakeNode(new SyntaxTree(), ref stream) as VariableNode;
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

            var node = VariableNodeMaker.Instance.MakeNode(new SyntaxTree(), ref stream) as VariableNode;
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

            var node = VariableNodeMaker.Instance.MakeNode(new SyntaxTree(), ref stream) as VariableNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("episode", node.Name.ToString());
            Assert.AreEqual("Episode", node.TypeExpression.ToString());

            // has no default value set
            Assert.IsNull(node.Children);
        }

        [Test]
        public void VariableNode_WithComplexTypeExpression_SetsNameCorrectly()
        {
            var text = @"$episode: [[Episode!]!]! = JEDI";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = VariableNodeMaker.Instance.MakeNode(new SyntaxTree(), ref stream) as VariableNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("episode", node.Name.ToString());
            Assert.AreEqual("[[Episode!]!]!", node.TypeExpression.ToString());

            var defaultValue = node.Children.FirstOrDefault<InputValueNode>() as EnumValueNode;
            Assert.IsNotNull(defaultValue);
            Assert.AreEqual("JEDI", defaultValue.Value.ToString());
        }

        [Test]
        public void VariableNode_WithDirective_AssignsDirectiveNode()
        {
            var text = @"$episode: Episode @myDirective";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = VariableNodeMaker.Instance.MakeNode(new SyntaxTree(), ref stream) as VariableNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("episode", node.Name.ToString());
            Assert.AreEqual("Episode", node.TypeExpression.ToString());

            // has directive child
            var directiveNode = node.Children.FirstOrDefault<DirectiveNode>();
            Assert.IsNotNull(directiveNode);
            Assert.AreEqual("myDirective", directiveNode.DirectiveName.ToString());
        }

        [Test]
        public void VariableNode_WithDirectiveAndParams_AssignsDirectiveNode()
        {
            var text = @"$episode: Episode @myDirective(param1: ""value1"")";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = VariableNodeMaker.Instance.MakeNode(new SyntaxTree(), ref stream) as VariableNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("episode", node.Name.ToString());
            Assert.AreEqual("Episode", node.TypeExpression.ToString());

            // has directive child
            var directiveNode = node.Children.FirstOrDefault<DirectiveNode>();
            Assert.IsNotNull(directiveNode);
            Assert.AreEqual("myDirective", directiveNode.DirectiveName.ToString());

            var inputCollection = directiveNode.Children.ElementAt(0) as InputItemCollectionNode;
            Assert.IsNotNull(inputCollection);
            Assert.AreEqual(1, inputCollection.Children.Count);

            var child = inputCollection.Children.ElementAt(0) as InputItemNode;
            Assert.IsNotNull(child);
            Assert.AreEqual("param1", child.InputName.ToString());
            Assert.AreEqual(1, child.Children.Count);

            var value = child.Children.ElementAt(0) as ScalarValueNode;
            Assert.IsNotNull(value);
            Assert.AreEqual(ScalarValueType.String, value.ValueType);
            Assert.AreEqual("\"value1\"", value.Value.ToString());
        }

        [Test]
        public void VariableNode_WithDefaultValue_DirectiveAndParams_AssignsDirectiveNode()
        {
            var text = @"$episode: Episode = JEDI @myDirective(param1: ""value1"")";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = VariableNodeMaker.Instance.MakeNode(new SyntaxTree(), ref stream) as VariableNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("episode", node.Name.ToString());
            Assert.AreEqual("Episode", node.TypeExpression.ToString());

            // default value child
            var defaultValue = node.Children.FirstOrDefault<InputValueNode>() as EnumValueNode;
            Assert.IsNotNull(defaultValue);
            Assert.AreEqual("JEDI", defaultValue.Value.ToString());

            // has directive child
            var directiveNode = node.Children.FirstOrDefault<DirectiveNode>();
            Assert.IsNotNull(directiveNode);
            Assert.AreEqual("myDirective", directiveNode.DirectiveName.ToString());

            var inputCollection = directiveNode.Children.ElementAt(0) as InputItemCollectionNode;
            Assert.IsNotNull(inputCollection);
            Assert.AreEqual(1, inputCollection.Children.Count);

            var child = inputCollection.Children.ElementAt(0) as InputItemNode;
            Assert.IsNotNull(child);
            Assert.AreEqual("param1", child.InputName.ToString());
            Assert.AreEqual(1, child.Children.Count);

            var value = child.Children.ElementAt(0) as ScalarValueNode;
            Assert.IsNotNull(value);
            Assert.AreEqual(ScalarValueType.String, value.ValueType);
            Assert.AreEqual("\"value1\"", value.Value.ToString());
        }
    }
}