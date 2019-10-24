// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.Lexing
{
    using System;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Parsing.Lexing.CharacterGroupValidation;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using CHARS = GraphQL.AspNet.Parsing.ParserConstants.Characters;

    /// <summary>
    /// Extension methods for custom business logic related phrase token parsing..
    /// </summary>
    public static class LexerSourceExtensions
    {
        /// <summary>
        /// Trims a single trailing carriage return if one is found.
        /// </summary>
        /// <param name="slice">The slice.</param>
        /// <returns>System.ReadOnlySpan&lt;System.Char&gt;.</returns>
        public static ReadOnlySpan<char> TrimTrailingCarriageReturn(this in ReadOnlySpan<char> slice)
        {
            return slice.Length > 0 && slice[slice.Length - 1] == CHARS.CR
                ? slice.Slice(0, slice.Length - 1)
                : slice;
        }

        /// <summary>
        /// Assumes the cursor is currnetly pointed at a comment
        /// and extracts the rest of the current line and all subsequent lines until
        /// a non-comment is found
        /// comment is no longer found.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="location">The location the comment started from.</param>
        /// <returns>ReadOnlySpan&lt;System.Char&gt;.</returns>
        public static ReadOnlyMemory<char> NextComment(this ref SourceText source, out SourceLocation location)
        {
            location = source.RetrieveCurrentLocation();
            var text = source.NextLine();
            CommentPhraseValidator.Instance.ValidateOrThrow(source, text, location);
            return source.SliceMemory(location.AbsoluteIndex, text.Length);
        }

        /// <summary>
        /// Assumes the cursor is currently pointed at the start of a control token
        /// and extracts the entirity of the token regardless of length.
        /// </summary>
        /// <param name="source">The source text to read from.</param>
        /// <param name="location">The location the token started.</param>
        /// <returns>System.ReadOnlySpan&lt;System.Char&gt;.</returns>
        public static ReadOnlyMemory<char> NextControlPhrase(this ref SourceText source, out SourceLocation location)
        {
            location = source.RetrieveCurrentLocation();
            var text = source.NextPhrase(ControlPhraseValidator.IsPossibleControlPhrase);
            ControlPhraseValidator.Instance.ValidateOrThrow(text, location);
            return source.SliceMemory(location.AbsoluteIndex, text.Length);
        }

        /// <summary>
        /// Returns all the characters from the current position up to and including an unescaped
        /// string (double quote: '"') delimiter. A double quote proceeded by a slash is skipped
        /// and included in the output string.
        /// </summary>
        /// <param name="source">The source text to read from.</param>
        /// <param name="location">The location the token started.</param>
        /// <returns>System.ReadOnlySpan&lt;System.Char&gt;.</returns>
        public static ReadOnlyMemory<char> NextString(this ref SourceText source, out SourceLocation location)
        {
            location = source.RetrieveCurrentLocation();
            var text = source.NextPhrase(StringValidator.IsDelimitedString);
            StringValidator.Instance.ValidateOrThrow(source, text, location);
            return source.SliceMemory(location.AbsoluteIndex, text.Length);
        }

        /// <summary>
        /// Assumes the cursor is currently pointed at the start of a name group and
        /// extracts the name up to the next white space character. An exception is thrown
        /// if any invalid characters exist in the name.
        /// </summary>
        /// <param name="source">The source text to read from.</param>
        /// <param name="location">The location the token started.</param>
        /// <returns>System.ReadOnlySpan&lt;System.Char&gt;.</returns>
        public static ReadOnlyMemory<char> NextName(this ref SourceText source, out SourceLocation location)
        {
            location = source.RetrieveCurrentLocation();
            var text = source.NextFilter(NameValidator.IsValidNameCharacter);
            NameValidator.Instance.ValidateOrThrow(source, text, location);
            return source.SliceMemory(location.AbsoluteIndex, text.Length);
        }

        /// <summary>
        /// Assumes the cursor is pointing at the start of a number. Extracts the next group and
        /// validates it. An exception is thrown if any invalid characters exist in the number.
        /// </summary>
        /// <param name="source">The source text to read from.</param>
        /// <param name="location">The location the token started.</param>
        /// <returns>System.ReadOnlySpan&lt;System.Char&gt;.</returns>
        public static ReadOnlyMemory<char> NextNumber(this ref SourceText source, out SourceLocation location)
        {
            location = source.RetrieveCurrentLocation();
            var text = source.NextFilter(NumberValidator.IsValidNumberCharacter);
            NumberValidator.Instance.ValidateOrThrow(source, text, location);
            return source.SliceMemory(location.AbsoluteIndex, text.Length);
        }
    }
}