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
    using GraphQL.AspNet.Common.Source;

    /// <summary>
    /// A token representing a control flow glyph of arbitrary length.
    /// </summary>
    /// <seealso cref="LexicalToken" />
    [DebuggerDisplay("Type: {TokenType}, Location: ({Location.LineNumber}:{Location.LineIndex})")]
    public class ControlToken : LexicalToken
    {
        /// <summary>
        /// Creates a valid control token from teh given type.
        /// </summary>
        /// <param name="tokenType">Type of the token.</param>
        /// <param name="location">The location.</param>
        /// <returns>ControlToken.</returns>
        public static ControlToken FromType(TokenType tokenType, SourceLocation location)
        {
            return new ControlToken(tokenType, tokenType.Description(), location);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlToken"/> class.
        /// </summary>
        /// <param name="tokenType">Type of the token.</param>
        /// <param name="text">The text.</param>
        /// <param name="location">The location.</param>
        public ControlToken(TokenType tokenType, ReadOnlyMemory<char> text, SourceLocation location)
            : base(tokenType, text, location)
        {
        }

        /// <summary>
        /// Gets a value indicating whether this instance should be ignored during common processing.
        /// </summary>
        /// <value><c>true</c> if this instance is ignored; otherwise, <c>false</c>.</value>
        public override bool IsIgnored
        {
            get
            {
                switch (this.TokenType)
                {
                    case TokenType.Comma:
                        return true;
                    default:
                        return false;
                }
            }
        }
    }
}