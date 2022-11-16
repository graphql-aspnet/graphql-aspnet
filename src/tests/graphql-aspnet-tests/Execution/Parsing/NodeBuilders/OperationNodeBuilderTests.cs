// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Parsing2.NodeBuilders
{
    using System;
    using GraphQL.AspNet.Parsing2.Lexing;
    using GraphQL.AspNet.Parsing2.Lexing.Source;
    using GraphQL.AspNet.Parsing2;
    using GraphQL.AspNet.Parsing2.NodeBuilders;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using NUnit.Framework;
    using GraphQL.AspNet.Tests.Parsing2.Helpers;

    [TestFixture]
    public class OperationNodeBuilderTests
    {
        [Test]
        public void VariableCollectionIsParsed()
        {
            var text = @"query namedQuery($var1: EPISODE = JEDI){}";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            OperationNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.Operation,
                    primaryText: "query",
                    secondaryText: "namedQuery",
                    new SynNodeTestCase(
                        SyntaxNodeType.VariableCollection)));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void UnnamedQueryisQueryOperationWithNoName()
        {
            var text = @"{}";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            OperationNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertSynNodeChain(
                stream.Source,
                tree,
                new SynNodeTestCase(
                    SyntaxNodeType.Document,
                    new SynNodeTestCase(SyntaxNodeType.Operation)));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void OnlyOperationTypeDeclared()
        {
            var text = @"namedQueryType {}";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            OperationNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertSynNodeChain(
                stream.Source,
                tree,
                new SynNodeTestCase(
                    SyntaxNodeType.Document,
                    new SynNodeTestCase(
                        SyntaxNodeType.Operation,
                        primaryText: "namedQueryType")));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void NameAndTypeDeclaration()
        {
            var text = @"namedQueryType aNamedQueryName{}";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            OperationNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertSynNodeChain(
                stream.Source,
                tree,
                new SynNodeTestCase(
                    SyntaxNodeType.Document,
                    new SynNodeTestCase(
                        SyntaxNodeType.Operation,
                        primaryText: "namedQueryType",
                        secondaryText: "aNamedQueryName")));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void NamedQuery_WithDirective_SetsNameCorrectly()
        {
            var text = @"namedQueryType aNamedQuery @someDirective(if: true){}";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            OperationNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertSynNodeChain(
                stream.Source,
                tree,
                new SynNodeTestCase(
                    SyntaxNodeType.Document,
                    new SynNodeTestCase(
                        SyntaxNodeType.Operation,
                        primaryText: "namedQueryType",
                        primaryValueType: null,
                        secondaryText: "aNamedQuery",
                        new SynNodeTestCase(
                            SyntaxNodeType.Directive,
                            primaryText: "someDirective"))));

            SyntaxTreeOperations.Release(ref tree);
        }
    }
}