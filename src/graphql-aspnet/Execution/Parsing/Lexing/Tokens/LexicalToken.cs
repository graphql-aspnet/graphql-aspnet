// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Parsing.Lexing.Tokens
{
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;

    /// <summary>
    /// A lexical token, representing a meaningful character or set of characters
    /// extracted from the raw query text.
    /// </summary>
    [DebuggerDisplay("{TokenType}")]
    internal struct LexicalToken
    {
        /// <summary>
        /// Gets a token that indicates the end of a file.
        /// </summary>
        /// <value>The eo f.</value>
        public static LexicalToken EoF { get; } = new LexicalToken(
            TokenType.EndOfFile,
            SourceTextBlockPointer.None,
            SourceLocation.None,
            true);

        /// <summary>
        /// Initializes a new instance of the <see cref="LexicalToken" /> struct.
        /// </summary>
        /// <param name="tokenType">Type of the token.</param>
        /// <param name="block">A pointer to the block of text this token represents, in the related source.</param>
        /// <param name="location">The location in the source document where the token occured.</param>
        /// <param name="isIgnored">if set to <c>true</c> this token will be ignored
        /// during common processing.</param>
        public LexicalToken(
            TokenType tokenType,
            SourceTextBlockPointer block,
            SourceLocation location,
            bool isIgnored = false)
        {
            this.Location = Validation.ThrowIfNullOrReturn(location, nameof(location));
            this.TokenType = tokenType;
            this.Block = block;
            this.IsIgnored = isIgnored;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LexicalToken" /> struct.
        /// </summary>
        /// <param name="tokenType">Type of the token.</param>
        /// <param name="isIgnored">if set to <c>true</c> this token will be ignored
        /// during common processing.</param>
        public LexicalToken(
            TokenType tokenType,
            bool isIgnored = false)
            : this(tokenType, SourceTextBlockPointer.None, SourceLocation.None, isIgnored)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LexicalToken" /> struct.
        /// </summary>
        /// <param name="tokenType">Type of the token.</param>
        /// <param name="location">The location in the source document where the token occured.</param>
        /// <param name="isIgnored">if set to <c>true</c> this token will be ignored
        /// during common processing.</param>
        public LexicalToken(
            TokenType tokenType,
            SourceLocation location,
            bool isIgnored = false)
            : this(tokenType, SourceTextBlockPointer.None, location, isIgnored)
        {
        }

        /// <summary>
        /// Gets the type of the token.
        /// </summary>
        /// <value>The type of the token.</value>
        public TokenType TokenType { get; }

        /// <summary>
        /// Gets the location.
        /// </summary>
        /// <value>The location.</value>
        public SourceLocation Location { get; }

        /// <summary>
        /// Gets a pointer to a block of text, in the related source text, that represents this token.
        /// </summary>
        /// <value>The text.</value>
        public SourceTextBlockPointer Block { get; }

        /// <summary>
        /// Gets a value indicating whether this token should be ignored during common processing.
        /// </summary>
        /// <value><c>true</c> if this instance is ignored; otherwise, <c>false</c>.</value>
        public bool IsIgnored { get; }
    }
}