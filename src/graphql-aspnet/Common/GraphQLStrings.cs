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
    using System.Text.RegularExpressions;
    using GraphQL.AspNet.Parsing;

    /// <summary>
    /// Helper methods for serializing and deserialzing strings according to graphql standards.
    /// </summary>
    public static class GraphQLStrings
    {
        /// <summary>
        /// <para>
        /// Helper method that will take a raw, delimited, and UTC escaped block of a query and
        /// generate a string value devoid of delimiters and unescaped using the rules of string delimiting
        /// for graphql. (supports single and triple quote blocks). An improperly delimited string will be
        /// returned as null.
        /// </para>
        /// <para>
        /// e.g.  Converts  {"Hell\u019f"}  to {HellƟ}.
        /// </para>
        /// </summary>
        /// <param name="text">The text to manipulate.</param>
        /// <param name="nullOnFailure">if set to <c>true</c> should the string not be correctly delimited null is returned. When
        /// false and the stirng is not correclty delimited, the original string is returned unaltered.</param>
        /// <returns>System.String.</returns>
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

                // if this point is reached no delimiters were encountered,  its an invalid string
                // representation
            }

            return nullOnFailure ? null : text.ToString();
        }
    }
}