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
    /// are valid for a name in graphql
    /// Spec: https://graphql.github.io/graphql-spec/June2018/#sec-Names .
    /// </summary>
    public class NameValidator
    {
        private const string ERROR_EMPTY = "Unexpected string, expected a name, received nothing.";
        private const string INVALID_NAME = "Invalid Name. Expected '{0}', received '{1}'.";

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static NameValidator Instance { get; } = new NameValidator();

        /// <summary>
        /// Prevents a default instance of the <see cref="NameValidator"/> class from being created.
        /// </summary>
        private NameValidator()
        {
        }

        /// <summary>
        /// Validates the characters as being valid for a graphql name type or throws an exception.
        /// </summary>
        /// <param name="source">The source containing the phrase.</param>
        /// <param name="phrase">The phrase to validate.</param>
        /// <param name="location">The location the source where the phrase started.</param>
        public void ValidateOrThrow(in SourceText source, in ReadOnlySpan<char> phrase, SourceLocation location)
        {
            if (phrase.Length == 0)
                throw new GraphQLSyntaxException(location, ERROR_EMPTY);

            // must start with underscore or letter
            if (!char.IsLetter(phrase[0]) && phrase[0] != '_')
            {
                throw new GraphQLSyntaxException(
                    location,
                    string.Format(INVALID_NAME, "[_a-zA-Z]", phrase[0]));
            }

            // letters must be [_a-zA-Z0-9]
            for (var i = 1; i < phrase.Length; i++)
            {
                if (char.IsLetterOrDigit(phrase[i]))
                    continue;
                if (phrase[i] == '_')
                    continue;

                throw new GraphQLSyntaxException(
                    source.OffsetLocation(location, i),
                    string.Format(INVALID_NAME, "[_a-zA-Z]", phrase[i]));
            }
        }

        /// <summary>
        /// Determines whether the given character is valid in a graphql name..
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns><c>true</c> if the character is valid; otherwise, <c>false</c>.</returns>
        public static bool IsValidNameCharacter(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }
    }
}