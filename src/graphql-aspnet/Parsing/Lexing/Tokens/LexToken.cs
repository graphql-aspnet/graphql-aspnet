// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.Lexing.Tokens
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Source;

    /// <summary>
    /// A lexical token, representing a meaningful character or set of characters
    /// extracted from the raw query text.
    /// </summary>
    [DebuggerDisplay("{TokenType}")]
    public struct LexToken
    {
        /// <summary>
        /// Gets an empty lexical token, it contains no useful information.
        /// </summary>
        /// <value>The empty.</value>
        public static LexToken Empty { get; } = new LexToken(TokenType.None, ReadOnlyMemory<char>.Empty, SourceLocation.None, true);

        /// <summary>
        /// Gets a token that indicates the end of a file.
        /// </summary>
        /// <value>The eo f.</value>
        public static LexToken EoF { get; } = new LexToken(TokenType.EndOfFile, ReadOnlyMemory<char>.Empty, SourceLocation.None, true);

        /// <summary>
        /// Initializes a new instance of the <see cref="LexToken" /> struct.
        /// </summary>
        /// <param name="tokenType">Type of the token.</param>
        /// <param name="text">The text included in hte token.</param>
        /// <param name="location">The location in the source document where the token occured.</param>
        /// <param name="isIgnored">if set to <c>true</c> this token will be ignored
        /// during common processing.</param>
        public LexToken(
            TokenType tokenType,
            ReadOnlyMemory<char> text,
            SourceLocation location,
            bool isIgnored = false)
        {
            this.Location = Validation.ThrowIfNullOrReturn(location, nameof(location));
            this.TokenType = tokenType;
            this.Text = text;
            this.IsIgnored = isIgnored;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LexToken" /> struct.
        /// </summary>
        /// <param name="tokenType">Type of the token.</param>
        /// <param name="isIgnored">if set to <c>true</c> this token will be ignored
        /// during common processing.</param>
        public LexToken(
            TokenType tokenType,
            bool isIgnored = false)
        {
            this.TokenType = tokenType;
            this.Location = SourceLocation.None;
            this.Text = null;
            this.IsIgnored = isIgnored;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LexToken" /> struct.
        /// </summary>
        /// <param name="tokenType">Type of the token.</param>
        /// <param name="location">The location in the source document where the token occured.</param>
        /// <param name="isIgnored">if set to <c>true</c> this token will be ignored
        /// during common processing.</param>
        public LexToken(
            TokenType tokenType,
            SourceLocation location,
            bool isIgnored = false)
        {
            this.TokenType = tokenType;
            this.Location = location;
            this.Text = null;
            this.IsIgnored = isIgnored;
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
        /// Gets the text that was parsed into the token.
        /// </summary>
        /// <value>The text.</value>
        public ReadOnlyMemory<char> Text { get; }

        /// <summary>
        /// Gets a value indicating whether this token should be ignored during common processing.
        /// </summary>
        /// <value><c>true</c> if this instance is ignored; otherwise, <c>false</c>.</value>
        public bool IsIgnored { get; }
    }
}