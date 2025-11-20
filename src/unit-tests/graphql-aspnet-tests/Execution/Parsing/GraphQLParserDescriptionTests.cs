// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.Parsing
{
    using GraphQL.AspNet.Execution.Parsing;
    using GraphQL.AspNet.Execution.Parsing.Exceptions;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using NUnit.Framework;

    /// <summary>
    /// Tests for GraphQL Spec (Sept 2025) description strings support.
    /// Ensures that description strings before operations, fragments, and variables
    /// are properly skipped and do not cause syntax exceptions.
    /// </summary>
    [TestFixture]
    public class GraphQLParserDescriptionTests
    {
        [Test]
        public void ParseDocument_OperationWithSingleLineDescription_ParsesSuccessfully()
        {
            var query = @"
                ""This is a query description""
                query MyQuery {
                    field1
                }";

            var parser = new GraphQLParser();
            var sourceText = new SourceText(query);
            var syntaxTree = SyntaxTree.WithDocumentRoot();
            parser.FillSyntaxTree(ref syntaxTree, ref sourceText);

            // Should have: RootNode -> Operation -> FieldCollection
            Assert.IsTrue(syntaxTree.BlockLength > 0);
            Assert.AreEqual(SyntaxNodeType.Document, syntaxTree.RootNode.NodeType);
        }

        [Test]
        public void ParseDocument_OperationWithMultiLineDescription_ParsesSuccessfully()
        {
            var query = @"
                """"""
                This is a multi-line
                query description
                """"""
                query MyQuery {
                    field1
                }";

            var parser = new GraphQLParser();
            var sourceText = new SourceText(query);
            var syntaxTree = SyntaxTree.WithDocumentRoot();
            parser.FillSyntaxTree(ref syntaxTree, ref sourceText);

            // Should have: RootNode -> Operation -> FieldCollection
            Assert.IsTrue(syntaxTree.BlockLength > 0);
            Assert.AreEqual(SyntaxNodeType.Document, syntaxTree.RootNode.NodeType);
        }

        [Test]
        public void ParseDocument_AnonymousQueryWithDescriptionAndKeyword_ThrowsSyntaxException()
        {
            var query = @"
                ""Anonymous query description""
                query {
                    field1
                }";

            // Descriptions are not allowed on anonymous operations (even with keyword)
            Assert.Throws<GraphQLSyntaxException>(() =>
            {
                var parser = new GraphQLParser();
                var sourceText = new SourceText(query);
                var syntaxTree = SyntaxTree.WithDocumentRoot();
                parser.FillSyntaxTree(ref syntaxTree, ref sourceText);
            });
        }

        [Test]
        public void ParseDocument_ShorthandQueryWithDescription_ThrowsSyntaxException()
        {
            var query = @"
                ""Shorthand query description""
                {
                    field1
                }";

            // Descriptions are not allowed on query shorthand
            Assert.Throws<GraphQLSyntaxException>(() =>
            {
                var parser = new GraphQLParser();
                var sourceText = new SourceText(query);
                var syntaxTree = SyntaxTree.WithDocumentRoot();
                parser.FillSyntaxTree(ref syntaxTree, ref sourceText);
            });
        }

        [Test]
        public void ParseDocument_AnonymousMutationWithDescription_ThrowsSyntaxException()
        {
            var query = @"
                ""Anonymous mutation description""
                mutation {
                    field1
                }";

            // Descriptions are not allowed on anonymous operations
            Assert.Throws<GraphQLSyntaxException>(() =>
            {
                var parser = new GraphQLParser();
                var sourceText = new SourceText(query);
                var syntaxTree = SyntaxTree.WithDocumentRoot();
                parser.FillSyntaxTree(ref syntaxTree, ref sourceText);
            });
        }

        [Test]
        public void ParseDocument_FragmentWithSingleLineDescription_ParsesSuccessfully()
        {
            var query = @"
                ""Fragment description""
                fragment MyFragment on SomeType {
                    field1
                    field2
                }";

            var parser = new GraphQLParser();
            var sourceText = new SourceText(query);
            var syntaxTree = SyntaxTree.WithDocumentRoot();
            parser.FillSyntaxTree(ref syntaxTree, ref sourceText);

            // Should parse successfully without throwing exceptions
            Assert.IsTrue(syntaxTree.BlockLength > 0);
            Assert.AreEqual(SyntaxNodeType.Document, syntaxTree.RootNode.NodeType);
        }

        [Test]
        public void ParseDocument_FragmentWithMultiLineDescription_ParsesSuccessfully()
        {
            var query = @"
                """"""
                This is a multi-line
                fragment description
                """"""
                fragment MyFragment on SomeType {
                    field1
                    field2
                }";

            var parser = new GraphQLParser();
            var sourceText = new SourceText(query);
            var syntaxTree = SyntaxTree.WithDocumentRoot();
            parser.FillSyntaxTree(ref syntaxTree, ref sourceText);

            // Should parse successfully without throwing exceptions
            Assert.IsTrue(syntaxTree.BlockLength > 0);
            Assert.AreEqual(SyntaxNodeType.Document, syntaxTree.RootNode.NodeType);
        }

        [Test]
        public void ParseDocument_VariableWithSingleLineDescription_ParsesSuccessfully()
        {
            var query = @"
                query MyQuery(
                    ""Variable description""
                    $myVar: String
                ) {
                    field1(arg: $myVar)
                }";

            var parser = new GraphQLParser();
            var sourceText = new SourceText(query);
            var syntaxTree = SyntaxTree.WithDocumentRoot();
            parser.FillSyntaxTree(ref syntaxTree, ref sourceText);

            // Should parse successfully without throwing exceptions
            Assert.IsTrue(syntaxTree.BlockLength > 0);
            Assert.AreEqual(SyntaxNodeType.Document, syntaxTree.RootNode.NodeType);
        }

        [Test]
        public void ParseDocument_VariableWithMultiLineDescription_ParsesSuccessfully()
        {
            var query = @"
                query MyQuery(
                    """"""
                    This is a multi-line
                    variable description
                    """"""
                    $myVar: String
                ) {
                    field1(arg: $myVar)
                }";

            var parser = new GraphQLParser();
            var sourceText = new SourceText(query);
            var syntaxTree = SyntaxTree.WithDocumentRoot();
            parser.FillSyntaxTree(ref syntaxTree, ref sourceText);

            // Should parse successfully without throwing exceptions
            Assert.IsTrue(syntaxTree.BlockLength > 0);
            Assert.AreEqual(SyntaxNodeType.Document, syntaxTree.RootNode.NodeType);
        }

        [Test]
        public void ParseDocument_VariableWithDescriptionAndStringDefaultValue_ParsesSuccessfully()
        {
            var query = @"
                query MyQuery(
                    ""Variable description""
                    $myVar: String = ""default value""
                ) {
                    field1(arg: $myVar)
                }";

            var parser = new GraphQLParser();
            var sourceText = new SourceText(query);
            var syntaxTree = SyntaxTree.WithDocumentRoot();
            parser.FillSyntaxTree(ref syntaxTree, ref sourceText);

            // Should parse correctly, not confusing description string with default value string
            Assert.IsTrue(syntaxTree.BlockLength > 0);
            Assert.AreEqual(SyntaxNodeType.Document, syntaxTree.RootNode.NodeType);
        }

        [Test]
        public void ParseDocument_MultipleVariablesWithDescriptions_ParsesSuccessfully()
        {
            var query = @"
                query MyQuery(
                    ""First variable description""
                    $var1: String
                    ""Second variable description""
                    $var2: Int
                    ""Third variable description""
                    $var3: Boolean
                ) {
                    field1(arg1: $var1, arg2: $var2, arg3: $var3)
                }";

            var parser = new GraphQLParser();
            var sourceText = new SourceText(query);
            var syntaxTree = SyntaxTree.WithDocumentRoot();
            parser.FillSyntaxTree(ref syntaxTree, ref sourceText);

            // Should parse all variables correctly
            Assert.IsTrue(syntaxTree.BlockLength > 0);
            Assert.AreEqual(SyntaxNodeType.Document, syntaxTree.RootNode.NodeType);
        }

        [Test]
        public void ParseDocument_MixedVariablesWithAndWithoutDescriptions_ParsesSuccessfully()
        {
            var query = @"
                query MyQuery(
                    ""Variable with description""
                    $var1: String
                    $var2: Int
                    ""Another variable with description""
                    $var3: Boolean
                ) {
                    field1(arg1: $var1, arg2: $var2, arg3: $var3)
                }";

            var parser = new GraphQLParser();
            var sourceText = new SourceText(query);
            var syntaxTree = SyntaxTree.WithDocumentRoot();
            parser.FillSyntaxTree(ref syntaxTree, ref sourceText);

            // Should parse all variables correctly, some with descriptions, some without
            Assert.IsTrue(syntaxTree.BlockLength > 0);
            Assert.AreEqual(SyntaxNodeType.Document, syntaxTree.RootNode.NodeType);
        }

        [Test]
        public void ParseDocument_MultipleOperationsAndFragmentsWithDescriptions_ParsesSuccessfully()
        {
            var query = @"
                ""Query description""
                query MyQuery {
                    field1
                    ...MyFragment
                }

                ""Mutation description""
                mutation MyMutation {
                    field2
                }

                ""Fragment description""
                fragment MyFragment on SomeType {
                    field3
                }";

            var parser = new GraphQLParser();
            var sourceText = new SourceText(query);
            var syntaxTree = SyntaxTree.WithDocumentRoot();
            parser.FillSyntaxTree(ref syntaxTree, ref sourceText);

            // Should parse all operations and fragments correctly
            Assert.IsTrue(syntaxTree.BlockLength > 0);
            Assert.AreEqual(SyntaxNodeType.Document, syntaxTree.RootNode.NodeType);
        }

        [Test]
        public void ParseDocument_VariableWithDescriptionAndMultiLineStringDefaultValue_ParsesSuccessfully()
        {
            var query = @"
                query MyQuery(
                    """"""
                    Multi-line variable description
                    with details
                    """"""
                    $myVar: String = """"""
                    Multi-line default
                    value string
                    """"""
                ) {
                    field1(arg: $myVar)
                }";

            var parser = new GraphQLParser();
            var sourceText = new SourceText(query);
            var syntaxTree = SyntaxTree.WithDocumentRoot();
            parser.FillSyntaxTree(ref syntaxTree, ref sourceText);

            // Should correctly distinguish between description and default value
            Assert.IsTrue(syntaxTree.BlockLength > 0);
            Assert.AreEqual(SyntaxNodeType.Document, syntaxTree.RootNode.NodeType);
        }

        [Test]
        public void ParseDocument_ComplexScenarioWithAllDescriptionTypes_ParsesSuccessfully()
        {
            var query = @"
                ""Main query description""
                query MainQuery(
                    ""ID variable description""
                    $id: ID!
                    ""Optional filter description""
                    $filter: String = ""default""
                ) {
                    user(id: $id, filter: $filter) {
                        name
                        ...UserDetails
                    }
                }

                ""User details fragment description""
                fragment UserDetails on User {
                    email
                    age
                }";

            var parser = new GraphQLParser();
            var sourceText = new SourceText(query);
            var syntaxTree = SyntaxTree.WithDocumentRoot();
            parser.FillSyntaxTree(ref syntaxTree, ref sourceText);

            // Should parse entire complex document correctly
            Assert.IsTrue(syntaxTree.BlockLength > 0);
            Assert.AreEqual(SyntaxNodeType.Document, syntaxTree.RootNode.NodeType);
        }
    }
}