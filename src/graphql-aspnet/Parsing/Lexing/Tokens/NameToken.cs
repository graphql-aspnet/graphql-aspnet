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
    /// A token representing the name of a field or other item.
    /// </summary>
    /// <seealso cref="LexicalToken" />
    [DebuggerDisplay("Type: {TokenType}, Value: {Text}, Location: ({Location.LineNumber}:{Location.LineIndex})")]
    public class NameToken : LexicalToken
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NameToken"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="startLocation">The start location.</param>
        public NameToken(ReadOnlyMemory<char> text, SourceLocation startLocation)
            : base(TokenType.Name, text, startLocation)
        {
        }

        /// <summary>
        /// Gets a value indicating whether this instance should be ignored during common processing.
        /// </summary>
        /// <value><c>true</c> if this instance is ignored; otherwise, <c>false</c>.</value>
        public override bool IsIgnored => false;
    }
}