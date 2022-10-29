// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Parsing
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using NUnit.Framework;

    /// <summary>
    /// Tests to validate the tokenization of a string.
    /// </summary>
    [TestFixture]
    public class LexerTests
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

            var source = new SourceText(qualifiedQuery.AsMemory());
            var tokenSet = Lexer.Tokenize(source);

            // first two tokens should be control parens
            HelperAsserts.AssertTokenChain(
                tokenSet,
                new LexToken(TokenType.Name, "mutation".AsMemory(), SourceLocation.None),
                new LexToken(TokenType.CurlyBraceLeft, "{".AsMemory(), SourceLocation.None),
                new LexToken(TokenType.Name, "createHero".AsMemory(), SourceLocation.None),
                new LexToken(TokenType.ParenLeft, "(".AsMemory(), SourceLocation.None),
                new LexToken(TokenType.Name, "name".AsMemory(), SourceLocation.None),
                new LexToken(TokenType.Colon, ":".AsMemory(), SourceLocation.None),
                new LexToken(TokenType.String, "\"John\"".AsMemory(), SourceLocation.None),
                new LexToken(TokenType.Comma, ",".AsMemory(), SourceLocation.None, true),
                new LexToken(TokenType.Name, "age".AsMemory(), SourceLocation.None),
                new LexToken(TokenType.Colon, ":".AsMemory(), SourceLocation.None),
                new LexToken(TokenType.Integer, "23".AsMemory(), SourceLocation.None),
                new LexToken(TokenType.ParenRight, ")".AsMemory(), SourceLocation.None),
                new LexToken(TokenType.CurlyBraceLeft, "{".AsMemory(), SourceLocation.None),
                new LexToken(TokenType.Name, "name".AsMemory(), SourceLocation.None),
                new LexToken(TokenType.Comment, "# Queries can have comments!".AsMemory(), SourceLocation.None, true),
                new LexToken(TokenType.Name, "friends".AsMemory(), SourceLocation.None),
                new LexToken(TokenType.CurlyBraceLeft, "{".AsMemory(), SourceLocation.None),
                new LexToken(TokenType.Name, "name".AsMemory(), SourceLocation.None),
                new LexToken(TokenType.CurlyBraceRight, "}".AsMemory(), SourceLocation.None),
                new LexToken(TokenType.CurlyBraceRight, "}".AsMemory(), SourceLocation.None),
                new LexToken(TokenType.CurlyBraceRight, "}".AsMemory(), SourceLocation.None),
                new LexToken(TokenType.EndOfFile));
        }

        [Test]
        public void Lexer_Tokenize_KitchenSink_ParsesExpectedly()
        {
            var text = ResourceLoader.ReadAllLines("KitchenSink", "KitchenSink.graphql");
            var source = new SourceText(text.AsMemory());
            var tokenSet = Lexer.Tokenize(source);
            Assert.AreEqual(191, tokenSet.Count);
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
                var source = new SourceText(qualifiedQuery.AsMemory());
                Lexer.Tokenize(source);
            }
            catch (GraphQLSyntaxException ex)
            {
                Assert.AreEqual(
                    @"                                  createHero(  name: ""John"", age: 2ee.43  ) {",
                    ex.Location.LineText.ToString());
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
                var source = new SourceText(qualifiedQuery.AsMemory());
                Lexer.Tokenize(source);
            }
            catch (GraphQLSyntaxException ex)
            {
                Assert.AreEqual(
                    @"                                  createHero(  name: ""J\wohn"", age: 2ee.43  ) {",
                    ex.Location.LineText.ToString());
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
                var source = new SourceText(qualifiedQuery.AsMemory());
                Lexer.Tokenize(source);
            }
            catch (GraphQLSyntaxException ex)
            {
                Assert.AreEqual(@"                                    na&me", ex.Location.LineText.ToString());
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
            var source = new SourceText(text.AsMemory());

            var tokenSet = Lexer.Tokenize(source);
            var tokenList = tokenSet.ToList();

            // first two tokens should be control parens
            Assert.AreEqual(9, tokenList.Count);
            HelperAsserts.AssertToken(tokenList[0], "(", TokenType.ParenLeft, 0);
            HelperAsserts.AssertToken(tokenList[1], ")", TokenType.ParenRight, 1);
            HelperAsserts.AssertToken(tokenList[2], "\"abc123\"", TokenType.String, 2);
            HelperAsserts.AssertToken(tokenList[3], "ab_2C", TokenType.Name, 12);
            HelperAsserts.AssertToken(tokenList[4], "...", TokenType.SpreadOperator, 18);
            HelperAsserts.AssertToken(tokenList[5], "{", TokenType.CurlyBraceLeft, 21);
            HelperAsserts.AssertToken(tokenList[6], "123", TokenType.Integer, 22);
            HelperAsserts.AssertToken(tokenList[7], "}", TokenType.CurlyBraceRight, 25);
            HelperAsserts.AssertEndsWithEoF(tokenList);
        }

        [TestCase("...", TokenType.SpreadOperator)]
        [TestCase("|", TokenType.Pipe)]
        [TestCase("{", TokenType.CurlyBraceLeft)]
        [TestCase("}", TokenType.CurlyBraceRight)]
        [TestCase("[", TokenType.BracketLeft)]
        [TestCase("]", TokenType.BracketRight)]
        [TestCase(":",  TokenType.Colon)]
        [TestCase("$",  TokenType.Dollar)]
        [TestCase("@",  TokenType.AtSymbol)]
        [TestCase("!",  TokenType.Bang)]
        [TestCase("tes3t",  TokenType.Name)]
        [TestCase("#t123",  TokenType.Comment)]
        [TestCase("\"test\"",  TokenType.String)]
        [TestCase("\"\"\"test\"\"\"",  TokenType.String)]
        [TestCase("1234", TokenType.Integer)]
        [TestCase("1E341234",  TokenType.Float)]
        [TestCase("123455.8",  TokenType.Float)]
        [TestCase("123.4556",  TokenType.Float)]
        [TestCase("123.45e6",  TokenType.Float)]
        public void Lexer_Tokenize_SingleToken_ValidStringReturnsCorrectTokenType(
            string text,
            TokenType tokenType)
        {
            var source = new SourceText(text.AsMemory());

            var tokenSet = Lexer.Tokenize(source);
            var tokenList = tokenSet.ToList();

            // first two tokens should be control parens
            Assert.AreEqual(2, tokenList.Count);
            HelperAsserts.AssertToken(tokenList[0], text, tokenType, 0);
            HelperAsserts.AssertEndsWithEoF(tokenList);
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
                var source = new SourceText(text.AsMemory());
                Lexer.Tokenize(source);
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
        public void Lexer_Tokenize_LongValidDocumentYieldsNoErrors()
        {
            var sourceText = ResourceLoader.ReadAllLines("Lexer_Tokenizing", "SemiLongValidDocument.graphql");
            var source = new SourceText(sourceText.AsMemory());
            var tokenSet = Lexer.Tokenize(source);
        }
    }
}