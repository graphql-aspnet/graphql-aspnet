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
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing2;
    using GraphQL.AspNet.Parsing2.NodeBuilders;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class OperationNodeBuilderTests
    {
        [Test]
        public void VariableCollectionIsParsed()
        {
            var text = @"query namedQuery($var1: EPISODE = JEDI){}";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            OperationNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                ref tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.Operation,
                    primaryText: "query",
                    secondaryText: "namedQuery",
                    new SynNodeTestCase(
                        SynNodeType.VariableCollection)));
        }

        [Test]
        public void UnnamedQueryisQueryOperationWithNoName()
        {
            var text = @"{}";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            OperationNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertSynNodeChain(
                ref tree,
                new SynNodeTestCase(
                    SynNodeType.Document,
                    new SynNodeTestCase(SynNodeType.Operation)));
        }

        [Test]
        public void OnlyOperationTypeDeclared()
        {
            var text = @"namedQueryType {}";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            OperationNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertSynNodeChain(
                ref tree,
                new SynNodeTestCase(
                    SynNodeType.Document,
                    new SynNodeTestCase(
                        SynNodeType.Operation,
                        primaryText: "namedQueryType")));
        }

        [Test]
        public void NameAndTypeDeclaration()
        {
            var text = @"namedQueryType aNamedQueryName{}";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            OperationNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertSynNodeChain(
                ref tree,
                new SynNodeTestCase(
                    SynNodeType.Document,
                    new SynNodeTestCase(
                        SynNodeType.Operation,
                        primaryText: "namedQueryType",
                        secondaryText: "aNamedQueryName")));
        }

        [Test]
        public void NamedQuery_WithDirective_SetsNameCorrectly()
        {
            var text = @"namedQueryType aNamedQuery @someDirective(if: true){}";
            var stream = Lexer.Tokenize(new SourceText(text.AsMemory()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            OperationNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertSynNodeChain(
                ref tree,
                new SynNodeTestCase(
                    SynNodeType.Document,
                    new SynNodeTestCase(
                        SynNodeType.Operation,
                        primaryText: "namedQueryType",
                        primaryValueType: null,
                        secondaryText: "aNamedQuery",
                        new SynNodeTestCase(
                            SynNodeType.Directive,
                            primaryText: "someDirective"))));
        }
    }
}