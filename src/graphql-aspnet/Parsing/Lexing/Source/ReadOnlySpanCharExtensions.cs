// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.Lexing.Source
{
    using System;
    using CHARS = GraphQL.AspNet.Parsing.ParserConstants.Characters;

    /// <summary>
    /// Helper methods against a span of characters.
    /// </summary>
    public static class ReadOnlySpanCharExtensions
    {
        /// <summary>
        /// Determines whether the given span of characters "could be" a properly escaped
        /// unique sequence according to graphQL. This method will rule out a string from being
        /// a valid unicode string but can't conclusively tell that it is valid in all cases
        /// spec: https://graphql.github.io/graphql-spec/June2018/#sec-String-Value .
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns><c>true</c> if span could potentially be an escaped unicode character; otherwise, <c>false</c>.</returns>
        public static bool CouldBeGraphQLEscapedUnicodeCharacter(this in ReadOnlySpan<char> text)
        {
            // at least: \u0
            // at most:  \uFFFF
            if (text.Length > 6)
                return false;

            if (text.Length == 1)
                return text[0] == CHARS.ESCAPED_CHAR_INDICATOR;

            if (text.Length == 2)
                return text.StartsWith(CHARS.UnicodePrefix.Span, StringComparison.Ordinal);

            // at this point its either a real unicode sequence or its not
            return text.IsGraphQLEscapedUnicodeCharacter();
        }

        /// <summary>
        /// Determines whether the given span of characters represents a properly escaped
        /// unique sequence according to graphQL
        /// spec: https://graphql.github.io/graphql-spec/June2018/#sec-String-Value.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns><c>true</c> if span represents an escaped unicode character; otherwise, <c>false</c>.</returns>
        public static bool IsGraphQLEscapedUnicodeCharacter(this in ReadOnlySpan<char> text)
        {
            // at least: \u0
            // at most:  \uFFFF
            if (text.Length < 3 || text.Length > 6)
                return false;

            if (!text.StartsWith(CHARS.UnicodePrefix.Span, StringComparison.Ordinal))
                return false;

            // all chars after the prefix must be hex chars
            for (var i = 2; i < text.Length; i++)
            {
                if (char.IsDigit(text[i])) // 0-9
                    continue;

                if (text[i] >= 65 && text[i] <= 70) // A-F
                    continue;

                if (text[i] >= 97 && text[i] <= 102) // a-f
                    continue;

                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the given span of characters represents a valid escaped sequence of characters or not.
        /// Spec: https://graphql.github.io/graphql-spec/June2018/#sec-Appendix-Grammar-Summary.Lexical-Tokens .
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>System.Boolean.</returns>
        public static bool IsGraphQLEscapedCharacter(this in ReadOnlySpan<char> text)
        {
            if (text.Length < 2)
                return false;

            if (text.Length == 2)
            {
                return text[0] == '\\' && CHARS.ValidEscapableCharacters.Span.IndexOf(text[1]) >= 0;
            }

            return text.IsGraphQLEscapedUnicodeCharacter();
        }
    }
}