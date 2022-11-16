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
    using System.Text.RegularExpressions;
    using GraphQL.AspNet.Execution.Parsing;

    /// <summary>
    /// Helper methods for serializing and deserialzing strings according to graphql standards.
    /// </summary>
    public static class GraphQLStrings
    {
        /// <summary>
        /// Helper method that will take a raw, double-quoted, and unicode escaped block of characters and
        /// generate a string value devoid of delimiters and unescaped using the rules of string delimiting
        /// for graphql. (supports single and triple quote blocks). An improperly delimited string will be
        /// returned as null.
        /// </summary>
        /// <param name="text">The text to manipulate.</param>
        /// <param name="nullOnFailure">if set to <c>true</c> should the string not be correctly delimited null is returned. When
        /// false and the stirng is not correclty delimited, the original string is returned unaltered.</param>        ///
        /// <remarks>
        /// e.g.  Converts  { "Hell\u019f" }  to HellƟ.
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
                    // convert    """ some text """  => "some text"
                    text = text.Slice(2, text.Length - 4);
                }

                if (text.Length >= 2 &&
                    text.StartsWith(ParserConstants.NormalStringDelimiterMemory.Span, StringComparison.Ordinal) &&
                    text.EndsWith(ParserConstants.NormalStringDelimiterMemory.Span, StringComparison.Ordinal))
                {
                    text = text.Slice(1, text.Length - 2);
                    return Regex.Unescape(text.ToString());
                }
            }

            // if this point is reached no delimiters were encountered,  its an invalid string
            // representation
            return nullOnFailure ? null : text.ToString();
        }

        /// <summary>
        /// Escapes the specified text turing necessary characters into escaped unicode
        /// representations of themselves.
        /// </summary>
        /// <param name="text">The text to escape.</param>
        /// <remarks>Example:  "endingQuote\""  =>   "endingQuote\\u0022".</remarks>
        /// <returns>The escaped text.</returns>
        public static string Escape(string text)
        {
            if (text == null)
                return null;

            StringBuilder sb = new StringBuilder();
            foreach (char c in text)
            {
                if (c > 127 || c == '"')
                {
                    string encodedValue = "\\u" + ((int)c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}