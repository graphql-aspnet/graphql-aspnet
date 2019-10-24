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
    using CHARS = GraphQL.AspNet.Parsing.ParserConstants.Characters;

    /// <summary>
    /// A token representing one of two number types supported by graph ql (int or float).
    /// </summary>
    /// <seealso cref="LexicalToken" />
    [DebuggerDisplay("Type: {TokenType}, Value: {Text}, Location: ({Location.LineNumber}:{Location.LineIndex})")]
    public class NumberToken : LexicalToken
    {
        /// <summary>
        /// Creates a number token of the right type (float vs integer)
        /// based on the provided text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="startLocation">The start location.</param>
        /// <returns>GraphQL.AspNet.Parser.Lexing.Tokens.NumberToken.</returns>
        public static NumberToken FromSourceText(ReadOnlyMemory<char> text, SourceLocation startLocation)
        {
            return new NumberToken(
                text.Span.IndexOfAny(CHARS.FloatIndicatorChars.Span) >= 0 ? TokenType.Float : TokenType.Integer,
                text,
                startLocation);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberToken"/> class.
        /// </summary>
        /// <param name="tokenType">Type of the token.</param>
        /// <param name="text">The text that makes up the number.</param>
        /// <param name="startLocation">The start location.</param>
        private NumberToken(TokenType tokenType, ReadOnlyMemory<char> text, SourceLocation startLocation)
         : base(tokenType, text, startLocation)
        {
        }

        /// <summary>
        /// Gets a value indicating whether this instance should be ignored during common processing.
        /// </summary>
        /// <value><c>true</c> if this instance is ignored; otherwise, <c>false</c>.</value>
        public override bool IsIgnored => false;
    }
}