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
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;
    using NUnit.Framework;

    [TestFixture]
    public class ReadOnlySpanCharExtensionsTests
    {
        // Fixed-width format tests: \uXXXX (exactly 4 hex digits)
        // Per September 2025 spec (and October 2021), must be exactly 4 hex digits and valid scalar value
        // Spec grammar: EscapedUnicode :: HexDigit HexDigit HexDigit HexDigit
        [TestCase(@"\u1234", true)]
        [TestCase(@"\u12F4", true)]
        [TestCase(@"\u0A1b", true)]
        [TestCase(@"\u0000", true)] // Valid scalar value
        [TestCase(@"\u007F", true)] // Valid scalar value
        [TestCase(@"\uE000", true)] // Valid scalar value after surrogate range
        [TestCase(@"\uFFFF", true)] // Max BMP value
        [TestCase(@"\u1", false)] // Too few hex digits (was incorrectly true in old impl)
        [TestCase(@"\u12", false)] // Too few hex digits (was incorrectly true in old impl)
        [TestCase(@"\u123", false)] // Too few hex digits (was incorrectly true in old impl)
        [TestCase(@"\uD800", false)] // High surrogate (invalid scalar value)
        [TestCase(@"\uDBFF", false)] // High surrogate
        [TestCase(@"\uDC00", false)] // Low surrogate (invalid scalar value)
        [TestCase(@"\uDFFF", false)] // Low surrogate
        [TestCase(@"\uDEAD", false)] // Unpaired surrogate (invalid scalar value)
        [TestCase(@"\ufffg", false)] // Invalid hex digit
        [TestCase(@"\ufGff", false)] // Invalid hex digit
        [TestCase(@"\r0A1b", false)] // Doesn't start with \u
        [TestCase(@"\u", false)] // No hex digits
        [TestCase(@"\", false)] // Just backslash
        [TestCase(@"", false)] // Empty
        [TestCase("\\u12\rC", false)] // Contains line terminator
        [TestCase(@"\u12345", false)] // Too many hex digits (5)
        [TestCase(@"\U1234", false)] // Wrong case for u
        [TestCase(@"abc", false)] // Not an escape sequence
        public void ReadOnlySpanChar_IsGraphQLEscapedUnicodeCharacter(string text, bool result)
        {
            Assert.AreEqual(result, text.AsSpan().IsGraphQLEscapedUnicodeCharacter());
        }

        // Variable-width format tests: \u{X} to \u{XXXXXX} (1-6 hex digits)
        // New in September 2025 spec
        [TestCase(@"\u{61}", true)] // 'a'
        [TestCase(@"\u{0}", true)] // Null character
        [TestCase(@"\u{1F600}", true)] // 😀 emoji (supplementary character)
        [TestCase(@"\u{10FFFF}", true)] // Maximum valid Unicode scalar value
        [TestCase(@"\u{E000}", true)] // First value after surrogate range
        [TestCase(@"\u{D7FF}", true)] // Last value before surrogate range
        [TestCase(@"\u{FF}", true)] // 2 hex digits
        [TestCase(@"\u{FFF}", true)] // 3 hex digits
        [TestCase(@"\u{FFFF}", true)] // 4 hex digits
        [TestCase(@"\u{FFFFF}", true)] // 5 hex digits
        [TestCase(@"\u{100000}", true)] // 6 hex digits
        [TestCase(@"\u{D800}", false)] // High surrogate (invalid)
        [TestCase(@"\u{DBFF}", false)] // High surrogate (invalid)
        [TestCase(@"\u{DC00}", false)] // Low surrogate (invalid)
        [TestCase(@"\u{DFFF}", false)] // Low surrogate (invalid)
        [TestCase(@"\u{DEAD}", false)] // Unpaired surrogate (invalid)
        [TestCase(@"\u{110000}", false)] // Beyond max Unicode value
        [TestCase(@"\u{FFFFFFF}", false)] // Too many hex digits (7)
        [TestCase(@"\u{}", false)] // No hex digits
        [TestCase(@"\u{", false)] // Unterminated
        [TestCase(@"\u{123", false)] // Missing closing brace
        [TestCase(@"\u123}", false)] // Missing opening brace
        [TestCase(@"\u{G}", false)] // Invalid hex digit
        [TestCase(@"\u{12G4}", false)] // Invalid hex digit
        public void ReadOnlySpanChar_IsGraphQLVariableWidthEscapedUnicodeCharacter(string text, bool result)
        {
            Assert.AreEqual(result, text.AsSpan().IsGraphQLVariableWidthEscapedUnicodeCharacter());
        }

        // Surrogate pair handling tests
        [TestCase(0xD800, 0xDC00, true, 0x10000)] // Valid pair, first supplementary char
        [TestCase(0xD83D, 0xDE00, true, 0x1F600)] // Valid pair, 😀 emoji
        [TestCase(0xDBFF, 0xDFFF, true, 0x10FFFF)] // Valid pair, max supplementary char
        [TestCase(0xD800, 0xD800, false, 0)] // High + High (invalid)
        [TestCase(0xDC00, 0xDC00, false, 0)] // Low + Low (invalid)
        [TestCase(0xDC00, 0xD800, false, 0)] // Low + High (wrong order)
        [TestCase(0xD7FF, 0xDC00, false, 0)] // Not high surrogate + Low
        [TestCase(0xD800, 0xE000, false, 0)] // High + Not low surrogate
        public void ReadOnlySpanChar_TryParseSurrogatePair(int high, int low, bool expectedResult, int expectedCodePoint)
        {
            var result = ReadOnlySpanCharExtensions.TryParseSurrogatePair(high, low, out int codePoint);
            Assert.AreEqual(expectedResult, result);
            if (expectedResult)
            {
                Assert.AreEqual(expectedCodePoint, codePoint);
            }
        }

        // Surrogate detection tests
        [TestCase(0xD800, true)] // First high surrogate
        [TestCase(0xDBFF, true)] // Last high surrogate
        [TestCase(0xD7FF, false)] // Before surrogate range
        [TestCase(0xDC00, false)] // Low surrogate (not high)
        [TestCase(0xE000, false)] // After surrogate range
        public void ReadOnlySpanChar_IsHighSurrogate(int value, bool expected)
        {
            Assert.AreEqual(expected, ReadOnlySpanCharExtensions.IsHighSurrogate(value));
        }

        [TestCase(0xDC00, true)] // First low surrogate
        [TestCase(0xDFFF, true)] // Last low surrogate
        [TestCase(0xDBFF, false)] // High surrogate (not low)
        [TestCase(0xD7FF, false)] // Before surrogate range
        [TestCase(0xE000, false)] // After surrogate range
        public void ReadOnlySpanChar_IsLowSurrogate(int value, bool expected)
        {
            Assert.AreEqual(expected, ReadOnlySpanCharExtensions.IsLowSurrogate(value));
        }

        [TestCase(@"\r", true)]
        [TestCase(@"\n", true)]
        [TestCase(@"\/", true)]
        [TestCase(@"\\", true)]
        [TestCase(@"\b", true)]
        [TestCase(@"\f", true)]
        [TestCase(@"\t", true)]
        [TestCase("\\\"", true)]
        [TestCase(@"\a", false)]
        [TestCase(@"\3", false)]
        [TestCase(@"\", false)]
        [TestCase(@"", false)]
        [TestCase(@"abc", false)]
        public void ReadOnlySpanChar_IsGraphQLEscapedCharacter(string text, bool result)
        {
            Assert.AreEqual(result, text.AsSpan().IsGraphQLEscapedCharacter());
        }

        [TestCase(@"", false)]
        [TestCase(@"\", true)]
        [TestCase(@"\u", true)]
        [TestCase(@"\u{", true)] // Could be start of variable-width
        [TestCase(@"\u12", true)] // Could be incomplete fixed-width
        [TestCase(@"\ua", true)] // Could be incomplete fixed-width
        [TestCase(@"\u{1", true)] // Could be incomplete variable-width
        [TestCase(@"\u{12", true)] // Could be incomplete variable-width
        [TestCase(@"\y", false)]
        [TestCase(@"\1", false)]
        [TestCase(@"\u12345", false)] // Too long for fixed-width, not in braces
        public void ReadOnlySpanChar_CouldBeGraphQLEscapedUnicodeCharacter(string text, bool result)
        {
            Assert.AreEqual(result, text.AsSpan().CouldBeGraphQLEscapedUnicodeCharacter());
        }
    }
}