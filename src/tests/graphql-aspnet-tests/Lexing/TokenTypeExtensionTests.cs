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
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
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
    }
}