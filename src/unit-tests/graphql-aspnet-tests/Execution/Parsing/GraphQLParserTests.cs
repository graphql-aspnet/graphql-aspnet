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
    using GraphQL.AspNet.Tests.CommonHelpers;
    using NUnit.Framework;

    /// <summary>
    /// Tests centered around doing a complete parse of a
    /// source query into a properly formatted syntaxTree that can be executed by a schema.
    /// </summary>
    [TestFixture]
    public class GraphQLParserTests
    {
        [Test]
        public void ParseDocument_NonQueryThrowsException()
        {
            var qualifiedQuery = string.Empty;

            Assert.Throws<GraphQLSyntaxException>(() =>
            {
                var parser = new GraphQLParser();
                var sourceText = new SourceText(qualifiedQuery);
                var syntaxTree = SyntaxTree.FromDocumentRoot();
                parser.CreateSyntaxTree(ref syntaxTree, ref sourceText);
            });
        }

        [Test]
        public void ParseDocument_EmptyQueryParsesDefaultQuery()
        {
            var qualifiedQuery = "{}";

            var parser = new GraphQLParser();
            var sourceText = new SourceText(qualifiedQuery);
            var syntaxTree = SyntaxTree.FromDocumentRoot();
            parser.CreateSyntaxTree(ref syntaxTree, ref sourceText);

            // RootNode | Operation -> Empty Field Set
            Assert.AreEqual(2, syntaxTree.BlockLength);
        }

        [Test]
        public void ParseDocument_InvalidDocumentThrowsException()
        {
            var text = "query someQuery{field1, field2";

            Assert.Throws<GraphQLSyntaxException>(() =>
            {
                var parser = new GraphQLParser();
                var sourceText = new SourceText(text);
                var syntaxTree = SyntaxTree.FromDocumentRoot();
                parser.CreateSyntaxTree(ref syntaxTree, ref sourceText);
            });
        }

        [Test]
        public void ParseDocument_KitchenSinkParses_DoesNotThrowExeception()
        {
            var qualifiedQuery = ResourceLoader.ReadAllLines("KitchenSink", "KitchenSink.graphql");

            var parser = new GraphQLParser();
            var sourceText = new SourceText(qualifiedQuery);
            var syntaxTree = SyntaxTree.FromDocumentRoot();
            parser.CreateSyntaxTree(ref syntaxTree, ref sourceText);
            Assert.IsTrue(syntaxTree.BlockLength > 0);

            SyntaxTreeOperations.Release(ref syntaxTree);
        }

        [TestCase("abc", "abc")]
        [TestCase("a\rb\nc\t", "a b c ")]
        [TestCase("abc ", "abc ")]
        [TestCase("abc    \r\n\t", "abc ")]
        [TestCase("   abc ", " abc ")]
        [TestCase("   ab c ", " ab c ")]
        [TestCase("   ab c   d    ", " ab c d ")]
        [TestCase("a b c", "a b c")]
        [TestCase("a \n   \rb\n  \r  \tc", "a b c")]
        public void StripInsignificantWhiteSpace_StripsAsExpected(string text, string output)
        {
            Assert.AreEqual(output, GraphQLParser.StripInsignificantWhiteSpace(text));
        }
    }
}