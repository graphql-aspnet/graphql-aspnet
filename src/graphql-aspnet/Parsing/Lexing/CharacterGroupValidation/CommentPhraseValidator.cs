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
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using CHARS = GraphQL.AspNet.Parsing.ParserConstants.Characters;

    /// <summary>
    /// Validates that a group of characters conforms the specification for a comment
    /// Spec: https://graphql.github.io/graphql-spec/June2018/#sec-Comments.
    /// </summary>
    public class CommentPhraseValidator
    {
        private const string INVALID_COMMENT = "Invalid Comment. Expected '{0}', recieved '{1}'";
        private const string INVALID_COMMENT_NEWLINE = "Invalid Comment. New lines '\\n' is not allowed.";

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static CommentPhraseValidator Instance { get; } = new CommentPhraseValidator();

        /// <summary>
        /// Prevents a default instance of the <see cref="CommentPhraseValidator"/> class from being created.
        /// </summary>
        private CommentPhraseValidator()
        {
        }

        /// <summary>
        /// Validates the set of characters, together, form a valid comment or throws an exception.
        /// </summary>
        /// <param name="source">The source containing the phrase.</param>
        /// <param name="text">The phrase to validate.</param>
        /// <param name="location">The location the source where the phrase started.</param>
        public void ValidateOrThrow(
            in SourceText source,
            in ReadOnlySpan<char> text,
            SourceLocation location)
        {
            // must start with a '#'
            if (text.Length < 1)
            {
                throw new GraphQLSyntaxException(
                    location,
                    string.Format(INVALID_COMMENT, "#", "{nothing}"));
            }

            if (text[0] != (char)TokenType.Comment)
            {
                throw new GraphQLSyntaxException(
                    location,
                    string.Format(INVALID_COMMENT, "#", text[0]));
            }

            // graphql only supports single line comments
            var index = text.IndexOf(CHARS.NL);
            if (index >= 0)
            {
                throw new GraphQLSyntaxException(
                    source.OffsetLocation(location, index),
                    string.Format(INVALID_COMMENT_NEWLINE));
            }
        }
    }
}