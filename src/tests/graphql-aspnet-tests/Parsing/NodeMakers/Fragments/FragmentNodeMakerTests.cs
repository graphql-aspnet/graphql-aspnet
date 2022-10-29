// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Parsing.NodeMakers.Fragments
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Parsing;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing.NodeMakers.FieldMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentConstruction.Steps;
    using NUnit.Framework;

    [TestFixture]
    public class FragmentNodeMakerTests
    {
        [Test]
        public void FragmentSpread_FragmentName_ParsesCorrectly()
        {
            var text = "...someFragmentA,";
            var tokenStream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            tokenStream.Prime();

            var syntaxTree = new SyntaxTree();

            var node = FragementNodeMaker.Instance.MakeNode(syntaxTree, ref tokenStream) as FragmentSpreadNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("someFragmentA", node.PointsToFragmentName.ToString());
            Assert.IsNull(node.Children);
        }

        [Test]
        public void FragmentSpread_WithDirective_ParsesCorrectly()
        {
            var text = "...someFragmentA @skip(if: true),";
            var tokenStream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            tokenStream.Prime();

            var syntaxTree = new SyntaxTree();

            var node = FragementNodeMaker.Instance.MakeNode(syntaxTree, ref tokenStream) as FragmentSpreadNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("someFragmentA", node.PointsToFragmentName.ToString());
            Assert.AreEqual(1, node.Children.Count);

            var directive = node.Children.FirstOrDefault<DirectiveNode>();
            Assert.IsNotNull(directive);
            Assert.AreEqual("skip", directive.DirectiveName.ToString());
        }

        [Test]
        public void FieldFragmentNodeMaker_NoName_ThrowsEceptions()
        {
            var text = "...,";
            var tokenStream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            tokenStream.Prime();

            try
            {
                FragementNodeMaker.Instance.MakeNode(new SyntaxTree(), ref tokenStream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void FieldFragmentNodeMaker_InvalidName_ThrowsEceptions()
        {
            var text = "...123A,";
            var tokenStream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            tokenStream.Prime();

            try
            {
                FragementNodeMaker.Instance.MakeNode(new SyntaxTree(), ref tokenStream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void FragmentNode_ValidInlineFragment_NoFields_ParsesCorrectly()
        {
            var text = "...on User{},";
            var tokenStream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            tokenStream.Prime();

            var syntaxTree = new SyntaxTree();

            var node = FragementNodeMaker.Instance.MakeNode(syntaxTree, ref tokenStream) as InlineFragmentNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("User", node.TargetType.ToString());
            Assert.IsNull(node.Children);
        }

        [Test]
        public void FragmentNode_ValidInlineFragment_NoFields_WithDirective_ParsesCorrectly()
        {
            var text = "...on User @skip(if: true) {},";
            var tokenStream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            tokenStream.Prime();

            var node = FragementNodeMaker.Instance.MakeNode(new SyntaxTree(), ref tokenStream) as InlineFragmentNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("User", node.TargetType.ToString());
            Assert.AreEqual(1, node.Children.Count);

            var directive = node.Children.FirstOrDefault<DirectiveNode>();
            Assert.IsNotNull(directive);
            Assert.AreEqual("skip", directive.DirectiveName.ToString());
        }

        [Test]
        public void FragmentNode_ValidInlineFragment_WithFields_ParsesCorrectly()
        {
            var text = "...on User{field1, field2},";
            var tokenStream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            tokenStream.Prime();

            var node = FragementNodeMaker.Instance.MakeNode(new SyntaxTree(), ref tokenStream) as InlineFragmentNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("User", node.TargetType.ToString());
            Assert.AreEqual(1, node.Children.Count);

            var collection = node.Children.ElementAt(0) as FieldCollectionNode;
            Assert.IsNotNull(collection);
            Assert.AreEqual(2, collection.Children.Count);
        }
    }
}