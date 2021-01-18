// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

// ReSharper disable All
// ReSharper disable All
// ReSharper disable All
namespace GraphQL.AspNet.Tests.Lexing
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.CharacterGroupValidation;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Tests.Common;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using NUnit.Framework;

    /// <summary>
    /// Tests of the business logic extractors from <see cref="SourceText"/>.
    /// </summary>
    [TestFixture]
    public class SourceTextLexerExtensionsTests
    {
        [TestCase("|", 0, "|")]
        [TestCase("{", 0, "{")]
        [TestCase("}", 0, "}")]
        [TestCase("[", 0, "[")]
        [TestCase("]", 0, "]")]
        [TestCase(":", 0, ":")]
        [TestCase("$", 0, "$")]
        [TestCase("@", 0, "@")]
        [TestCase("!", 0, "!")]
        [TestCase("...", 0, "...")]
        public void SourceText_NextControlPhrase_ValidStrings(string text, int offSet, string outputText)
        {
            var source = new SourceText(text.AsMemory(), offSet);
            var result = source.NextControlPhrase(out var location);

            Assert.AreEqual(outputText, result.ToString());
            Assert.IsNotNull(location);
        }

        [TestCase("...\n..", 4, 4, 2, 0)]
        [TestCase("....", 3, 3, 1, 3)]
        [TestCase("...", 1, 1, 1, 1)]
        [TestCase("..", 0, 0, 1, 0)]
        public void SourceText_NextControlPhrase_InValidStrings(
            string text,
            int offSet,
            int errorAbsoluteIndex,
            int errorLine,
            int errorLineIndex)
        {
            var source = new SourceText(text.AsMemory(), offSet);
            try
            {
                var result = source.NextControlPhrase(out var location);
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

        [TestCase(false, "asdf\"\"\"abadsad123#@!@#.{}c123\"\"\"asdbob", 4, "\"\"\"abadsad123#@!@#.{}c123\"\"\"", 28, 32)] // terminated block string
        [TestCase(false, "\"\"abc123\"bob", 0, "\"\"", 2, 2)] // starting with an empty string
        [TestCase(false, "\"abc123\"bob", 0, "\"abc123\"", 8, 8)] // regular string
        [TestCase(false, "\"abc1\\\"23\"bob", 0, "\"abc1\\\"23\"", 10, 10)] // regular string with escaped quote
        [TestCase(false, @"""bo\r\tb""", 0, @"""bo\r\tb""", 9, 9)] // contains escaped chars
        [TestCase(false, @"""bo\u123Aqeqwe""", 0, @"""bo\u123Aqeqwe""", 15, 15)] // contains unicode escaped char '\u123'
        [TestCase(false, @"""bo\u123Aq\r\t\\\fe\u12Q\u1\u1F\u1f\ue\n\r\u34 qwe""", 0, @"""bo\u123Aq\r\t\\\fe\u12Q\u1\u1F\u1f\ue\n\r\u34 qwe""", 51, 51)] // contains unicode escaped char '\u123'
        [TestCase(true, "LongStringNoExtra.txt", 0, null, 325, 325)]
        [TestCase(true, "LongStringWithExtra.txt", 0, null, 313, 313)]
        [TestCase(true, "LongStringFromOffSet.txt", 6, null, 313, 319)]
        public void SourceText_NextString_ValidStrings(
            bool isFileReference,
            string text,
            int offset,
            string expectedResult,
            int expectedResultLength,
            int expectedCursorLocation)
        {
            SourceText source;
            if (!isFileReference)
            {
                source = new SourceText(text.AsMemory(), offset);
            }
            else
            {
                try
                {
                    var sourceText = ResourceLoader.ReadAllLines("LexerSource_NextString_Tests", text);
                    source = new SourceText(sourceText.AsMemory(), offset);

                    // account for a windows clone that may put in carriage returns
                    // cursor pointers would be after the carriage returns in any files
                    expectedResultLength += sourceText.Count(x => x == '\r');
                    expectedCursorLocation += sourceText.Count(x => x == '\r');
                }
                catch (Exception ex)
                {
                    Assert.Warn($"Unable to load the test file '{text}', test could not performed. Exception: {ex.Message}");
                    return;
                }
            }

            var result = source.NextString(out var location);

            Assert.IsNotNull(location);
            Assert.AreEqual(expectedCursorLocation, source.Cursor);
            Assert.AreEqual(expectedResultLength, result.Length);

            // Note: expectedResult is ignored if null.
            if (expectedResult != null)
                Assert.AreEqual(expectedResult, result.ToString());
        }

        [TestCase("", 0, -1, -1, -1)] // nothing
        [TestCase("\"\"\"", 0, 0, 1, 0)] // only an opening block delimiter
        [TestCase("\"\"\"\"", 0, 0, 1, 0)] // mismatched delimiters BLOCK and SINGLE
        [TestCase("\"", 0, 0, 1, 0)] // not long enough
        [TestCase("a\nab\"abc123423", 0, 0, 1, 0)] // no opening delimiter
        [TestCase("a\nab\"abc123423", 4,  4, 2, 2)] // unterminated from a line
        [TestCase("\"abc123423", 0, 0, 1, 0)] // unterminated
        [TestCase(@"""bo\w\tb""", 0, 3, 1, 3)] // contains escaped char '\w'
        [TestCase(@"""bo\uQ23Aqeqwe""", 0, 3, 1, 3)] // contains invalid unicode escaped char '\uQ'
        [TestCase(@"""bo\U012WAqeqwe""", 0, 3, 1, 3)] // contains invalid unicode escaped char '\U012'
        [TestCase(@"""bo\u123Aq\r\t\\\\\qfe\u12Q\u1\u1F\u1f\ue\n\r\u34 qwe""", 0, 18, 1, 18)]
        public void SourceText_NextString_InvalidString(
            string text,
            int offset,
            int errorAbsoluteIndex,
            int errorLine,
            int errorLineIndex)
        {
            // an invalid string will be one with no terminator
            try
            {
                var source = new SourceText(text.AsMemory(), offset);
                var result = source.NextString(out var location);
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

        private static readonly object[] COMMENT_TEST_CASES =
        {
            new object[]
            {
                "#", 0, "#", 1, string.Empty,
            },
            new object[]
            {
                "123#Comment1\n#Comment2\r\nNotAComment",
                3,
                "#Comment1",
                13,
                "#Comment2\r\nNotAComment",
            },
            new object[]
            {
                "123#HelloThere\nNewText",
                3,
                "#HelloThere",
                15,
                "NewText",
            },
            new object[]
            {
                "    # Queries can have comments!\n                       friends",
                4,
                "# Queries can have comments!",
                33,
                "                       friends",
            },
        };

        [Test]
        [TestCaseSource(nameof(COMMENT_TEST_CASES))]
        public void SourceText_NextComment(string text, int offSet, string expectedOutput, int cursorLoc, string remaining = null)
        {
            var source = new SourceText(text.AsMemory(), offSet);
            var comment = source.NextComment(out var location).ToString();
            Assert.AreEqual(expectedOutput, comment);
            Assert.AreEqual(cursorLoc, source.Cursor);
            Assert.IsNotNull(location);

            if (remaining != null)
                Assert.AreEqual(remaining, source.ToString());
        }

        [TestCase("")]
        [TestCase("abc123john")]
        [TestCase("#abc123\njohn")]
        public void SourceText_NextComment_InvalidString(string text)
        {
            // direct test of error conditions against hte validator as the source extractor
            // should disallow all conditions by the nature of extracting the next line
            try
            {
                var source = new SourceText(text.AsMemory());
                var location = source.RetrieveCurrentLocation();
                var result = source.Next(source.Length);
                CommentPhraseValidator.Instance.ValidateOrThrow(source, result, location);
            }
            catch (GraphQLSyntaxException ex)
            {
                Assert.IsNotNull(ex.Location);
                return;
            }
            catch (Exception e)
            {
                Assert.Fail($"Expected a {typeof(GraphQLSyntaxException).Name} to be thrown but recieved {e.GetType().Name}.");
                return;
            }

            Assert.Fail($"Expected a {typeof(GraphQLSyntaxException).Name} to be thrown but no throw occured.");
        }

        [TestCase("abc", 0, "abc", 3, "")]
        [TestCase("Bbc", 0, "Bbc", 3, "")]
        [TestCase("aBc", 0, "aBc", 3, "")]
        [TestCase("a1c", 0, "a1c", 3, "")]
        [TestCase("A_b9,", 0, "A_b9", 4, ",")]
        [TestCase("_abc,", 0, "_abc", 4, ",")]
        [TestCase("_a_b_c_1_2,", 0, "_a_b_c_1_2", 10, ",")]
        [TestCase("(_abc: \"Bob\")", 1, "_abc", 5, ": \"Bob\")")]
        public void SourceText_NextName_ValidNames(
            string text,              // the source text
            int offSet,               // offset within the source text to start reading
            string expectedOut,       // the expected output of the "name" if any
            int expectedCursorLoc,    // the new expected location of the cursor
            string expectedRemaining) // the remaining text on the source buffer
        {
            var source = new SourceText(text.AsMemory(), offSet);
            var result = source.NextName(out var location);

            // location should point to the start of the extraction
            Assert.IsNotNull(location);
            Assert.AreEqual(offSet, location.AbsoluteIndex);

            Assert.AreEqual(expectedOut, result.ToString());
            Assert.AreEqual(expectedCursorLoc, source.Cursor);
            Assert.AreEqual(expectedRemaining, source.ToString());
        }

        [TestCase("", 0, -1, -1, -1)]
        [TestCase("2abc", 0, 0, 1, 0)]
        [TestCase("a!bc", 0, 1, 1, 1)]
        [TestCase("abc\nabd!bc", 4, 7, 2, 3)]
        public void SourceText_NextName_InvalidNames(
        string text,
        int offset,
        int errorAbsoluteIndex,
        int errorLine,
        int errorLineIndex)
        {
            try
            {
                // query against the validation directly
                // to force some exceptions that would not occur due to extraction delimiters
                var source = new SourceText(text.AsMemory(), offset);
                var location = source.RetrieveCurrentLocation();
                var slice = source.Next(text.Length - offset);
                NameValidator.Instance.ValidateOrThrow(source, slice, location);
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

        [TestCase("12.345abc", 0, "12.345", 6, "abc")]
        [TestCase("12.34", 0, "12.34", 5, "")]
        [TestCase("12.34e5", 0, "12.34e5", 7, "")]
        [TestCase("12.34,123e5", 0, "12.34", 5, ",123e5")]
        [TestCase("12.34E5", 0, "12.34E5", 7, "")]
        [TestCase("12345", 2, "345", 5, "")]
        [TestCase("-12345", 0, "-12345", 6, "")]
        [TestCase("-12345.2", 0, "-12345.2", 8, "")]
        [TestCase("-12345.2e5", 0, "-12345.2e5", 10, "")]
        [TestCase("-12345e5", 0, "-12345e5", 8, "")]
        [TestCase("12345def", 2, "345", 5, "def")]
        [TestCase("12345e-3def", 2, "345e-3", 8, "def")]
        [TestCase("12345def", 0, "12345", 5, "def")]
        [TestCase("abc12345def", 3, "12345", 8, "def")]
        public void SourceText_NextNumber_ValidNumbers(
            string text,              // the source text
            int offSet,               // offset within the source text to start reading
            string expectedOut,       // the expected output of the "number" if any
            int expectedCursorLoc,    // the new expected location of the cursor
            string expectedRemaining) // the remaining text on the source buffer
        {
            var source = new SourceText(text.AsMemory(), offSet);
            var result = source.NextNumber(out var location);

            // location should point to the start of the extraction
            Assert.IsNotNull(location);
            Assert.AreEqual(offSet, location.AbsoluteIndex);

            Assert.AreEqual(expectedOut, result.ToString());
            Assert.AreEqual(expectedCursorLoc, source.Cursor);
            Assert.AreEqual(expectedRemaining, source.ToString());
        }

        [TestCase(".123456", 0, 1, 0)]
        [TestCase("12.3.45", 4, 1, 4)]
        [TestCase("12..45", 3, 1, 3)]
        [TestCase("12.e45", 3, 1, 3)]
        [TestCase("12ee45", 3, 1, 3)]
        [TestCase("12eE45", 3, 1, 3)]
        [TestCase("12Ee45", 3, 1, 3)]
        [TestCase("12EE45", 3, 1, 3)]
        [TestCase("12.3e4E5", 6, 1, 6)]
        [TestCase("12.3E4e5", 6, 1, 6)]
        [TestCase("12.3E4E5", 6, 1, 6)]
        [TestCase("12.3e4e5", 6, 1, 6)]
        [TestCase("12.e3e5", 3, 1, 3)]
        [TestCase("123456.", 6, 1, 6)]
        [TestCase("12.345e", 6, 1, 6)]
        [TestCase("abc12345", 0, 1, 0)] // doesn't point at a number
        public void SourceText_NextNumber_InvalidNumbers(
            string text,
            int errorAbsoluteIndex,
            int errorLine,
            int errorLineIndex)
        {
            try
            {
                var source = new SourceText(text.AsMemory());
                var result = source.NextNumber(out var location);
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
    }
}