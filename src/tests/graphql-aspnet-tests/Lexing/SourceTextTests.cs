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
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing.Lexing.Source.SourceRules;
    using GraphQL.AspNet.Tests.Lexing.Helpers;
    using NUnit.Framework;

    [TestFixture]
    public class SourceTextTests
    {
        [TestCase("123456", 2, 5, SourceTextPosition.FromCurrentCursor, false, 2)]
        [TestCase("123456", 2, 7, SourceTextPosition.FromStart, false, 2)]
        [TestCase("123456", 2, 2, SourceTextPosition.FromCurrentCursor, true, 4)]
        [TestCase("123456", 5, 2, SourceTextPosition.FromCurrentCursor, false, 5)]
        [TestCase("123456", 5, 2, SourceTextPosition.FromStart, true, 2)]
        [TestCase("123456", 0, 2, SourceTextPosition.FromStart, true, 2)]
        public void SourceText_Seek(
            string text,
            int initialIndex,
            int lengthToSeek,
            SourceTextPosition from,
            bool shouldExecuteCorrectly,
            int newCursorLocation)
        {
            if (!shouldExecuteCorrectly)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var failureSource = new SourceText(text.AsMemory(), initialIndex);
                    failureSource.Seek(lengthToSeek, from);
                });
            }
            else
            {
                var source = new SourceText(text.AsMemory(), initialIndex);
                source.Seek(lengthToSeek, from);
                Assert.AreEqual(newCursorLocation, source.Cursor);
            }
        }

        [TestCase("12345", 0, 0)]
        [TestCase("12345", 3, 3)]
        [TestCase("", 3, 0)]
        public void SourceText_Constructor_ValidValues(string text, int offset, int expectedCursorPosition)
        {
            var source = new SourceText(text.AsMemory(), offset);
            Assert.AreEqual(expectedCursorPosition, source.Cursor);
        }

        [TestCase("12345", 7)]
        [TestCase("12345", -1)]
        public void SourceText_Constructor_InvalidValues(string text, int offset)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SourceText(text.AsMemory(), offset));
        }

        [TestCase("123", 0, 0, true)]
        [TestCase("", 0, 0, false)]
        public void SourceText_Cursor(string text, int offset, int expectedCursorIndex, bool hasData)
        {
            var source = new SourceText(text.AsMemory(), offset);
            Assert.AreEqual(expectedCursorIndex, source.Cursor);
            Assert.AreEqual(!hasData, source.EndOfFile);
            Assert.AreEqual(hasData, source.HasData);
        }

        [TestCase("123", 0, 3)]
        [TestCase("", 0, 0)]
        public void SourceText_Length(string text, int offset, int expectedLength)
        {
            var source = new SourceText(text.AsMemory(), offset);
            Assert.AreEqual(expectedLength, source.Length);
        }

        [TestCase("", 0, "")]
        [TestCase("123", 0, "123")]
        [TestCase("123", 2, "3")]
        public void SourceText_ToString_WithNoData_ReturnsEmptyString(string text, int offset, string expectedOutput)
        {
            var source = new SourceText(text.AsMemory(), offset);
            Assert.AreEqual(expectedOutput, source.ToString());
        }

        [Test]
        public void SourceText_Peek_AtEndOfText_ReturnsEmpty()
        {
            var source = new SourceText("123".AsMemory());

            while (source.HasData)
                source.Next();

            Assert.IsTrue(ReadOnlySpan<char>.Empty == source.Peek(3));
        }

        [TestCase("12345678", 0, 6, "123456")]
        [TestCase("12345", 0, 6, "12345")]
        [TestCase("12345", 3, 6, "45")]
        [TestCase("123", 0, 3, "123")]
        [TestCase("12345", 3, -3, "123")]
        [TestCase("12345", 1, -1, "1")]
        [TestCase("12345", 1, 0, "")]
        [TestCase("12345", 0, -1, "")]
        [TestCase("12345", 0, -50, "")]
        public void SourceText_Peek(string text, int offset, int count, string expectedText)
        {
            var source = new SourceText(text.AsMemory(), offset);
            var cursor = source.Cursor;
            Assert.AreEqual(expectedText, source.Peek(count).ToString());
            Assert.AreEqual(cursor, source.Cursor);
        }

        [TestCase("12345", 0, "")]
        [TestCase("12345", 1, "")]
        [TestCase("12345", 500, "")]
        [TestCase("12345", -1, "5")]
        [TestCase("12345", -4, "2345")]
        [TestCase("12345", -5, "12345")]
        [TestCase("12345", -6, "12345")]
        [TestCase("12345", -500, "12345")]
        public void SourceText_Peek_FromEndOfBlock(string text,  int count, string expectedText)
        {
            // seek back two from the '3' => '12'
            var source = new SourceText(text.AsMemory());
            source.Seek(source.Length, SourceTextPosition.FromStart);
            Assert.AreEqual(expectedText, source.Peek(count).ToString());
        }

        [TestCase("", 0, '\0')]
        [TestCase("12345", 0, '1')]
        [TestCase("12\xFEFF345", 3, '3')]
        [TestCase("12345", 4, '5')]
        [TestCase("12\xFEFF345", 2, '\xFEFF')]
        [TestCase("12345", 3, '4')]
        public void SourceText_Peek1(string text, int offSet, char expectedText)
        {
            var source = new SourceText(text.AsMemory(), offSet);
            Assert.AreEqual(expectedText, source.Peek());
        }

        [TestCase("", 0, false)]
        [TestCase("1234", 0, true)]
        [TestCase("1234", 2, true)]
        public void SourceText_HasData_WithDataRemaining_ReturnsTrue(string text, int offSet, bool expectedOutput)
        {
            var source = new SourceText(text.AsMemory(), offSet);
            Assert.AreEqual(expectedOutput, source.HasData);
        }

        [TestCase("12345678", 0, 0, "", 0)]
        [TestCase("12345678", 0, 3, "123", 3)]
        [TestCase("1234", 0, 9, "1234", 4)]
        [TestCase("12345678", 0, -1, "123", 3)]
        public void SourceText_Next(string text, int offset, int count, string expectedText, int expectedCursorIndex)
        {
            if (count >= 0)
            {
                var source = new SourceText(text.AsMemory(), offset);
                var next = source.Next(count);
                Assert.AreEqual(expectedText, next.ToString());
                Assert.AreEqual(expectedCursorIndex, source.Cursor);
            }
            else
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var source = new SourceText(text.AsMemory(), offset);
                    var next = source.Next(count);
                });
            }
        }

        [Test]
        public void SourceText_Next_ContinuesToReturnDataTillEmpty()
        {
            var text = "12345";
            var source = new SourceText(text.AsMemory());
            var i = 0;
            while (source.HasData)
            {
                if (i > 5)
                {
                    Assert.Fail("Source Text did not properly signal it had emptied its data queue at the appropriate time");
                }

                var data = source.Next();
                Assert.AreEqual(1, data.Length);
                Assert.AreEqual(text[i], data[0]);
                i++;
            }

            // esnure 5 iterations occured
            Assert.AreEqual(5, i);
            Assert.AreEqual(5, source.Cursor);
        }

        [TestCase("123", 0, false)]
        [TestCase(" 123", 0, true)]
        [TestCase("1 23", 1, true)]
        public void SourceText_PeekNextIsWhiteSpace(string text, int offset, bool expectedValue)
        {
            var source = new SourceText(text.AsMemory(), offset);
            Assert.AreEqual(expectedValue, source.PeekNextIsWhitespace());
        }

        [TestCase("", 0, "")]
        [TestCase("\n\n", 1, "")] // no text on the line
        [TestCase("\n\r\n", 1, "")] // no text on the line
        [TestCase("\n\r\n", 2, "")] // no text on the line
        [TestCase("\n\xFEFF\r\n", 2, "\xFEFF")] // BOM char
        [TestCase("\n   \r\n", 3, "   ")] // does not truncate
        [TestCase("12345", -1, "12345")]
        [TestCase("12345", 0, "12345")]
        [TestCase("12345", 1, "12345")]
        [TestCase("12345", 2, "12345")]
        [TestCase("12345", 3, "12345")]
        [TestCase("12345", 4, "12345")]
        [TestCase("12345", 5, "")]
        [TestCase("12345\r\n567", 6, "12345")]
        [TestCase("12345\r\n567", 3, "12345")]
        [TestCase("12345\n567", 6, "567")]
        [TestCase("12345\n567\n890", 6, "567")]
        [TestCase("12345\n567\n890", 7, "567")]
        [TestCase("12345\n567\n890", 8, "567")]
        [TestCase("12345\n567\n890", 13, "")]// at the end of hte string
        [TestCase("12345\n567\n890", 14, "")] // beyond the end of hte string
        public void SourceText_PeekFullLine(string text, int offset, string expectedLineText)
        {
            var source = new SourceText(text.AsMemory());
            Assert.AreEqual(expectedLineText, source.PeekFullLine(offset).ToString());
        }

        [TestCase("123", 0, false)]
        [TestCase("\n123", 0, true)]
        [TestCase("1\n23", 0, false)]
        [TestCase("1\n23", 1, true)]
        [TestCase("1\n23", 2, false)]
        [TestCase("\r\n23", 0, true)]
        [TestCase("\r23", 0, false)]
        public void SourceText_PeekNextIsLineTerminator(string text, int offset, bool isLineTerminator)
        {
            var source = new SourceText(text.AsMemory(), offset);
            Assert.AreEqual(isLineTerminator, source.PeekNextIsLineTerminator());
        }

        [TestCase("123\r\n456", 0, "123")]
        [TestCase("123\n456", 0, "123")]
        [TestCase("123\n456", 4, "456")]
        [TestCase("123\n456", 3, "")]
        [TestCase("123456", 0, "123456")]
        [TestCase("", 0, "")]
        public void SourceText_PeekLine(string text, int offset, string expectedResult)
        {
            var source = new SourceText(text.AsMemory(), offset);
            var line = source.PeekLine();
            Assert.AreEqual(expectedResult, line.ToString());
        }

        [TestCase("", 0, 0, "")]
        [TestCase("1\n\t 567", 1, 3, "567")]
        [TestCase("12345656", 0, 0, "12345656")]
        [TestCase("  \n\t\r  \xFEFF  dsdf", 0, 10, "dsdf")] // byte order mark
        public void SourceText_SkipWhiteSpace(string text, int offset, int expectedSkippedChars, string remainingChars)
        {
            var source = new SourceText(text.AsMemory(), offset);
            Assert.AreEqual(expectedSkippedChars, source.SkipWhitespace());
            Assert.AreEqual(remainingChars, source.ToString());
        }

        [TestCase("1234\n5678", 0, 1, 0, 0)]
        [TestCase("1234\n5678", 6, 2, 1, 6)]
        [TestCase("1234", 0, 1, 0, 0)]
        [TestCase("", 0, -1, -1, -1)]
        public void SourceText_RetrieveCurrentLocation(string text, int offset, int expectedLineNumber, int expectedLineIndex, int expectedAbsoluteIndex)
        {
            var source = new SourceText(text.AsMemory(), offset);
            var line = source.RetrieveCurrentLocation();

            Assert.IsNotNull(line);
            Assert.AreEqual(expectedLineNumber, line.LineNumber);
            Assert.AreEqual(expectedLineIndex, line.LineIndex);
            Assert.AreEqual(expectedAbsoluteIndex, line.AbsoluteIndex);
        }

        [TestCase("1234\n", 0, "1234", "")]
        [TestCase("\n1234567", 0, "", "1234567")]
        [TestCase("1234\r\n567", 0, "1234", "567")]
        [TestCase("1234\n567", 0, "1234", "567")]
        [TestCase("1234567", 0, "1234567", "")]
        [TestCase("", 0, "", "")]
        public void SourceText_NextLine(string text, int offset, string expectedText, string expectedRemainingText)
        {
            var source = new SourceText(text.AsMemory(), offset);
            Assert.AreEqual(expectedText, source.NextLine().ToString());
            Assert.AreEqual(expectedRemainingText, source.ToString());
        }

        [Test]
        public void SourceText_NextLine_WithMultipleLineDataEndingInLineTerminator_SegmentsCorrectly()
        {
            var source = new SourceText("1234\r\n567\n8910\n111213\n".AsMemory());
            Assert.AreEqual("1234", source.NextLine().ToString());
            Assert.AreEqual("567", source.NextLine().ToString());
            Assert.AreEqual("8910", source.NextLine().ToString());
            Assert.AreEqual("111213", source.NextLine().ToString());
            Assert.IsTrue(!source.HasData);
        }

        [TestCase("", 0, -1)]
        [TestCase("1234567", 0, 1)]
        [TestCase("1234567", 4, 1)]
        [TestCase("1234\r\n567\n8910\n111213", 8, 2)]
        [TestCase("1234\r\n567\n8910\n111213", 12, 3)]
        [TestCase("1234\r\n567\n8910\n111213", 19, 4)]
        public void SourceText_LineNumber(string text, int offset, int expectedLineNumber)
        {
            var source = new SourceText(text.AsMemory(), offset);
            Assert.AreEqual(expectedLineNumber, source.CurrentLineNumber);
        }

        [TestCase("# Some Text", 0, GraphQLSourceRule.IsCommentGlyph, true)]
        [TestCase("# So#me Text", 3, GraphQLSourceRule.IsCommentGlyph, false)]
        [TestCase("# So#me Text", 4, GraphQLSourceRule.IsCommentGlyph, true)]
        [TestCase("# So#me Text", 5, GraphQLSourceRule.IsCommentGlyph, false)]
        [TestCase("Some Other Text", 0, GraphQLSourceRule.IsCommentGlyph, false)]
        [TestCase("Some Other Text", 5, GraphQLSourceRule.IsCommentGlyph, false)]
        [TestCase("123...", 3, GraphQLSourceRule.IsControlGlyph, true)]
        [TestCase("123...", 2, GraphQLSourceRule.IsControlGlyph, false)]
        [TestCase("...123...", 0, GraphQLSourceRule.IsControlGlyph, true)]
        [TestCase("{", 0, GraphQLSourceRule.IsControlGlyph, true)]
        [TestCase("[", 0, GraphQLSourceRule.IsControlGlyph, true)]
        [TestCase("(", 0, GraphQLSourceRule.IsControlGlyph, true)]
        [TestCase("}", 0, GraphQLSourceRule.IsControlGlyph, true)]
        [TestCase("]", 0, GraphQLSourceRule.IsControlGlyph, true)]
        [TestCase(")", 0, GraphQLSourceRule.IsControlGlyph, true)]
        [TestCase("=", 0, GraphQLSourceRule.IsControlGlyph, true)]
        [TestCase(",", 0, GraphQLSourceRule.IsControlGlyph, true)]
        [TestCase(".", 0, GraphQLSourceRule.IsControlGlyph, true)]
        [TestCase(":", 0, GraphQLSourceRule.IsControlGlyph, true)]
        [TestCase("|", 0, GraphQLSourceRule.IsControlGlyph, true)]
        [TestCase("@", 0, GraphQLSourceRule.IsControlGlyph, true)]
        [TestCase("$", 0, GraphQLSourceRule.IsControlGlyph, true)]
        [TestCase("#", 0, GraphQLSourceRule.IsControlGlyph, true)]
        [TestCase("A", 0, GraphQLSourceRule.IsControlGlyph, false)]
        [TestCase("", 0, GraphQLSourceRule.IsControlGlyph, false)]
        [TestCase(" ", 0, GraphQLSourceRule.IsControlGlyph, false)]
        public void SourceText_CheckCursor(string sourceText, int offSet, GraphQLSourceRule rule, bool expectedValue)
        {
            var source = new SourceText(sourceText.AsMemory(), offSet);
            Assert.AreEqual(expectedValue, source.CheckCursor(rule));
        }

        [TestCase("12345", 4, -7, "1234", false)]
        [TestCase("12345", 4, -2, "34", false)]
        [TestCase("12345", 4, -1, "4", false)]
        [TestCase("12345", 4, 1, "5", false)]
        [TestCase("12345", 0, 6, "12345", false)]
        [TestCase("12345", -1, 3, "", true)]
        [TestCase("12345", 0, 3, "123", false)]
        public void SourceText_Slice(string text, int start, int length, string expectedResult, bool throwException)
        {
            if (throwException)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var throwSource = new SourceText(text.AsMemory());
                    throwSource.Slice(start, length);
                });
            }
            else
            {
                var source = new SourceText(text.AsMemory());
                var result = source.Slice(start, length);

                Assert.AreEqual(result.ToString(), expectedResult);
            }
        }

        [TestCase("123 4567\n987", 4, "4567", 8)]
        [TestCase("123 4567 987", 3, "", 3)]
        [TestCase("123 4567 987", 9, "987", 12)]
        [TestCase("123 4567", 0, "123", 3)]
        [TestCase("1234567", 0, "1234567", 7)]
        public void SourceText_NextCharacterGroup(string text, int start, string expectedResult, int expectedCursorLocation)
        {
            var source = new SourceText(text.AsMemory(), start);
            var nextGroup = source.NextCharacterGroup();

            Assert.AreEqual(expectedResult, nextGroup.ToString());
            Assert.AreEqual(expectedCursorLocation, source.Cursor);
        }

        [TestCase("", 0, "", 0)]
        [TestCase("123a", 2, "3", 3)]
        [TestCase("abc123", 0, "", 0)]
        [TestCase("1234567", 0, "1234567", 7)]
        [TestCase("123a4567", 0, "123", 3)]
        public void SourceText_NextFilter(string text, int start, string expectedOutput, int newCursor)
        {
            var source = new SourceText(text.AsMemory(), start);
            var result = source.NextFilter(char.IsDigit);

            Assert.AreEqual(expectedOutput, result.ToString());
            Assert.AreEqual(newCursor, source.Cursor);
        }

        [Test]
        public void SourceText_NextFilter_FromEndOfLength()
        {
            var source = new SourceText("123".AsMemory());
            var result = source.Next(source.Length);
            Assert.AreEqual("123", result.ToString());
            Assert.AreEqual(3, source.Cursor);

            result = source.NextFilter(char.IsDigit);

            Assert.AreEqual(string.Empty, result.ToString());
            Assert.AreEqual(3, source.Cursor);
        }

        private static readonly object[] NEXT_PHRASE_TEST_CASES =
        {
            // properly delimited
            new object[]
            {
                "'12345'qwer",
                0,
                new SourceNextPhraseDelegate(SourceTextTestPredicates.SingleQuoteDelimited),
                "'12345'",
                7,
            },

            // no end delimiter consumes everything (cursor placed beyond end of text)
            new object[]
            {
                "'12345qwer",
                0,
                new SourceNextPhraseDelegate(SourceTextTestPredicates.SingleQuoteDelimited),
                "'12345qwer",
                10,
            },

            // entire phrase consumes everything up to length
            new object[]
            {
                "'12345qwer'",
                0,
                new SourceNextPhraseDelegate(SourceTextTestPredicates.SingleQuoteDelimited),
                "'12345qwer'",
                11,
            },
        };

        [Test]
        [TestCaseSource(nameof(NEXT_PHRASE_TEST_CASES))]
        public void Source_NextPhrase(
            string text,
            int offSet,
            SourceNextPhraseDelegate predicate,
            string expectedOutput,
            int expectedCursorPosition)
        {
            var source = new SourceText(text.AsMemory(), offSet);
            var result = source.NextPhrase(predicate);

            Assert.AreEqual(expectedOutput, result.ToString());
            Assert.AreEqual(expectedCursorPosition, source.Cursor);
        }

        [TestCase("", 0, -1, -1, -1)]
        [TestCase("12345", 0, 0, 1, 0)]
        [TestCase("12345", 3, 3, 1, 3)]
        [TestCase("12345", 5, 5, 1, 5)]
        [TestCase("12345", 99, 5, 1, 5)]
        [TestCase("12345", -1, 0, 1, 0)] // before the start of the string
        [TestCase("12\n345", 3, 3, 2, 0)] // at the 3
        [TestCase("12\n345", 2, 2, 1, 2)] // between 2 and \n
        [TestCase("12\n\r\n\n\n\n\n\n\n3\n\n\n\n345", 17, 17, 13, 1)]
        public void SourceNext_RetrieveLocationFromPosition(
            string text,
            int atIndex,
            int expectedPosition,
            int expectedLine,
            int expectedPositionInLine)
        {
            var sourceText = new SourceText(text.AsMemory());

            var result = sourceText.RetrieveLocationFromPosition(atIndex);
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedPosition, result.AbsoluteIndex);
            Assert.AreEqual(expectedLine, result.LineNumber);
            Assert.AreEqual(expectedPositionInLine, result.LineIndex);
            Assert.AreEqual(expectedPositionInLine + 1, result.LinePosition);
        }

        [TestCase("12\n345", 1, 4, 5, 2, 2)]
        [TestCase("12\n345", 1, 2, 3, 2, 0)]
        [TestCase("12345", 0, 0, 0, 1, 0)]
        public void SourceNext_OffsetFromPosition(
            string text,
            int startingAtIndex,
            int thenOffset,
            int expectedPosition,
            int expectedLine,
            int expectedPositionInLine)
        {
            var sourceText = new SourceText(text.AsMemory());
            var result = sourceText.RetrieveLocationFromPosition(startingAtIndex);
            Assert.IsNotNull(result);

            result = sourceText.OffsetLocation(result, thenOffset);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedPosition, result.AbsoluteIndex);
            Assert.AreEqual(expectedLine, result.LineNumber);
            Assert.AreEqual(expectedPositionInLine, result.LineIndex);
        }
    }
}