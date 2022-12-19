// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.Parsing.NodeBuilders.Inputs
{
    using System;
    using GraphQL.AspNet.Execution.Parsing;
    using GraphQL.AspNet.Execution.Parsing.Exceptions;
    using GraphQL.AspNet.Execution.Parsing.Lexing;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;
    using GraphQL.AspNet.Execution.Parsing.NodeBuilders.Inputs;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Execution.Parsing.Helpers;
    using NUnit.Framework;

    [TestFixture]
    public class VariableNodeBuilderTests
    {
        [Test]
        public void VariableNode_ImproperNameDeclaration_ThrowsException()
        {
            // no leading $ on the variable name
            var text = @"episode: Episode";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                VariableNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }
            finally
            {
                SyntaxTreeOperations.Release(ref tree);
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void VariableNode_NoTypeDeclarationNoDefault_ThrowsException()
        {
            // no Type name
            var text = @"$episode";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                VariableNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }
            finally
            {
                SyntaxTreeOperations.Release(ref tree);
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void VariableNode_NoTypeDeclarationWithDefault_ThrowsException()
        {
            // no Type name but with a default, still fails
            var text = @"$episode = JEDI";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            try
            {
                VariableNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }
            finally
            {
                SyntaxTreeOperations.Release(ref tree);
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void VariableNode_WithDefault_SetsNameCorrectly()
        {
            var text = @"$episode: Episode = JEDI";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            VariableNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.Variable,
                    "episode",
                    "Episode",
                    new SynNodeTestCase(
                        SyntaxNodeType.EnumValue,
                        "JEDI")));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void VariableNode_WithNotNull_SetsFlagCorrectly()
        {
            var text = @"$episode: Episode! = JEDI";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            VariableNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.Variable,
                    "episode",
                    "Episode!",
                    new SynNodeTestCase(
                        SyntaxNodeType.EnumValue,
                        "JEDI")));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void VariableNode_WithNoDefault_SetsNameCorrectly()
        {
            var text = @"$episode: Episode";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            VariableNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.Variable,
                    "episode",
                    "Episode"));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void VariableNode_WithComplexTypeExpression_SetsNameCorrectly()
        {
            var text = @"$episode: [[Episode!]!]! = JEDI";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            VariableNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.Variable,
                    "episode",
                    "[[Episode!]!]!",
                    new SynNodeTestCase(
                        SyntaxNodeType.EnumValue,
                        "JEDI")));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void VariableNode_WithDirective_AssignsDirectiveNode()
        {
            var text = @"$episode: Episode @myDirective";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            VariableNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.Variable,
                    "episode",
                    "Episode",
                    new SynNodeTestCase(
                        SyntaxNodeType.Directive,
                        "myDirective")));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void VariableNode_WithDirectiveAndParams_AssignsDirectiveNode()
        {
            var text = @"$episode: Episode @myDirective(param1: ""value1"")";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            VariableNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.Variable,
                    "episode",
                    "Episode",
                    new SynNodeTestCase(
                        SyntaxNodeType.Directive,
                        "myDirective",
                        new SynNodeTestCase(
                            SyntaxNodeType.InputItemCollection,
                            new SynNodeTestCase(
                                SyntaxNodeType.InputItem,
                                "param1",
                                new SynNodeTestCase(
                                    SyntaxNodeType.ScalarValue,
                                    "\"value1\"",
                                    ScalarValueType.String))))));

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void VariableNode_WithDefaultValue_DirectiveAndParams_AssignsDirectiveNode()
        {
            var text = @"$episode: Episode = JEDI @myDirective(param1: ""value1"")";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SyntaxTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            VariableNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SyntaxNodeType.Variable,
                    "episode",
                    "Episode",
                    new SynNodeTestCase(
                        SyntaxNodeType.EnumValue,
                        "JEDI"),
                    new SynNodeTestCase(
                        SyntaxNodeType.Directive,
                        "myDirective",
                        new SynNodeTestCase(
                            SyntaxNodeType.InputItemCollection,
                            new SynNodeTestCase(
                                SyntaxNodeType.InputItem,
                                "param1",
                                new SynNodeTestCase(
                                    SyntaxNodeType.ScalarValue,
                                    "\"value1\"",
                                    ScalarValueType.String))))));

            SyntaxTreeOperations.Release(ref tree);
        }
    }
}