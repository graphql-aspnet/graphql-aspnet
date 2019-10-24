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
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;

    /// <summary>
    /// A token representing an as yet untyped parsed null phrase.
    /// </summary>
    /// <seealso cref="LexicalToken" />
    [DebuggerDisplay("null")]
    public class NullToken : LexicalToken
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NullToken" /> class.
        /// </summary>
        /// <param name="location">The location in the source text where this null
        /// phrase occured..</param>
        public NullToken(SourceLocation location)
            : base(TokenType.Null, ParserConstants.Keywords.Null, location)
        {
        }
    }
}