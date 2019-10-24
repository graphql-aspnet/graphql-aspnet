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

    /// <summary>
    /// A validator that will inspect a span to ensure its characters
    /// are valid for a number in graphql
    /// Spec: https://graphql.github.io/graphql-spec/June2018/#sec-Int-Value
    /// Spec: https://graphql.github.io/graphql-spec/June2018/#sec-Float-Value .
    /// </summary>
    public class NumberValidator
    {
        private const string INVALID_NUMBER_EMPTY = "Invalid number. No phrase provided.";
        private const string INVALID_NUMBER = "Invalid Number. Expected '{0}', received '{1}'.";

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static NumberValidator Instance { get; } = new NumberValidator();

        /// <summary>
        /// Prevents a default instance of the <see cref="NumberValidator"/> class from being created.
        /// </summary>
        private NumberValidator()
        {
        }

        /// <summary>
        /// Validates the set of characters, together, form a valid number or throws an exception.
        /// </summary>
        /// <param name="source">The source containing the phrase.</param>
        /// <param name="phrase">The phrase to validate.</param>
        /// <param name="location">The location the source where the phrase started.</param>
        public void ValidateOrThrow(in SourceText source, in ReadOnlySpan<char> phrase, SourceLocation location)
        {
            if (phrase.Length == 0)
                throw new GraphQLSyntaxException(location, INVALID_NUMBER_EMPTY);

            // must start with negative sign or digit
            if (phrase[0] != '-' && !char.IsDigit(phrase[0]))
            {
                throw new GraphQLSyntaxException(
                    location,
                    string.Format(INVALID_NUMBER, "-, 0-9", phrase[0]));
            }

            // ends with a number
            if (!char.IsDigit(phrase[phrase.Length - 1]))
            {
                throw new GraphQLSyntaxException(
                    source.OffsetLocation(location, phrase.Length - 1),
                    string.Format(INVALID_NUMBER, "0-9", phrase[phrase.Length - 1]));
            }

            // ensure correct number and location of special characters (., e, E)
            var hasDecimal = false;
            var hasLetterE = false;
            for (var index = 1; index < phrase.Length - 1; index++)
            {
                // ensure only one decimal point
                if (this.MatchCharFoundOnlyOnceAndNextCharIsValidOrThrow(
                    source,
                    phrase,
                    location,
                    '.',
                    hasDecimal,
                    index,
                    (c) => !char.IsDigit(c),
                    "0-9"))
                {
                    hasDecimal = true;
                    continue;
                }

                // ensure only one e (or E)
                if (this.MatchCharFoundOnlyOnceAndNextCharIsValidOrThrow(
                    source,
                    phrase,
                    location,
                    'e',
                    hasLetterE,
                    index,
                    (c) => !char.IsDigit(c) && c != '-',
                    "0-9,e,-"))
                {
                    hasLetterE = true;
                    continue;
                }

                if (this.MatchCharFoundOnlyOnceAndNextCharIsValidOrThrow(
                    source,
                    phrase,
                    location,
                    'E',
                    hasLetterE,
                    index,
                    (c) => !char.IsDigit(c) && c != '-',
                    "0-9,e,-"))
                {
                    hasLetterE = true;
                }
            }
        }

        /// <summary>
        /// Returns a value indicating if the next character after the current index
        /// is a digit or not. Also returns true if there is no character after the current one.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="text">The text phrase within the source being inspected.</param>
        /// <param name="location">The starting location of the text in the source.</param>
        /// <param name="matchChar">The character to check for.</param>
        /// <param name="matchCharAlreadySeen">if set to <c>true</c> indicate the character has already been
        /// seen once in the text.</param>
        /// <param name="currentIndex">The current index being inspected in the phrase.</param>
        /// <param name="nextCharChecker">A function to validate the next character in the sequence
        /// after a match is found.</param>
        /// <param name="expectedValues">The expected values to display in the error message.</param>
        /// <returns><c>true</c> if digit or nothing found, <c>false</c> otherwise.</returns>
        private bool MatchCharFoundOnlyOnceAndNextCharIsValidOrThrow(
            in SourceText source,
            ReadOnlySpan<char> text,
            SourceLocation location,
            char matchChar,
            bool matchCharAlreadySeen,
            int currentIndex,
            Func<char, bool> nextCharChecker,
            string expectedValues)
        {
            if (text[currentIndex] != matchChar)
                return false;

            if (matchCharAlreadySeen || currentIndex >= text.Length)
            {
                throw new GraphQLSyntaxException(
                    source.OffsetLocation(location, currentIndex),
                    string.Format(INVALID_NUMBER, expectedValues, text[currentIndex]));
            }

            if (nextCharChecker(text[currentIndex + 1]))
            {
                throw new GraphQLSyntaxException(
                    source.OffsetLocation(location, currentIndex + 1),
                    string.Format(INVALID_NUMBER, expectedValues, text[currentIndex + 1]));
            }

            return true;
        }

        /// <summary>
        /// Determines whether the given character is valid in a graphql name.
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns><c>true</c> if the character is valid; otherwise, <c>false</c>.</returns>
        public static bool IsValidNumberCharacter(char c)
        {
            return ParserConstants.Characters.ValidDigitChars.Span.IndexOf(c) >= 0;
        }
    }
}