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
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution.Parsing.Exceptions;
    using GraphQL.AspNet.Execution.Parsing.Lexing;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Execution.Parsing.Helpers;
    using NUnit.Framework;

    /// <summary>
    /// Tests to validate the tokenization of a string.
    /// </summary>
    [TestFixture]
    internal class LexerTests
    {
        [Test]
        public void Lexer_Tokenize_SimpleQuery_ReturnsExpectedTokens()
        {
            var qualifiedQuery = @"mutation {
                                  createHero(  name: ""John"", age: 23  ) {
                                    name
                                    # Queries can have comments!
                                    friends {
                                      name
                                    }
                                  }
                                }";

            var source = new SourceText(qualifiedQuery.AsSpan());
            var tokenSet = Lexer.Tokenize(source);

            // first two tokens should be control parens
            HelperAsserts.AssertTokenChain(
                tokenSet,
                new LexicalTokenTestCase(TokenType.Name, "mutation", SourceLocation.None),
                new LexicalTokenTestCase(TokenType.CurlyBraceLeft, "{", SourceLocation.None),
                new LexicalTokenTestCase(TokenType.Name, "createHero", SourceLocation.None),
                new LexicalTokenTestCase(TokenType.ParenLeft, "(", SourceLocation.None),
                new LexicalTokenTestCase(TokenType.Name, "name", SourceLocation.None),
                new LexicalTokenTestCase(TokenType.Colon, ":", SourceLocation.None),
                new LexicalTokenTestCase(TokenType.String, "\"John\"", SourceLocation.None),
                new LexicalTokenTestCase(TokenType.Comma, ",", SourceLocation.None, true),
                new LexicalTokenTestCase(TokenType.Name, "age", SourceLocation.None),
                new LexicalTokenTestCase(TokenType.Colon, ":", SourceLocation.None),
                new LexicalTokenTestCase(TokenType.Integer, "23", SourceLocation.None),
                new LexicalTokenTestCase(TokenType.ParenRight, ")", SourceLocation.None),
                new LexicalTokenTestCase(TokenType.CurlyBraceLeft, "{", SourceLocation.None),
                new LexicalTokenTestCase(TokenType.Name, "name", SourceLocation.None),
                new LexicalTokenTestCase(TokenType.Comment, "# Queries can have comments!", SourceLocation.None, true),
                new LexicalTokenTestCase(TokenType.Name, "friends", SourceLocation.None),
                new LexicalTokenTestCase(TokenType.CurlyBraceLeft, "{", SourceLocation.None),
                new LexicalTokenTestCase(TokenType.Name, "name", SourceLocation.None),
                new LexicalTokenTestCase(TokenType.CurlyBraceRight, "}", SourceLocation.None),
                new LexicalTokenTestCase(TokenType.CurlyBraceRight, "}", SourceLocation.None),
                new LexicalTokenTestCase(TokenType.CurlyBraceRight, "}", SourceLocation.None),
                new LexicalTokenTestCase(TokenType.EndOfFile));
        }

        [Test]
        public void Lexer_Tokenize_KitchenSink_ParsesExpectedly()
        {
            var text = ResourceLoader.ReadAllLines("KitchenSink", "KitchenSink.graphql");
            var source = new SourceText(text.AsSpan());
            var tokenSet = Lexer.Tokenize(source);
            var list = tokenSet.ToList(false);
            Assert.AreEqual(191, list.Count);
        }

        [Test]
        public void Lexer_Tokenize_SimpleInvalidQuery_ThrowsExpectedException()
        {
            // exception invalid number cant havel double ee
            // expected exception at line 2:68  (the first e in '2ee.43')
            var qualifiedQuery = @"mutation {
                                  createHero(  name: ""John"", age: 2ee.43  ) {
                                    name
                                    # Queries can have comments!
                                    friends {
                                      name
                                    }
                                  }
                                }";

            try
            {
                var source = new SourceText(qualifiedQuery.AsSpan());
                var tokenStream = Lexer.Tokenize(source);
                var allTokens = tokenStream.ToList();
            }
            catch (GraphQLSyntaxException ex)
            {
                Assert.AreEqual(2, ex.Location.LineNumber);
                Assert.AreEqual(68, ex.Location.LineIndex);
                return;
            }

            Assert.Fail($"Expected {typeof(GraphQLSyntaxException).Name} to be thrown but it wasnt");
        }

        [Test]
        public void Lexer_Tokenize_SimpleInvalidEscapedCharQuery_ThrowsExpectedException()
        {
            // exception invalid number cant havel double ee
            // expected exception at line 2:68  (the first e in '2ee.43')
            var qualifiedQuery = @"mutation {
                                  createHero(  name: ""J\wohn"", age: 2ee.43  ) {
                                    name
                                    # Queries can have comments!
                                    friends {
                                      name
                                    }
                                  }
                                }";

            try
            {
                var source = new SourceText(qualifiedQuery.AsSpan());
                var tokenStream = Lexer.Tokenize(source);
                var allTokens = tokenStream.ToList();
            }
            catch (GraphQLSyntaxException ex)
            {
                Assert.AreEqual(2, ex.Location.LineNumber);
                Assert.AreEqual(55, ex.Location.LineIndex);
                return;
            }

            Assert.Fail($"Expected {typeof(GraphQLSyntaxException).Name} to be thrown but it wasnt");
        }

        [Test]
        public void Lexer_Tokenize_SimpleInvalidControlCharQuery_ThrowsExpectedExceptionAndLine()
        {
            // exception invalid number cant havel double ee
            // expected exception at line 3:38  (the '&' in na&me)
            var qualifiedQuery = @"mutation {
                                  createHero(  name: ""John"", age: 2.4e323  ) {
                                    na&me
                                    # Queries can have comments!
                                    friends {
                                      name
                                    }
                                  }
                                }";

            try
            {
                var source = new SourceText(qualifiedQuery.AsSpan());
                var tokenStream = Lexer.Tokenize(source);
                var allTokens = tokenStream.ToList();
            }
            catch (GraphQLSyntaxException ex)
            {
                Assert.AreEqual(3, ex.Location.LineNumber);
                Assert.AreEqual(38, ex.Location.LineIndex);
                Assert.AreEqual(39, ex.Location.LinePosition);
                return;
            }

            Assert.Fail($"Expected {typeof(GraphQLSyntaxException).Name} to be thrown but it wasnt");
        }

        [Test]
        public void Lexer_Tokenize_SimpleString_ReturnsExpectedTokens()
        {
            var text = "()\"abc123\"  ab_2C ...{123}";
            var source = new SourceText(text.AsSpan());

            var tokenSet = Lexer.Tokenize(source);
            var tokenList = tokenSet.ToList();

            // first two tokens should be control parens
            HelperAsserts.AssertTokenChain(
                tokenSet,
                new LexicalTokenTestCase(TokenType.ParenLeft, "("),
                new LexicalTokenTestCase(TokenType.ParenRight, ")"),
                new LexicalTokenTestCase(TokenType.String, "\"abc123\""),
                new LexicalTokenTestCase(TokenType.Name, "ab_2C"),
                new LexicalTokenTestCase(TokenType.SpreadOperator, "..."),
                new LexicalTokenTestCase(TokenType.CurlyBraceLeft, "{"),
                new LexicalTokenTestCase(TokenType.Integer, "123"),
                new LexicalTokenTestCase(TokenType.CurlyBraceRight, "}"),
                new LexicalTokenTestCase(TokenType.EndOfFile));
        }

        [TestCase("...", TokenType.SpreadOperator)]
        [TestCase("|", TokenType.Pipe)]
        [TestCase("{", TokenType.CurlyBraceLeft)]
        [TestCase("}", TokenType.CurlyBraceRight)]
        [TestCase("[", TokenType.BracketLeft)]
        [TestCase("]", TokenType.BracketRight)]
        [TestCase(":", TokenType.Colon)]
        [TestCase("$", TokenType.Dollar)]
        [TestCase("@", TokenType.AtSymbol)]
        [TestCase("!", TokenType.Bang)]
        [TestCase("tes3t", TokenType.Name)]
        [TestCase("#t123", TokenType.Comment)]
        [TestCase("\"test\"", TokenType.String)]
        [TestCase("\"\"\"test\"\"\"", TokenType.String)]
        [TestCase("1234", TokenType.Integer)]
        [TestCase("1E341234", TokenType.Float)]
        [TestCase("123455.8", TokenType.Float)]
        [TestCase("123.4556", TokenType.Float)]
        [TestCase("123.45e6", TokenType.Float)]
        public void Lexer_Tokenize_SingleToken_ValidStringReturnsCorrectTokenType(
            string text,
            TokenType tokenType)
        {
            var source = new SourceText(text.AsSpan());

            var tokenSet = Lexer.Tokenize(source);

            // first two tokens should be control parens
            HelperAsserts.AssertTokenChain(
                tokenSet,
                new LexicalTokenTestCase(tokenType, text),
                new LexicalTokenTestCase(TokenType.EndOfFile));
        }

        [TestCase("()....44", 5, 1, 5)] // interpreted as an invalid spread operator (4th period)
        [TestCase("()\nabc.44", 6, 2, 3)] // interpreted as an invalid spread operator
        [TestCase("()..44", 2, 1, 2)] // interpreted as an invalid spread operator
        [TestCase("123..44", 4, 1, 4)] // interpreted as an invalid number
        public void Lexer_Tokenize_InvalidString_ThrowsException(
            string text,
            int errorAbsoluteIndex,
            int errorLine,
            int errorLineIndex)
        {
            try
            {
                var source = new SourceText(text.AsSpan());
                var stream = Lexer.Tokenize(source);
                var allTokens = stream.ToList();
            }
            catch (GraphQLSyntaxException ex)
            {
                Assert.IsNotNull(ex.Location);
                Assert.AreEqual(errorAbsoluteIndex, ex.Location.AbsoluteIndex);
                Assert.AreEqual(errorLine, ex.Location.LineNumber);
                Assert.AreEqual(errorLineIndex, ex.Location.LineIndex);
                return;
            }
            catch (Exception e)
            {
                Assert.Fail($"Expected a {typeof(GraphQLSyntaxException).Name} to be thrown but recieved {e.GetType().Name}.");
                return;
            }

            Assert.Fail($"Expected a {typeof(GraphQLSyntaxException).Name} to be thrown but no throw occured.");
        }

        [Test]
        public void Lexer_Tokenize_LongValidDocumentYieldsNoExceptions()
        {
            var sourceText = ResourceLoader.ReadAllLines("Lexer_Tokenizing", "SemiLongValidDocument.graphql");
            var source = new SourceText(sourceText.AsSpan());
            var tokenSet = Lexer.Tokenize(source);
            var allTokens = tokenSet.ToList();
        }
    }
}