// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Parsing2.NodeBuilders.Inputs
{
    using System;
    using GraphQL.AspNet.Parsing2.Lexing;
    using GraphQL.AspNet.Parsing2.Exceptions;
    using GraphQL.AspNet.Parsing2.Lexing.Source;
    using GraphQL.AspNet.Parsing2;
    using GraphQL.AspNet.Parsing2.NodeBuilders.Inputs;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using NUnit.Framework;
    using GraphQL.AspNet.Tests.Parsing2.Helpers;

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

            var tree = SynTree.FromDocumentRoot();
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
                SynTreeOperations.Release(ref tree);
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

            var tree = SynTree.FromDocumentRoot();
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
                SynTreeOperations.Release(ref tree);
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

            var tree = SynTree.FromDocumentRoot();
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
                SynTreeOperations.Release(ref tree);
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void VariableNode_WithDefault_SetsNameCorrectly()
        {
            var text = @"$episode: Episode = JEDI";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            VariableNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.Variable,
                    "episode",
                    "Episode",
                    new SynNodeTestCase(
                        SynNodeType.EnumValue,
                        "JEDI")));

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void VariableNode_WithNotNull_SetsFlagCorrectly()
        {
            var text = @"$episode: Episode! = JEDI";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            VariableNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.Variable,
                    "episode",
                    "Episode!",
                    new SynNodeTestCase(
                        SynNodeType.EnumValue,
                        "JEDI")));

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void VariableNode_WithNoDefault_SetsNameCorrectly()
        {
            var text = @"$episode: Episode";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            VariableNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.Variable,
                    "episode",
                    "Episode"));

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void VariableNode_WithComplexTypeExpression_SetsNameCorrectly()
        {
            var text = @"$episode: [[Episode!]!]! = JEDI";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            VariableNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.Variable,
                    "episode",
                    "[[Episode!]!]!",
                    new SynNodeTestCase(
                        SynNodeType.EnumValue,
                        "JEDI")));

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void VariableNode_WithDirective_AssignsDirectiveNode()
        {
            var text = @"$episode: Episode @myDirective";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            VariableNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.Variable,
                    "episode",
                    "Episode",
                    new SynNodeTestCase(
                        SynNodeType.Directive,
                        "myDirective")));

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void VariableNode_WithDirectiveAndParams_AssignsDirectiveNode()
        {
            var text = @"$episode: Episode @myDirective(param1: ""value1"")";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            VariableNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.Variable,
                    "episode",
                    "Episode",
                    new SynNodeTestCase(
                        SynNodeType.Directive,
                        "myDirective",
                        new SynNodeTestCase(
                            SynNodeType.InputItemCollection,
                            new SynNodeTestCase(
                                SynNodeType.InputItem,
                                "param1",
                                new SynNodeTestCase(
                                    SynNodeType.ScalarValue,
                                    "\"value1\"",
                                    ScalarValueType.String))))));

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void VariableNode_WithDefaultValue_DirectiveAndParams_AssignsDirectiveNode()
        {
            var text = @"$episode: Episode = JEDI @myDirective(param1: ""value1"")";
            var stream = Lexer.Tokenize(new SourceText(text.AsSpan()));
            stream.Prime();

            var tree = SynTree.FromDocumentRoot();
            var docNode = tree.RootNode;

            VariableNodeBuilder.Instance.BuildNode(ref tree, ref docNode, ref stream);

            HelperAsserts.AssertChildNodeChain(
                stream.Source,
                tree,
                docNode,
                new SynNodeTestCase(
                    SynNodeType.Variable,
                    "episode",
                    "Episode",
                    new SynNodeTestCase(
                        SynNodeType.EnumValue,
                        "JEDI"),
                    new SynNodeTestCase(
                        SynNodeType.Directive,
                        "myDirective",
                        new SynNodeTestCase(
                            SynNodeType.InputItemCollection,
                            new SynNodeTestCase(
                                SynNodeType.InputItem,
                                "param1",
                                new SynNodeTestCase(
                                    SynNodeType.ScalarValue,
                                    "\"value1\"",
                                    ScalarValueType.String))))));

            SynTreeOperations.Release(ref tree);
        }
    }
}