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
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing.NodeMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using NUnit.Framework;

    /// <summary>
    /// Defines tests related to creation of the opening, top level
    /// <see cref="OperationTypeNode"/> under which a complete document segment is defined.
    /// </summary>
    [TestFixture]
    public class OperationTypeNodeTests
    {
        [Test]
        public void OperationTypeNodeMaker_UnnamedQueryisQueryOperationWithNoName()
        {
            var text = @"{}";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = OperationTypeNodeMaker.Instance.MakeNode(stream) as OperationTypeNode;
            Assert.IsNotNull(node);
            Assert.AreEqual(string.Empty, node.OperationType.ToString());
            Assert.AreEqual(string.Empty, node.OperationName.ToString());
        }

        [Test]
        public void OperationTypeNodeMaker_OnlyOperationTypeDeclared()
        {
            var text = @"namedQueryType {}";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = OperationTypeNodeMaker.Instance.MakeNode(stream) as OperationTypeNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("namedQueryType", node.OperationType.ToString());
            Assert.AreEqual(string.Empty, node.OperationName.ToString());
        }

        [Test]
        public void OperationTypeNodeMaker_NameAndTypeDeclaration()
        {
            var text = @"namedQueryType aNamedQueryName{}";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = OperationTypeNodeMaker.Instance.MakeNode(stream) as OperationTypeNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("namedQueryType", node.OperationType.ToString());
            Assert.AreEqual("aNamedQueryName", node.OperationName.ToString());
        }

        [Test]
        public void OperationTypeNodeMaker_NamedQuery_WithDirective_SetsNameCorrectly()
        {
            var text = @"namedQueryType aNamedQuery @someDirective(if: true){}";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var node = OperationTypeNodeMaker.Instance.MakeNode(stream) as OperationTypeNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("namedQueryType", node.OperationType.ToString());
            Assert.AreEqual("aNamedQuery", node.OperationName.ToString());
            Assert.AreEqual(1, node.Children.Count);

            var directive = node.Children.FirstOrDefault<DirectiveNode>();
            Assert.IsNotNull(directive);
            Assert.AreEqual("someDirective", directive.DirectiveName.ToString());
        }
    }
}