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
    using System;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Tokens;
    using NUnit.Framework;

    [TestFixture]
    public class TokenTypeExtensionTests
    {
        [TestCase("{", TokenType.CurlyBraceLeft)]
        [TestCase("}", TokenType.CurlyBraceRight)]
        [TestCase("!", TokenType.Bang)]
        [TestCase("$", TokenType.Dollar)]
        [TestCase(")", TokenType.ParenRight)]
        [TestCase("(", TokenType.ParenLeft)]
        [TestCase("=", TokenType.EqualsSign)]
        [TestCase(".", TokenType.SpreadOperatorInitiator)]
        [TestCase("...", TokenType.SpreadOperator)]
        [TestCase("not", TokenType.None)] // three chars but not a spread
        [TestCase(":", TokenType.Colon)]
        [TestCase("@", TokenType.AtSymbol)]
        [TestCase("[", TokenType.BracketLeft)]
        [TestCase("]", TokenType.BracketRight)]
        [TestCase("|", TokenType.Pipe)]
        [TestCase("#", TokenType.Comment)]
        [TestCase("", TokenType.None)]
        [TestCase("'", TokenType.None)]
        [TestCase("\"", TokenType.None)]
        [TestCase("q", TokenType.None)] // just not a token
        [TestCase("noToLong", TokenType.None)]
        public void ToTokenType_ValidEntries(string text, TokenType expectedType)
        {
            var span = text.AsSpan();
            var result = span.ToTokenType();
            Assert.AreEqual(expectedType, result);
        }

        [Test]
        public void AllTokenTypesHaveADescription()
        {
            foreach (TokenType token in Enum.GetValues(typeof(TokenType)))
            {
                if (token == TokenType.None)
                    continue;

                var description = token.Description().ToString();
                if (string.IsNullOrWhiteSpace(description))
                {
                    Assert.Fail($"Tokentype '{token.ToString()}' has no defined description");
                }
            }
        }

        [Test]
        public void ToTokenType_EnsureAllAsciiBasedTokenTypesAreIncluded()
        {
            var allValues = Enum.GetValues(typeof(TokenType));
            foreach (TokenType tt in allValues)
            {
                char c = (char)(int)tt;
                if (c >= 32)
                {
                    var result = c.ToTokenType();
                    Assert.AreNotEqual(TokenType.None, result);
                }
            }
        }
    }
}