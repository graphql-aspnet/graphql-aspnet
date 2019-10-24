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
    /// A token representing a string of arbitrary data.
    /// </summary>
    /// <seealso cref="LexicalToken" />
    [DebuggerDisplay("Type: {TokenType}, Length: {Text.Length}, Location: ({Location.LineNumber}:{Location.LineIndex})")]
    public class StringToken : LexicalToken
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringToken"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="location">The location.</param>
        public StringToken(ReadOnlyMemory<char> text, SourceLocation location)
            : base(TokenType.String, text, location)
        {
        }

        /// <summary>
        /// Gets a value indicating whether this instance should be ignored during common processing.
        /// </summary>
        /// <value><c>true</c> if this instance is ignored; otherwise, <c>false</c>.</value>
        public override bool IsIgnored => false;
    }
}