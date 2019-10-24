// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Lexing.NodeMakers.Fragments
{
    using System;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing.NodeMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using NUnit.Framework;

    /// <summary>
    /// Tests around a top, document level, fragment node...not embedded in a field list.
    /// </summary>
    [TestFixture]
    public class NamedFragmentNodeMakerTests
    {
        [Test]
        public void FragmentRootNodeMaker_FragmentKeyWord_ParsesCorrectly()
        {
            var text = "fragment someFragment on User{}";
            var tokenStream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            tokenStream.Prime();

            var node = NamedFragmentNodeMaker.Instance.MakeNode(tokenStream) as NamedFragmentNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("someFragment", node.FragmentName.ToString());
            Assert.AreEqual("User", node.TargetType.ToArray());
            Assert.AreEqual(0, node.Children.Count);
        }

        [Test]
        public void FragmentRootNodeMaker_WithDirective_ParsesCorrectly()
        {
            var text = "fragment someFragment on User @skip(if: true){}";
            var tokenStream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            tokenStream.Prime();

            var node = NamedFragmentNodeMaker.Instance.MakeNode(tokenStream) as NamedFragmentNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("someFragment", node.FragmentName.ToString());
            Assert.AreEqual("User", node.TargetType.ToArray());
            Assert.AreEqual(1, node.Children.Count); // just the directive (field colleciton is empty and not included)

            var directive = node.Children.FirstOrDefault<DirectiveNode>();
            Assert.IsNotNull(directive);
            Assert.AreEqual("skip", directive.DirectiveName.ToString());
        }

        [Test]
        public void FragmentRootNodeMaker_NotAtFragmentKeyword_ThrowsException()
        {
            var text = "query someFragment on User{}";
            var tokenStream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            tokenStream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() => { NamedFragmentNodeMaker.Instance.MakeNode(tokenStream); });
        }

        [Test]
        public void FragmentRootNodeMaker_NoOnKeyword_ThrowsException()
        {
            var text = "fragment someFragment in User{}";
            var tokenStream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            tokenStream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() => { NamedFragmentNodeMaker.Instance.MakeNode(tokenStream); });
        }

        [Test]
        public void FragmentRootNodeMaker_NoTargetType_ThrowsException()
        {
            var text = "fragment someFragment on{}";
            var tokenStream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            tokenStream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() => { NamedFragmentNodeMaker.Instance.MakeNode(tokenStream); });
        }

        [Test]
        public void FragmentRootNodeMaker_NoFragmentName_ThrowsException()
        {
            var text = "fragment on User{}";
            var tokenStream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            tokenStream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() => { NamedFragmentNodeMaker.Instance.MakeNode(tokenStream); });
        }

        [Test]
        public void FragmentRootNodeMaker_NameIsANotAName_ThrowsException()
        {
            var text = "fragment 123 on User{}";
            var tokenStream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            tokenStream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() => { NamedFragmentNodeMaker.Instance.MakeNode(tokenStream); });
        }

        [Test]
        public void FragmentRootNodeMaker_TargetTypeNotAName_ThrowsException()
        {
            var text = "fragment someFragment on \"User\"{}";
            var tokenStream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            tokenStream.Prime();

            Assert.Throws<GraphQLSyntaxException>(() => { NamedFragmentNodeMaker.Instance.MakeNode(tokenStream); });
        }

        [Test]
        public void FragmentRootNodeMaker_WithAFieldSet_ParsesCorrectly()
        {
            var text = "fragment someFragment on User{field1, field2}";
            var tokenStream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            tokenStream.Prime();

            var node = NamedFragmentNodeMaker.Instance.MakeNode(tokenStream) as NamedFragmentNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("someFragment", node.FragmentName.ToString());
            Assert.AreEqual("User", node.TargetType.ToArray());
            Assert.AreEqual(1, node.Children.Count);

            var child = node.Children[0] as FieldCollectionNode;
            Assert.IsNotNull(child);
            Assert.AreEqual(2, child.Children.Count);
        }
    }
}