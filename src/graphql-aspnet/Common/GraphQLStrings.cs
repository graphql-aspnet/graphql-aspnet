// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common
{
    using System;
    using System.Text;
    using GraphQL.AspNet.Execution.Parsing;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;

    /// <summary>
    /// Helper methods for serializing and deserialzing strings according to graphql standards.
    /// </summary>
    public static class GraphQLStrings
    {
        /// <summary>
        /// Helper method that will take a raw, double-quoted, and unicode escaped block of characters and
        /// generate a string value devoid of delimiters and unescaped characters using the rules of string delimiting
        /// for graphql. (supports single and triple quote blocks). An improperly delimited string will be
        /// returned as null.
        /// </summary>
        /// <param name="text">The text to manipulate.</param>
        /// <param name="nullOnFailure">if set to <c>true</c> should the string not be correctly delimited, null is returned. When
        /// false and the string is not correctly delimited, the original string is returned unaltered.</param>
        /// <remarks>
        /// e.g.  Converts  "Hell\u019f" => HellƟ, "Emoji\u{1F600}" => Emoji😀.
        /// <br/>
        /// spec: <see href="https://spec.graphql.org/September2025/#sec-String-Value" />.
        /// </remarks>
        /// <returns>The unescaped string or <c>null</c>.</returns>
        public static string UnescapeAndTrimDelimiters(ReadOnlySpan<char> text, bool nullOnFailure = true)
        {
            // the provided text should not be empty
            // an empty string would still be supplied as a set of delimiters: {""}
            if (!text.IsEmpty)
            {
                if (text.Length >= 6 &&
                    text.StartsWith(ParserConstants.BlockStringDelimiterMemory.Span, StringComparison.Ordinal) &&
                    text.EndsWith(ParserConstants.BlockStringDelimiterMemory.Span, StringComparison.Ordinal))
                {
                    // convert    """some text"""  => some text
                    text = text.Slice(3, text.Length - 6);
                    return text.ToString();
                }

                if (text.Length >= 2 &&
                    text.StartsWith(ParserConstants.NormalStringDelimiterMemory.Span, StringComparison.Ordinal) &&
                    text.EndsWith(ParserConstants.NormalStringDelimiterMemory.Span, StringComparison.Ordinal))
                {
                    text = text.Slice(1, text.Length - 2);
                    return UnescapeString(text);
                }
            }

            // if this point is reached no delimiters were encountered,  its an invalid string
            // representation
            return nullOnFailure ? null : text.ToString();
        }

        /// <summary>
        /// Unescapes a GraphQL string.
        /// Supports standard escapes (\n, \t, etc.), fixed-width unicode (\uXXXX), variable-width unicode (\u{...}),
        /// and surrogate pairs (\uXXXX\uXXXX).
        /// </summary>
        /// <param name="text">The text to unescape.</param>
        /// <returns>The unescaped string, or null if the string contains invalid escape sequences.</returns>
        private static string UnescapeString(ReadOnlySpan<char> text)
        {
            if (text.IndexOf('\\') == -1)
                return text.ToString();

            var result = new StringBuilder(text.Length);
            int i = 0;

            while (i < text.Length)
            {
                if (text[i] == '\\' && i + 1 < text.Length)
                {
                    char next = text[i + 1];

                    switch (next)
                    {
                        // Standard escape sequences
                        case 'n':
                            result.Append('\n');
                            i += 2;
                            continue;
                        case 't':
                            result.Append('\t');
                            i += 2;
                            continue;
                        case 'r':
                            result.Append('\r');
                            i += 2;
                            continue;
                        case 'b':
                            result.Append('\b');
                            i += 2;
                            continue;
                        case 'f':
                            result.Append('\f');
                            i += 2;
                            continue;
                        case '"':
                        case '\\':
                        case '/':
                            result.Append(next);
                            i += 2;
                            continue;

                        // Check for variable-width format: \u{...}
                        case 'u' when i + 2 < text.Length && text[i + 2] == '{':
                            {
                                if (!TryProcessVariableWidthUnicodeEscape(text, i, out string unicodeChar, out int charsConsumed))
                                    return null;

                                result.Append(unicodeChar);
                                i += charsConsumed;
                                continue;
                            }

                        // Fixed-width format: \uXXXX
                        case 'u' when i + 5 < text.Length:
                            {
                                if (!TryProcessFixedWidthUnicodeEscape(text, i, out string unicodeChar, out int charsConsumed))
                                    return null;

                                result.Append(unicodeChar);
                                i += charsConsumed;
                                continue;
                            }

                        case 'u':
                            return null; // Incomplete unicode escape

                        default:
                            return null; // Unknown escape sequence
                    }
                }

                result.Append(text[i]);
                i++;
            }

            return result.ToString();
        }

        /// <summary>
        /// Processes a variable-width unicode escape sequence in the format \u{...}.
        /// </summary>
        /// <param name="text">The text containing the escape sequence.</param>
        /// <param name="position">The current position at the backslash character.</param>
        /// <param name="unicodeChar">The decoded unicode character(s).</param>
        /// <param name="charsConsumed">The number of characters consumed from the input.</param>
        /// <returns><c>true</c> if the escape sequence was successfully processed; otherwise, <c>false</c>.</returns>
        private static bool TryProcessVariableWidthUnicodeEscape(
            ReadOnlySpan<char> text,
            int position,
            out string unicodeChar,
            out int charsConsumed)
        {
            unicodeChar = null;
            charsConsumed = 0;

            int closeIndex = text.Slice(position + 3).IndexOf('}');
            if (closeIndex == -1)
                return false; // Invalid escape sequence

            var hexChars = text.Slice(position + 3, closeIndex);
            if (hexChars.Length < 1 || hexChars.Length > 6)
                return false; // Invalid length

            if (!TryParseHexValue(hexChars, out int codePoint))
                return false; // Invalid hex

            if (!IsValidScalarValue(codePoint))
                return false; // Invalid scalar value

            unicodeChar = char.ConvertFromUtf32(codePoint);
            charsConsumed = 3 + closeIndex + 1;
            return true;
        }

        private static bool TryProcessFixedWidthUnicodeEscape(
            ReadOnlySpan<char> text,
            int position,
            out string unicodeChar,
            out int charsConsumed)
        {
            unicodeChar = null;
            charsConsumed = 0;

            var hexChars = text.Slice(position + 2, 4);
            if (!TryParseHexValue(hexChars, out int codePoint))
                return false; // Invalid hex

            // Check for surrogate pair
            if (ReadOnlySpanCharExtensions.IsHighSurrogate(codePoint))
            {
                // Look ahead for low surrogate
                if (position + 11 < text.Length && text[position + 6] == '\\' && text[position + 7] == 'u')
                {
                    var nextHexChars = text.Slice(position + 8, 4);
                    if (TryParseHexValue(nextHexChars, out int lowSurrogate) &&
                        ReadOnlySpanCharExtensions.TryParseSurrogatePair(codePoint, lowSurrogate, out int combinedCodePoint))
                    {
                        unicodeChar = char.ConvertFromUtf32(combinedCodePoint);
                        charsConsumed = 12;
                        return true;
                    }
                }

                // High surrogate without valid low surrogate is invalid
                return false;
            }

            // Low surrogate without preceding high surrogate is invalid
            if (ReadOnlySpanCharExtensions.IsLowSurrogate(codePoint))
                return false;

            // Regular code point (not surrogate)
            if (!IsValidScalarValue(codePoint))
                return false;

            unicodeChar = ((char)codePoint).ToString();
            charsConsumed = 6;
            return true;
        }

        private static bool TryParseHexValue(ReadOnlySpan<char> hexChars, out int value)
        {
            value = 0;
            for (int i = 0; i < hexChars.Length; i++)
            {
                char c = hexChars[i];
                int digit;

                if (c >= '0' && c <= '9')
                    digit = c - '0';
                else if (c >= 'A' && c <= 'F')
                    digit = c - 'A' + 10;
                else if (c >= 'a' && c <= 'f')
                    digit = c - 'a' + 10;
                else
                    return false;

                value = (value * 16) + digit;
            }

            return true;
        }

        /// <summary>
        /// Validates that a value is a valid Unicode scalar value.
        /// </summary>
        private static bool IsValidScalarValue(int value)
        {
            if (value is < ParserConstants.Characters.MIN_SCALAR_VALUE or > ParserConstants.Characters.MAX_SCALAR_VALUE)
                return false;

            if (value is >= ParserConstants.Characters.SURROGATE_MIN and <= ParserConstants.Characters.SURROGATE_MAX)
                return false;

            return true;
        }

        /// <summary>
        /// Escapes the specified text turning necessary characters into escaped unicode
        /// representations of themselves. Control characters (U+0000-U+001F, U+007F-U+009F),
        /// double quotes, and characters beyond the basic ASCII range are escaped.
        /// Supplementary characters (beyond U+FFFF) use the variable-width format \u{...}.
        /// Spec: <see href="https://spec.graphql.org/September2025/#sec-String-Value" />.
        /// </summary>
        /// <param name="text">The text to escape.</param>
        /// <remarks>
        /// Examples:
        /// "endingQuote\""  =>   "endingQuote\u0022"
        /// "Emoji😀"  =>   "Emoji\u{1F600}"
        /// "Control\u0001"  =>   "Control\u0001"
        /// </remarks>
        /// <returns>The escaped text.</returns>
        public static string Escape(string text)
        {
            if (text == null)
                return null;

            var sb = new StringBuilder();
            int i = 0;

            while (i < text.Length)
            {
                char c = text[i];
                int codePoint = c;

                // Check if this is a high surrogate (start of a surrogate pair)
                if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                {
                    // Convert surrogate pair to code point
                    codePoint = char.ConvertToUtf32(c, text[i + 1]);
                    sb.Append("\\u{");
                    sb.Append(codePoint.ToString("x"));
                    sb.Append('}');
                    i += 2;
                    continue;
                }

                // Control characters U+0000-U+001F (except common escapes)
                if (codePoint <= 0x001F)
                {
                    switch (codePoint)
                    {
                        // Use standard escape sequences for common characters
                        case '\n':
                            sb.Append("\\n");
                            break;
                        case '\r':
                            sb.Append("\\r");
                            break;
                        case '\t':
                            sb.Append("\\t");
                            break;
                        case '\b':
                            sb.Append("\\b");
                            break;
                        case '\f':
                            sb.Append("\\f");
                            break;

                        default:
                            // Other control characters use \uXXXX
                            sb.Append("\\u");
                            sb.Append(codePoint.ToString("x4"));
                            break;
                    }

                    i++;
                    continue;
                }

                // Control characters U+007F-U+009F
                if (codePoint >= ParserConstants.Characters.CONTROL_CHAR_MIN && codePoint <= ParserConstants.Characters.CONTROL_CHAR_MAX)
                {
                    sb.Append("\\u");
                    sb.Append(codePoint.ToString("x4"));
                    i++;
                    continue;
                }

                // Double quote
                if (c == '"')
                {
                    sb.Append("\\\"");
                    i++;
                    continue;
                }

                // Backslash
                if (c == '\\')
                {
                    sb.Append("\\\\");
                    i++;
                    continue;
                }

                // Characters beyond basic ASCII (> 127)
                if (codePoint > 127)
                {
                    sb.Append("\\u");
                    sb.Append(codePoint.ToString("x4"));
                    i++;
                    continue;
                }

                // Regular printable ASCII characters
                sb.Append(c);
                i++;
            }

            return sb.ToString();
        }
    }
}