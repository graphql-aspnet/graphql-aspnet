// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing2.Lexing
{
    using System;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Parsing2.Lexing.CharacterGroupValidation;
    using GraphQL.AspNet.Parsing2.Lexing.Source;
    using GraphQL.AspNet.Parsing2.Lexing.Tokens;
    using CHARS = GraphQL.AspNet.Parsing2.ParserConstants.Characters;

    /// <summary>
    /// Extension methods for custom business logic related phrase token parsing
    /// </summary>
    public static class LexerSourceExtensions
    {
        /// <summary>
        /// Retrieves the physical text pointed to by a source block definition.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="blockDefinition">The block definition.</param>
        /// <returns>System.ReadOnlySpan&lt;char&gt;.</returns>
        public static ReadOnlySpan<char> RetrieveText(this ref SourceText source, SourceTextBlockPointer blockDefinition)
        {
            if (blockDefinition.StartIndex < 0 || blockDefinition.Length <= 0)
                return ReadOnlySpan<char>.Empty;

            return source.Chars.Slice(blockDefinition.StartIndex, blockDefinition.Length);
        }

        /// <summary>
        /// Trims a single trailing carriage return if one is found.
        /// </summary>
        /// <param name="slice">The slice.</param>
        /// <returns>System.ReadOnlySpan&lt;System.Char&gt;.</returns>
        public static SourceTextBlockPointer TrimTrailingCarriageReturn(this ref SourceText source, SourceTextBlockPointer blockDefinition)
        {
            var block = source.RetrieveText(blockDefinition);
            return block.Length > 0 && block[block.Length - 1] == CHARS.CR
                ? new SourceTextBlockPointer(blockDefinition.StartIndex, blockDefinition.Length - 1)
                : blockDefinition;
        }

        /// <summary>
        /// Assumes the cursor is currently pointed at a comment
        /// and extracts the rest of the current line and all subsequent lines until
        /// a comment is no longer found.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="location">The location the comment started from.</param>
        /// <returns>A pointer to a block of text within the provided source.</returns>
        public static SourceTextBlockPointer NextComment(this ref SourceText source, out SourceLocation location)
        {
            location = source.RetrieveCurrentLocation();
            var block = source.NextLine();
            var text = source.RetrieveText(block);
            CommentPhraseValidator.Instance.ValidateOrThrow(source, text, location);
            return new SourceTextBlockPointer(location.AbsoluteIndex, text.Length);
        }

        /// <summary>
        /// Assumes the cursor is currently pointed at the start of a control token
        /// and extracts the entirity of the token regardless of length.
        /// </summary>
        /// <param name="source">The source text to read from.</param>
        /// <param name="location">The location the token started.</param>
        /// <returns>A pointer to a block of text within the provided source.</returns>
        public static SourceTextBlockPointer NextControlPhrase(this ref SourceText source, out SourceLocation location)
        {
            location = source.RetrieveCurrentLocation();
            var text = source.NextPhrase(ControlPhraseValidator.IsPossibleControlPhraseDelegate);
            ControlPhraseValidator.Instance.ValidateOrThrow(text, location);
            return new SourceTextBlockPointer(location.AbsoluteIndex, text.Length);
        }

        /// <summary>
        /// Returns all the characters from the current position up to and including an unescaped
        /// string (double quote: '"') delimiter. A double quote proceeded by a slash is skipped
        /// and included in the output string.
        /// </summary>
        /// <param name="source">The source text to read from.</param>
        /// <param name="location">The location the token started.</param>
        /// <returns>A pointer to a block of text within the provided source.</returns>
        public static SourceTextBlockPointer NextString(this ref SourceText source, out SourceLocation location)
        {
            location = source.RetrieveCurrentLocation();
            var text = source.NextPhrase(StringValidator.IsDelimitedStringDelegate);
            StringValidator.Instance.ValidateOrThrow(source, text, location);
            return new SourceTextBlockPointer(location.AbsoluteIndex, text.Length);
        }

        /// <summary>
        /// Assumes the cursor is currently pointed at the start of a name group and
        /// extracts the name up to the next white space character. An exception is thrown
        /// if any invalid characters exist in the name.
        /// </summary>
        /// <param name="source">The source text to read from.</param>
        /// <param name="location">The location the token started.</param>
        /// <returns>A pointer to a block of text within the provided source.</returns>
        public static SourceTextBlockPointer NextName(this ref SourceText source, out SourceLocation location)
        {
            location = source.RetrieveCurrentLocation();
            var text = source.NextFilter(NameValidator.IsValidNameCharacterDelegate);
            NameValidator.Instance.ValidateOrThrow(source, text, location);
            return new SourceTextBlockPointer(location.AbsoluteIndex, text.Length);
        }

        /// <summary>
        /// Assumes the cursor is pointing at the start of a number. Extracts the next group and
        /// validates it. An exception is thrown if any invalid characters exist in the number.
        /// </summary>
        /// <param name="source">The source text to read from.</param>
        /// <param name="location">The location the token started.</param>
        /// <returns>A pointer to a block of text within the provided source.</returns>
        public static SourceTextBlockPointer NextNumber(this ref SourceText source, out SourceLocation location)
        {
            location = source.RetrieveCurrentLocation();
            var text = source.NextFilter(NumberValidator.IsValidNumberCharacterDelegate);
            NumberValidator.Instance.ValidateOrThrow(source, text, location);
            return new SourceTextBlockPointer(location.AbsoluteIndex, text.Length);
        }

        /// <summary>
        /// Perform custom processing on any potential name token to determine
        /// if it should be interpreted as a special case token (e.g. "null" phrases).
        /// </summary>
        /// <param name="source">The source text containing the raw data.</param>
        /// <param name="block">The pointer to a block of text in the source.</param>
        /// <param name="location">The location in the source of hte given text.</param>
        /// <returns>LexicalToken.</returns>
        public static LexicalToken CharactersToToken(
            ref SourceText source,
            SourceTextBlockPointer block,
            SourceLocation location)
        {
            var text = source.RetrieveText(block);

            if (text.Equals(ParserConstants.Keywords.Null.Span, StringComparison.Ordinal))
            {
                return new LexicalToken(TokenType.Null, block, location);
            }
            else
            {
                return new LexicalToken(TokenType.Name, block, location);
            }
        }
    }
}