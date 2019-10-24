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
    /// A token representing a string of comment characters, including the leading control character.
    /// </summary>
    /// <seealso cref="LexicalToken" />
    [DebuggerDisplay("Type: {TokenType}, Location: ({Location.LineNumber}:{Location.LineIndex})")]
    public class CommentToken : LexicalToken
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommentToken" /> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="startLocation">The start location.</param>
        public CommentToken(ReadOnlyMemory<char> text, SourceLocation startLocation)
            : base(TokenType.Comment, text, startLocation)
        {
        }

        /// <summary>
        /// Gets a value indicating whether this instance should be ignored during common processing.
        /// </summary>
        /// <value><c>true</c> if this instance is ignored; otherwise, <c>false</c>.</value>
        public override bool IsIgnored => true;
    }
}