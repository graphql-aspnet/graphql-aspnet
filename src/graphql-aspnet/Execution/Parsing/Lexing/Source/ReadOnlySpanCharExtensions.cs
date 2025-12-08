// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Parsing.Lexing.Source
{
    using System;
    using CHARS = GraphQL.AspNet.Execution.Parsing.ParserConstants.Characters;

    /// <summary>
    /// Helper methods against a span of characters.
    /// </summary>
    internal static class ReadOnlySpanCharExtensions
    {
        /// <summary>
        /// Determines whether the given span of characters "could be" a properly escaped
        /// unicode sequence according to GraphQL. This method will rule out a string from being
        /// a valid unicode string but can't conclusively tell that it is valid in all cases.
        /// Spec: <see href="https://spec.graphql.org/September2025/#sec-String-Value" /> .
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns><c>true</c> if span could potentially be an escaped unicode character; otherwise, <c>false</c>.</returns>
        public static bool CouldBeGraphQLEscapedUnicodeCharacter(this in ReadOnlySpan<char> text)
        {
            // Fixed-width format: \uXXXX (exactly 4 hex digits)
            // Variable-width format: \u{X} to \u{XXXXXX} (1-6 hex digits in braces)
            if (text.Length == 1)
                return text[0] == CHARS.ESCAPED_CHAR_INDICATOR;

            if (text.Length == 2)
                return text.StartsWith(CHARS.UnicodePrefix.Span, StringComparison.Ordinal);

            // Check for variable-width format: \u{...}
            if (text.StartsWith(CHARS.VariableWidthUnicodePrefix.Span, StringComparison.Ordinal))
            {
                // \u{ - could be start
                if (text.Length == 3)
                    return true;

                // \u{X... - check if all chars after \u{ are hex digits (incomplete, no closing brace yet)
                // or if it's a complete variable-width sequence
                return text.IsGraphQLVariableWidthEscapedUnicodeCharacter() ||
                       AreAllHexDigits(text.Slice(3));
            }

            // Check for fixed-width format: \uXXXX
            if (text.StartsWith(CHARS.UnicodePrefix.Span, StringComparison.Ordinal))
            {
                // For lengths 3-5, check if remaining chars are hex digits (incomplete sequence)
                if (text.Length >= 3 && text.Length < 6)
                    return AreAllHexDigits(text.Slice(2));

                // Length 6 or more - must be a complete valid sequence
                return text.IsGraphQLEscapedUnicodeCharacter();
            }

            return false;
        }

        private static bool AreAllHexDigits(ReadOnlySpan<char> chars)
        {
            for (int i = 0; i < chars.Length; i++)
            {
                if (!IsHexDigit(chars[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the given span of characters represents a properly escaped
        /// unicode sequence according to GraphQL using the fixed-width format: \uXXXX (exactly 4 hex digits).
        /// This method also validates that the resulting value is a valid Unicode scalar value.
        /// Spec: <see href="https://spec.graphql.org/September2025/#sec-String-Value" />.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns><c>true</c> if span represents an escaped unicode character; otherwise, <c>false</c>.</returns>
        public static bool IsGraphQLEscapedUnicodeCharacter(this in ReadOnlySpan<char> text)
        {
            // Fixed-width format: \uXXXX (exactly 4 hex digits)
            if (text.Length != 6)
                return false;

            if (!text.StartsWith(CHARS.UnicodePrefix.Span, StringComparison.Ordinal))
                return false;

            // All chars after the prefix must be hex chars
            if (!IsHexDigit(text[2]) || !IsHexDigit(text[3]) || !IsHexDigit(text[4]) || !IsHexDigit(text[5]))
                return false;

            // Parse the hex value and validate it's a valid scalar value
            int value = ParseHexValue(text.Slice(2, 4));
            return IsValidScalarValue(value);
        }

        /// <summary>
        /// Determines whether the given span of characters represents a properly escaped
        /// unicode sequence according to GraphQL using the variable-width format: \u{X} to \u{XXXXXX} (1-6 hex digits in braces).
        /// This method also validates that the resulting value is a valid Unicode scalar value.
        /// Spec: <see href="https://spec.graphql.org/September2025/#sec-String-Value" />.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns><c>true</c> if span represents a variable-width escaped unicode character; otherwise, <c>false</c>.</returns>
        public static bool IsGraphQLVariableWidthEscapedUnicodeCharacter(this in ReadOnlySpan<char> text)
        {
            // Variable-width format: \u{X} to \u{XXXXXX} (1-6 hex digits in braces)
            // Minimum length: \u{X} = 5 characters
            // Maximum length: \u{XXXXXX} = 10 characters
            if (text.Length < 5 || text.Length > 10)
                return false;

            if (!text.StartsWith(CHARS.VariableWidthUnicodePrefix.Span, StringComparison.Ordinal))
                return false;

            if (text[text.Length - 1] != CHARS.CLOSE_BRACE)
                return false;

            // Extract hex digits between braces
            var hexDigits = text.Slice(3, text.Length - 4);

            // Must have 1-6 hex digits
            if (hexDigits.Length < 1 || hexDigits.Length > 6)
                return false;

            // All characters must be hex digits
            for (var i = 0; i < hexDigits.Length; i++)
            {
                if (!IsHexDigit(hexDigits[i]))
                    return false;
            }

            // Parse the hex value and validate it's a valid scalar value
            int value = ParseHexValue(hexDigits);
            return IsValidScalarValue(value);
        }

        /// <summary>
        /// Determines if a character is a valid hexadecimal digit (0-9, A-F, a-f).
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns><c>true</c> if the character is a hex digit; otherwise, <c>false</c>.</returns>
        private static bool IsHexDigit(char c)
        {
            return c is >= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f';
        }

        /// <summary>
        /// Parses a span of hexadecimal characters into an integer value.
        /// </summary>
        /// <param name="hexChars">The hexadecimal characters to parse.</param>
        /// <returns>The integer value represented by the hex characters.</returns>
        private static int ParseHexValue(ReadOnlySpan<char> hexChars)
        {
            int value = 0;
            for (int i = 0; i < hexChars.Length; i++)
            {
                value = (value * 16) + GetHexValue(hexChars[i]);
            }

            return value;
        }

        /// <summary>
        /// Gets the numeric value of a hexadecimal digit character.
        /// </summary>
        /// <param name="c">The hex digit character.</param>
        /// <returns>The numeric value (0-15).</returns>
        private static int GetHexValue(char c)
        {
            if (c is >= '0' and <= '9')
                return c - '0';

            if (c is >= 'A' and <= 'F')
                return c - 'A' + 10;

            if (c is >= 'a' and <= 'f')
                return c - 'a' + 10;

            return 0;
        }

        /// <summary>
        /// Validates that a value is a valid Unicode scalar value.
        /// Valid scalar values are U+0000 to U+D7FF or U+E000 to U+10FFFF (excluding surrogate pairs).
        /// Spec: <see href="https://spec.graphql.org/September2025/#sec-String-Value" />.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <returns><c>true</c> if the value is a valid scalar value; otherwise, <c>false</c>.</returns>
        private static bool IsValidScalarValue(int value)
        {
            if (value is < CHARS.MIN_SCALAR_VALUE or > CHARS.MAX_SCALAR_VALUE)
                return false;

            // Reject unpaired surrogates (U+D800 to U+DFFF)
            if (value is >= CHARS.SURROGATE_MIN and <= CHARS.SURROGATE_MAX)
                return false;

            return true;
        }

        /// <summary>
        /// Determines if the given span of characters represents a valid escaped sequence of characters or not.
        /// Supports both standard escape sequences (\n, \t, etc.) and unicode escape sequences (\uXXXX and \u{...}).
        /// Spec: <see href="https://spec.graphql.org/September2025/#sec-String-Value" /> .
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>System.Boolean.</returns>
        public static bool IsGraphQLEscapedCharacter(this in ReadOnlySpan<char> text)
        {
            if (text.Length < 2)
                return false;

            if (text.Length == 2)
                return text[0] == '\\' && CHARS.ValidEscapableCharacters.Span.IndexOf(text[1]) >= 0;

            // Check for unicode escape sequences (both fixed and variable-width)
            return text.IsGraphQLEscapedUnicodeCharacter() || text.IsGraphQLVariableWidthEscapedUnicodeCharacter();
        }

        /// <summary>
        /// Attempts to parse a surrogate pair from two consecutive \uXXXX escape sequences.
        /// The spec allows legacy support for surrogate pairs encoded as two sequential \uXXXX sequences.
        /// Spec: <see href="https://spec.graphql.org/September2025/#sec-String-Value" />.
        /// </summary>
        /// <param name="highSurrogate">The high surrogate value (U+D800-U+DBFF).</param>
        /// <param name="lowSurrogate">The low surrogate value (U+DC00-U+DFFF).</param>
        /// <param name="codePoint">The resulting Unicode code point if valid.</param>
        /// <returns><c>true</c> if the surrogates form a valid pair; otherwise, <c>false</c>.</returns>
        public static bool TryParseSurrogatePair(int highSurrogate, int lowSurrogate, out int codePoint)
        {
            codePoint = 0;

            // Validate high surrogate is in range U+D800-U+DBFF
            if (highSurrogate < CHARS.HIGH_SURROGATE_MIN || highSurrogate > CHARS.HIGH_SURROGATE_MAX)
                return false;

            // Validate low surrogate is in range U+DC00-U+DFFF
            if (lowSurrogate < CHARS.LOW_SURROGATE_MIN || lowSurrogate > CHARS.LOW_SURROGATE_MAX)
                return false;

            // Calculate the code point from the surrogate pair
            // Formula: (high - 0xD800) * 0x400 + (low - 0xDC00) + 0x10000
            codePoint = ((highSurrogate - CHARS.HIGH_SURROGATE_MIN) * 0x400) +
                        (lowSurrogate - CHARS.LOW_SURROGATE_MIN) +
                        0x10000;

            return true;
        }

        /// <summary>
        /// Determines if a value is in the high surrogate range (U+D800-U+DBFF).
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns><c>true</c> if the value is a high surrogate; otherwise, <c>false</c>.</returns>
        public static bool IsHighSurrogate(int value)
        {
            return value >= CHARS.HIGH_SURROGATE_MIN && value <= CHARS.HIGH_SURROGATE_MAX;
        }

        /// <summary>
        /// Determines if a value is in the low surrogate range (U+DC00-U+DFFF).
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns><c>true</c> if the value is a low surrogate; otherwise, <c>false</c>.</returns>
        public static bool IsLowSurrogate(int value)
        {
            return value >= CHARS.LOW_SURROGATE_MIN && value <= CHARS.LOW_SURROGATE_MAX;
        }
    }
}