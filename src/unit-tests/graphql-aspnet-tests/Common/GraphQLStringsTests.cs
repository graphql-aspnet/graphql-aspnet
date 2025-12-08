// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Common
{
    using GraphQL.AspNet.Common;
    using NUnit.Framework;

    [TestFixture]
    public class GraphQLStringsTests
    {
        // Basic unescape tests (existing functionality)
        [TestCase("\"abc\"", "abc")]
        [TestCase("\"\"\"abc\"\"\"", "abc")]
        [TestCase("\"\"\"abc\"\"", "\"\"abc\"")]
        [TestCase("\"abc\\u0245123\"", "abcɅ123")]
        [TestCase("\"a\\nbc\"", "a\nbc")]
        [TestCase("\"a\\n\\r\\u0000\\bbc\"", "a\n\r\0\bbc")]
        [TestCase("\"\\ucd5c\\uc608\\ub098\"", "최예나")]

        // Variable-width Unicode escape tests (new in September 2025 spec)
        [TestCase("\"\\u{61}\"", "a")] // Single ASCII character
        [TestCase("\"\\u{1F600}\"", "😀")] // Emoji (supplementary character)
        [TestCase("\"\\u{10FFFF}\"", "\U0010FFFF")] // Maximum valid Unicode
        [TestCase("\"Hello\\u{1F600}World\"", "Hello😀World")] // Emoji in middle of string
        [TestCase("\"\\u{0}\"", "\0")] // Null character
        [TestCase("\"\\u{E000}\"", "\uE000")] // First value after surrogate range

        // Surrogate pair tests (legacy format)
        [TestCase("\"\\uD83D\\uDE00\"", "😀")] // Surrogate pair for emoji
        [TestCase("\"\\uD800\\uDC00\"", "\U00010000")] // First supplementary character

        // Mixed escape formats
        [TestCase("\"\\u0041\\u{42}C\"", "ABC")] // Mix of fixed and variable-width
        [TestCase("\"\\n\\t\\r\"", "\n\t\r")] // Standard escapes
        [TestCase("\"\\\"Quote\\\"\"", "\"Quote\"")] // Escaped quotes
        [TestCase("\"\\\\/\"", "\\/")] // Escaped slash
        [TestCase("\"\\\\\"", "\\")] // Escaped backslash
        public void UnescapeAndTrimDelimiters_ValidInput_ReturnsExpectedOutput(string inputText, string expectedOutput)
        {
            var result = GraphQLStrings.UnescapeAndTrimDelimiters(inputText);

            Assert.AreEqual(
                expectedOutput,
                result,
                $"Expected '{expectedOutput}' but got '{result}'");
        }

        // Invalid escape sequence tests (should return null per spec)
        [TestCase("\"\\uD800\"")] // Unpaired high surrogate
        [TestCase("\"\\uDC00\"")] // Unpaired low surrogate
        [TestCase("\"\\uDEAD\"")] // Unpaired surrogate
        [TestCase("\"\\u{D800}\"")] // High surrogate in variable-width
        [TestCase("\"\\u{DFFF}\"")] // Low surrogate in variable-width
        [TestCase("\"\\u{110000}\"")] // Beyond max Unicode value
        [TestCase("\"\\u123\"")] // Too few hex digits (fixed-width)
        [TestCase("\"\\u{G}\"")] // Invalid hex digit
        [TestCase("\"\\u{\"")] // Unterminated variable-width
        [TestCase("\"\\u{123\"")] // Missing closing brace
        [TestCase("\"\\uD83D\"")] // High surrogate without low surrogate
        [TestCase("\"\\uD83D\\u0041\"")] // High surrogate followed by non-surrogate
        [TestCase("\"\\x41\"")] // Invalid escape sequence
        public void UnescapeAndTrimDelimiters_InvalidInput_ReturnsNull(string inputText)
        {
            var result = GraphQLStrings.UnescapeAndTrimDelimiters(inputText);

            Assert.IsNull(
                result,
                $"Expected null for invalid input '{inputText}' but got '{result}'");
        }

        // Escape method tests
        [TestCase("abc", "abc")] // Basic ASCII
        [TestCase("a\"b", "a\\\"b")] // Escaped quote
        [TestCase("a\\b", "a\\\\b")] // Escaped backslash
        [TestCase("\n", "\\n")] // Newline
        [TestCase("\r", "\\r")] // Carriage return
        [TestCase("\t", "\\t")] // Tab
        [TestCase("\b", "\\b")] // Backspace
        [TestCase("\f", "\\f")] // Form feed
        [TestCase("\u0001", "\\u0001")] // Control character
        [TestCase("\u001F", "\\u001f")] // Max control char (first range)
        [TestCase("\u007F", "\\u007f")] // DEL control character
        [TestCase("\u009F", "\\u009f")] // Max control char (second range)
        [TestCase("café", "caf\\u00e9")] // Non-ASCII Latin
        [TestCase("최예나", "\\ucd5c\\uc608\\ub098")] // Korean characters
        [TestCase("😀", "\\u{1f600}")] // Emoji (supplementary, uses variable-width)
        [TestCase("\U0010FFFF", "\\u{10ffff}")] // Max Unicode (supplementary)
        [TestCase("Hello\nWorld", "Hello\\nWorld")] // Newline in middle
        [TestCase("Tab\there", "Tab\\there")] // Tab in middle
        public void Escape_VariousInputs_ReturnsExpectedEscapedString(string input, string expectedOutput)
        {
            var result = GraphQLStrings.Escape(input);

            Assert.AreEqual(
                expectedOutput,
                result,
                $"Expected '{expectedOutput}' but got '{result}'");
        }

        [Test]
        public void Escape_NullInput_ReturnsNull()
        {
            var result = GraphQLStrings.Escape(null);
            Assert.IsNull(result);
        }

        [Test]
        public void UnescapeAndTrimDelimiters_NullOnFailureFalse_ReturnsOriginalString()
        {
            var input = "no delimiters";
            var result = GraphQLStrings.UnescapeAndTrimDelimiters(input, false);
            Assert.AreEqual(input, result);
        }
    }
}