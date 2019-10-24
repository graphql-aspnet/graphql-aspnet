// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Lexing
{
    using System;
    using GraphQL.AspNet.Parsing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using NUnit.Framework;

    /// <summary>
    /// Tests centered around doing a complete parse of a
    /// source query into a properly formatted syntaxTree that can be executed by a schema.
    /// </summary>
    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void ParseDocument_NonQueryThrowsException()
        {
            var qualifiedQuery = string.Empty;

            Assert.Throws<GraphQLSyntaxException>(() =>
            {
                var parser = new GraphQLParser();
                var syntaxTree = parser.ParseQueryDocument(qualifiedQuery.AsMemory());
            });
        }

        [Test]
        public void ParseDocument_EmptyQueryParsesDefaultQuery()
        {
            var qualifiedQuery = "{}";

            var parser = new GraphQLParser();
            var syntaxTree = parser.ParseQueryDocument(qualifiedQuery.AsMemory());
            Assert.AreEqual(1, syntaxTree.Nodes.Count);
        }

        [Test]
        public void ParseDocument_InvalidDocumentThrowsException()
        {
            var text = "query someQuery{field1, field2";

            Assert.Throws<GraphQLSyntaxException>(() =>
            {
                var parser = new GraphQLParser();
                var syntaxTree = parser.ParseQueryDocument(text.AsMemory());
            });
        }

        [Test]
        public void ParseDocument_KitchenSinkParses()
        {
            var qualifiedQuery = ResourceLoader.ReadAllLines("KitchenSink", "KitchenSink.graphql");

            var parser = new GraphQLParser();
            var syntaxTree = parser.ParseQueryDocument(qualifiedQuery.AsMemory());
            Assert.IsNotNull(syntaxTree);
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
            var parser = new GraphQLParser();
            Assert.AreEqual(output, parser.StripInsignificantWhiteSpace(text));
        }
    }
}