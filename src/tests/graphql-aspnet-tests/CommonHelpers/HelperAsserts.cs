// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.CommonHelpers
{
    using System;
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
        public static void AssertTokenChain(TokenStream tokenSet, params LexicalToken[] expectedTokens)
        {
            if (tokenSet.Count != expectedTokens.Length)
                Assert.Fail($"Expected {expectedTokens.Length} but received {tokenSet.Count}");

            tokenSet.Prime();
            var i = 0;
            while (tokenSet.Count > 0)
            {
                var expected = expectedTokens[i];
                AssertToken(tokenSet.ActiveToken, expected.GetType(), expected.Text.ToString(), expected.TokenType);
                tokenSet.Next(false);
                i++;
            }
        }

        /// <summary>
        /// Asserts that the token meets common checks.
        /// </summary>
        /// <typeparam name="TExpectedTokenType">The type of the t expected token type.</typeparam>
        /// <param name="token">The token.</param>
        /// <param name="expectedText">The expected text.</param>
        /// <param name="expectedType">The expected type of the 'TokenType' property.</param>
        /// <param name="expectedAbsolutePosition">The expected absolute position.</param>
        public static void AssertToken<TExpectedTokenType>(LexicalToken token, string expectedText, TokenType expectedType, int? expectedAbsolutePosition = null)
        {
            AssertToken(token, typeof(TExpectedTokenType), expectedText, expectedType, expectedAbsolutePosition);
        }

        /// <summary>
        /// Asserts that the token meets common checks.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="expectedClassType">Expected type of the class.</param>
        /// <param name="expectedText">The expected text.</param>
        /// <param name="expectedType">The expected type of the 'TokenType' property.</param>
        /// <param name="expectedAbsolutePosition">The expected absolute position.</param>
        public static void AssertToken(LexicalToken token, Type expectedClassType, string expectedText, TokenType expectedType, int? expectedAbsolutePosition = null)
        {
            Assert.IsNotNull(token);
            Assert.IsInstanceOf(expectedClassType, token);
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
            AssertToken<EndOfFileToken>(tokenSet.Last(), string.Empty, TokenType.EndOfFile, -1);
        }
    }
}