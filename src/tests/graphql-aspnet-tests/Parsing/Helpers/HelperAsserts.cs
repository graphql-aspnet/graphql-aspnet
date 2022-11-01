// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Parsing.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using NUnit.Framework;

    /// <summary>
    /// Some commonly used assert statements for various objects.
    /// </summary>
    public static class HelperAsserts
    {
        public static void AssertTokenChain(ref TokenStream tokenSet, params LexicalToken[] expectedTokens)
        {
            tokenSet.Prime();

            var listFound = new List<LexicalToken>();
            listFound.Add(tokenSet.ActiveToken);
            var i = 0;

            do
            {
                var expected = expectedTokens[i];
                AssertToken(tokenSet.ActiveToken, expected.Text.ToString(), expected.TokenType);
                tokenSet.Next(false);
                listFound.Add(tokenSet.ActiveToken);
                i++;
            }
            while (tokenSet.TokenType != TokenType.EndOfFile);

            Assert.AreEqual(listFound.Count, expectedTokens.Length, $"Expected {expectedTokens.Length} tokens, got {i}");
        }

        /// <summary>
        /// Asserts that the token meets common checks.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="expectedText">The expected text.</param>
        /// <param name="expectedType">The expected type of the 'TokenType' property.</param>
        /// <param name="expectedAbsolutePosition">The expected absolute position.</param>
        public static void AssertToken(LexicalToken token, string expectedText, TokenType expectedType, int? expectedAbsolutePosition = null)
        {
            Assert.IsNotNull(token);
            Assert.AreEqual(expectedType, token.TokenType);
            Assert.AreEqual(expectedText, token.Text.ToString());
            Assert.IsNotNull(token.Location, "Token does not contain a valid location");
            if (expectedAbsolutePosition != null)
            {
                Assert.AreEqual(expectedAbsolutePosition.Value, token.Location.AbsoluteIndex);
            }
        }

        /// <summary>
        /// Asserts that the token set provided ends with an "End of File" token.
        /// </summary>
        /// <param name="tokenSet">The token set.</param>
        public static void AssertEndsWithEoF(IEnumerable<LexicalToken> tokenSet)
        {
            Assert.IsNotNull(tokenSet);
            Assert.IsTrue(tokenSet.Any());
            AssertToken(tokenSet.Last(), string.Empty, TokenType.EndOfFile, -1);
        }
    }
}