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
    /// A represention of a string of characters in a source document that, together make up a single,
    /// logical segment of the document.
    /// </summary>
    [DebuggerDisplay("Type: {TokenType}, Location: ({Location.LineNumber}:{Location.LineIndex})")]
    public abstract class LexicalToken
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LexicalToken" /> class.
        /// </summary>
        /// <param name="tokenType">Type of the token.</param>
        /// <param name="text">The text.</param>
        /// <param name="location">The location.</param>
        protected LexicalToken(TokenType tokenType, ReadOnlyMemory<char> text, SourceLocation location)
        {
            this.Location = Validation.ThrowIfNullOrReturn(location, nameof(location));
            this.TokenType = tokenType;
            this.Text = text;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LexicalToken"/> class.
        /// </summary>
        /// <param name="tokenType">Type of the token.</param>
        protected LexicalToken(TokenType tokenType)
        {
            this.TokenType = tokenType;
            this.Location = SourceLocation.None;
            this.Text = null;
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
        /// Gets a value indicating whether this instance should be ignored during common processing.
        /// </summary>
        /// <value><c>true</c> if this instance is ignored; otherwise, <c>false</c>.</value>
        public virtual bool IsIgnored => false;
    }
}