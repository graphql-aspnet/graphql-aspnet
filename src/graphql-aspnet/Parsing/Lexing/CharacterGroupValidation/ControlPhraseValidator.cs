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
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using CHARS = GraphQL.AspNet.Parsing.ParserConstants.Characters;

    /// <summary>
    /// Validation logic for validating a set of characters as a control group or not.
    /// </summary>
    public class ControlPhraseValidator
    {
        private const string INVALID_LENGTH = "Unexpected Token. Expected {0} character(s), received {1}";
        private const string INVALID_CHARACTER = "Invalid character, '{0}' is not a valid token.";

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ControlPhraseValidator Instance { get; } = new ControlPhraseValidator();

        /// <summary>
        /// Prevents a default instance of the <see cref="ControlPhraseValidator"/> class from being created.
        /// </summary>
        private ControlPhraseValidator()
        {
        }

        /// <summary>
        /// Validates the set of characters, together, form a valid string or throws an exception.
        /// </summary>
        /// <param name="phrase">The phrase to validate.</param>
        /// <param name="location">The location the source where the phrase started.</param>
        public void ValidateOrThrow(
            in ReadOnlySpan<char> phrase,
            SourceLocation location)
        {
            // must be 1 char (most tokens) or 3 chars (spread operator)
            if (phrase.Length != 1 && phrase.Length != 3)
            {
                throw new GraphQLSyntaxException(
                    location,
                    string.Format(INVALID_LENGTH, "1 or 3", phrase.Length));
            }

            // Handle spread operator as a special case since the only length 3 control token is the spead operator
            if (phrase.Length == 3)
            {
                if (!phrase.EqualsCaseInvariant(CHARS.SpreadOperator.Span))
                {
                    throw new GraphQLSyntaxException(
                        location,
                        string.Format(INVALID_CHARACTER, phrase.ToString()));
                }

                return;
            }

            // ensure its not the spread initiator when at lenth 1
            if (phrase[0] == (char)TokenType.SpreadOperatorInitiator)
            {
                throw new GraphQLSyntaxException(
                    location,
                    string.Format(INVALID_CHARACTER, phrase.ToString()));
            }

            // make sure its in the list of approved single item tokens
            if (!IsAcceptedTokenGlyph(phrase[0]))
            {
                throw new GraphQLSyntaxException(
                    location,
                    string.Format(INVALID_CHARACTER, phrase.ToString()));
            }
        }

        /// <summary>
        /// Determines whether the given text is a a valid token glyph or not.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns><c>true</c> if the supplied text constitutes a valid token glyph; otherwise, <c>false</c>.</returns>
        public static NextPhraseResult IsPossibleControlPhrase(in ReadOnlySpan<char> text)
        {
            if (text.Length >= 3)
                return NextPhraseResult.Complete;

            if (text.Length < 3 && text[0] == (char)TokenType.SpreadOperatorInitiator)
                return NextPhraseResult.Continue;

            return NextPhraseResult.Complete;
        }

        /// <summary>
        /// Determines whether the given character is a (or is a potential) glyph representing
        /// a control token.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns><c>true</c> if [is accepted token glyph] [the specified c]; otherwise, <c>false</c>.</returns>
        public static bool IsAcceptedTokenGlyph(char c)
        {
            return TokenTypeExtensions.IsControlerGlyph(c);
        }
    }
}