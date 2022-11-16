// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Parsing2.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Parsing2;
    using GraphQL.AspNet.Parsing2.Lexing;
    using GraphQL.AspNet.Parsing2.Lexing.Source;
    using GraphQL.AspNet.Parsing2.Lexing.Tokens;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using NUnit.Framework;

    public class HelperAsserts
    {
        public static void AssertTokenChain(
            TokenStream tokenStream,
            params LexicalTokenTestCase[] expectedTokens)
        {
            tokenStream.Prime(false);

            var listFound = new List<LexicalToken>();
            listFound.Add(tokenStream.ActiveToken);
            var i = 0;

            do
            {
                var expected = expectedTokens[i];
                AssertActiveToken(ref tokenStream, expected.Text.ToString(), expected.TokenType);
                tokenStream.Next(false);
                listFound.Add(tokenStream.ActiveToken);
                i++;
            }
            while (tokenStream.TokenType != TokenType.EndOfFile);

            Assert.AreEqual(listFound.Count, expectedTokens.Length, $"Expected {expectedTokens.Length} tokens, got {i}");
        }

        /// <summary>
        /// Asserts that the token meets common checks.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="expectedText">The expected text.</param>
        /// <param name="expectedType">The expected type of the 'TokenType' property.</param>
        /// <param name="expectedAbsolutePosition">The expected absolute position.</param>
        public static void AssertActiveToken(ref TokenStream tokenStream, string expectedText, TokenType expectedType, int? expectedAbsolutePosition = null)
        {
            var token = tokenStream.ActiveToken;
            var text = tokenStream.ActiveTokenText;
            Assert.AreEqual(expectedType, token.TokenType);
            Assert.AreEqual(expectedText, text.ToString());

            if (expectedAbsolutePosition != null)
            {
                Assert.AreEqual(expectedAbsolutePosition.Value, token.Location.AbsoluteIndex);
            }
        }

        public static void AssertSynNodeChain(SourceText sourceText, SyntaxTree tree, SynNodeTestCase topLevelNode)
        {
            AssertSynNodeChain(sourceText, tree, tree.RootNode, topLevelNode);
        }

        public static void AssertSynNodeChain(SourceText sourceText, SyntaxTree tree, SyntaxNode node, SynNodeTestCase testCase)
        {
            if (testCase.NodeType.HasValue)
                Assert.AreEqual(testCase.NodeType, node.NodeType);

            if (testCase.PrimaryValueType.HasValue)
                Assert.AreEqual(testCase.PrimaryValueType.Value, node.PrimaryValue.ValueType);

            var primaryText = sourceText.Slice(node.PrimaryValue.TextBlock);
            var secondaryText = sourceText.Slice(node.SecondaryValue.TextBlock);
            Assert.AreEqual(testCase.PrimaryText, primaryText.ToString());
            Assert.AreEqual(testCase.SecondaryText, secondaryText.ToString());

            if (testCase.Children.Length > 0)
                AssertChildNodeChain(sourceText, tree, node, testCase.Children);
        }

        public static void AssertChildNodeChain(SourceText sourceText, SyntaxTree tree, SyntaxNode node, params SynNodeTestCase[] childTestCases)
        {
            if (childTestCases.Length == 1 && childTestCases[0] == SynNodeTestCase.NoChildren)
            {
                Assert.AreEqual(0, node.Coordinates.ChildBlockLength);
                return;
            }
            else if (childTestCases.Length > 0)
            {
                for (var i = 0; i < childTestCases.Length; i++)
                {
                    var childNode = tree.NodePool[node.Coordinates.ChildBlockIndex][i];
                    AssertSynNodeChain(sourceText, tree, childNode, childTestCases[i]);
                }
            }
        }
    }
}