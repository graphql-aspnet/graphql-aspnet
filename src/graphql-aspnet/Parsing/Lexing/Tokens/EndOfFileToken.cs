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

    /// <summary>
    /// A <see cref="LexicalToken" /> representing the end of a file or text string.
    /// </summary>
    /// <seealso cref="LexicalToken" />
    [DebuggerDisplay("Type: {TokenType}, Location: ({Location.LineNumber}:{Location.LineIndex})")]
    public class EndOfFileToken : LexicalToken
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static EndOfFileToken Instance { get; } = new EndOfFileToken();

        /// <summary>
        /// Prevents a default instance of the <see cref="EndOfFileToken"/> class from being created.
        /// </summary>
        private EndOfFileToken()
            : base(TokenType.EndOfFile)
        {
        }

        /// <summary>
        /// Gets a value indicating whether this instance should be ignored during common processing.
        /// </summary>
        /// <value><c>true</c> if this instance is ignored; otherwise, <c>false</c>.</value>
        public override bool IsIgnored => false;
    }
}