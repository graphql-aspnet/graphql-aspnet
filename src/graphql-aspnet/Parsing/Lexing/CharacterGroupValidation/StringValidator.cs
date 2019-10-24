// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.Lexing.CharacterGroupValidation
{
    using System;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using CHARS = GraphQL.AspNet.Parsing.ParserConstants.Characters;

    /// <summary>
    /// A validator that will inspect a span to ensure its characters
    /// are valid for a string in graphql
    /// Spec: https://graphql.github.io/graphql-spec/June2018/#sec-String-Value .
    /// </summary>
    public class StringValidator
    {
        private const string UNINTIALIZED_STRING = "Uninitialized string. Expected '{0}' received '{1}'";
        private const string UNTERMINATED_STRING = "Unterminated string. Expected '{0}' received '{1}'";
        private const string INVALID_STRING = "Invalid string. Expected '{0}', recieved '{1}'";
        private const string INVALID_ESCAPED_CHAR = "Unknown escaped character: '{0}'";

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static StringValidator Instance { get; } = new StringValidator();

        /// <summary>
        /// Prevents a default instance of the <see cref="StringValidator"/> class from being created.
        /// </summary>
        private StringValidator()
        {
        }

        /// <summary>
        /// Validates the set of characters, together, form a valid string or throws an exception.
        /// </summary>
        /// <param name="source">The source containing the phrase.</param>
        /// <param name="phrase">The phrase to validate.</param>
        /// <param name="location">The location the source where the phrase started.</param>
        public void ValidateOrThrow(
            in SourceText source,
            in ReadOnlySpan<char> phrase,
            SourceLocation location)
        {
            // gotta be at least an empty string:  ""
            if (phrase.Length < 2)
            {
                throw new GraphQLSyntaxException(
                    location,
                    string.Format(UNTERMINATED_STRING, CHARS.DOUBLE_QUOTE, string.Empty));
            }

            if (phrase.Length == 3
                && phrase.Equals(ParserConstants.BlockStringDelimiterMemory.Span, StringComparison.OrdinalIgnoreCase))
            {
                throw new GraphQLSyntaxException(
                    location,
                    string.Format(INVALID_STRING, CHARS.DOUBLE_QUOTE, "BLOCK_DELIMITER"));
            }

            // deteremine block string or not
            ReadOnlySpan<char> delimiter = IsBlockString(phrase)
                ? ParserConstants.BlockStringDelimiterMemory.Span
                : ParserConstants.NormalStringDelimiterMemory.Span;

            // ensure text startr with the expected character set
            if (!phrase.StartsWith(delimiter))
            {
                throw new GraphQLSyntaxException(
                    location,
                    string.Format(UNINTIALIZED_STRING, delimiter.ToString(), phrase[0]));
            }

            // it must be at least delimiter x2 long
            if (phrase.Length < (delimiter.Length * 2))
            {
                throw new GraphQLSyntaxException(
                    location,
                    string.Format(UNTERMINATED_STRING, delimiter.ToString(), phrase.Slice(delimiter.Length).ToString()));
            }

            // ensure the text ends with the delimiter
            if (!phrase.EndsWith(delimiter))
            {
                throw new GraphQLSyntaxException(
                    location,
                    string.Format(UNTERMINATED_STRING, delimiter.ToString(), phrase[phrase.Length - 1]));
            }

            // ensure the character prior to the delimiter is not an escape sequence
            if (phrase[phrase.Length - delimiter.Length - 1] == '\\')
            {
                throw new GraphQLSyntaxException(
                    location,
                    string.Format(UNTERMINATED_STRING, delimiter.ToString(), phrase.Slice(phrase.Length - delimiter.Length).ToString()));
            }

            // get the contents of the string (without delimiters) and inspect
            // for any escape sequences to ensure they are valid
            var text = phrase.Slice(delimiter.Length, phrase.Length - (2 * delimiter.Length));
            if (text.IndexOf(CHARS.ESCAPED_CHAR_INDICATOR) < 0)
                return;

            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] != CHARS.ESCAPED_CHAR_INDICATOR)
                {
                    continue;
                }

                // the central text can't end in a backslash
                if (i == text.Length - 1)
                {
                    throw new GraphQLSyntaxException(
                        source.OffsetLocation(location, i + delimiter.Length),
                        string.Format(INVALID_ESCAPED_CHAR, text[i]));
                }

                // with a backslash to start we'll either hit a valid character
                // then continue from there
                // or we never will (at which point fail out)
                var validEscapeChar = false;
                int j;
                for (j = i + 1; j < text.Length; j++)
                {
                    var testSlice = text.Slice(i, j - i + 1);
                    if (testSlice.IsGraphQLEscapedCharacter())
                    {
                        validEscapeChar = true;
                        break;
                    }

                    if (!testSlice.CouldBeGraphQLEscapedUnicodeCharacter())
                        break;
                }

                if (!validEscapeChar)
                {
                    throw new GraphQLSyntaxException(
                        source.OffsetLocation(location, i + delimiter.Length),
                        string.Format(INVALID_ESCAPED_CHAR, text.Slice(i, j - i + 1).ToString()));
                }

                i = j;
            }
        }

        /// <summary>
        /// Determines whether the given span of text is a block string or not
        /// by its starting delimiter.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>System.Boolean.</returns>
        public static bool IsBlockString(in ReadOnlySpan<char> text)
        {
            return text.IndexOf(ParserConstants.BlockStringDelimiterMemory.Span, StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>
        /// Determines whether the given text is a fully formed string according to graphql. Accounts for
        /// multi-line "triple quote" delimited strings, double quoted strings and an escaped string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns><c>true</c> if the supplied text constitutes a complete string; otherwise, <c>false</c>.</returns>
        public static NextPhraseResult IsDelimitedString(in ReadOnlySpan<char> text)
        {
            // need at least 3 characters before we can deteremine validity
            // result will be:
            // 1) '"""' (a triple quote indicating a block string)
            // OR
            // 2) '""?' (an empty string followed by some character)
            if (text.Length < 3)
                return NextPhraseResult.Continue;

            var isBlockString = IsBlockString(text);

            // if this isn't a block string then check
            // for special case, #2 above
            if (!isBlockString && text.StartsWith(ParserConstants.BlockStringDelimiterMemory.Span.Slice(0, 2)))
                return NextPhraseResult.CompleteOnPrevious;

            ReadOnlySpan<char> delimiter = isBlockString
                ? ParserConstants.BlockStringDelimiterMemory.Span
                : ParserConstants.NormalStringDelimiterMemory.Span;

            // read until the span ends with the correct delimiter
            // and that (start of delimiter - 1) is not an escaped quote
            if (text.Length <= delimiter.Length || !text.EndsWith(delimiter))
                return NextPhraseResult.Continue;

            return text[text.Length - delimiter.Length - 1] != '\\'
                ? NextPhraseResult.Complete
                : NextPhraseResult.Continue;
        }
    }
}