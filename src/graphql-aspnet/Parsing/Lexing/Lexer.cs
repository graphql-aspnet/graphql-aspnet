// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.Lexing
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;

    /// <summary>
    /// A tokenizer programmed with the symbols of graphql, capable of walking a string,
    /// splitting it into logical segments, tracking read position, performing look aheads etc.
    /// </summary>
    public static class Lexer
    {
        /// <summary>
        /// Converts a given source text into a stream of tokens that can be consumed to create a syntax tree.
        /// </summary>
        /// <param name="source">The source text to analyze.</param>
        /// <returns>TokenStream.</returns>
        public static TokenStream Tokenize(SourceText source)
        {
            return new TokenStream(source);
        }

        /// <summary>
        /// Reads and consumes the entire token stream and creates a list of tokens.
        /// </summary>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="skipIgnored">if set to <c>true</c> any tokens deemed as "ignorable" according
        /// to the graphql spec are automatically skipped over. This includes token such as whitespace, commas and comments.</param>
        /// <returns>List&lt;LexToken&gt;.</returns>
        public static List<LexicalToken> ToList(this TokenStream tokenStream, bool skipIgnored = true)
        {
            var list = new List<LexicalToken>();
            if (tokenStream.TokenType == TokenType.StartOfFile)
                tokenStream.Next(skipIgnored);

            list.Add(tokenStream.ActiveToken);

            while (tokenStream.TokenType != TokenType.EndOfFile)
            {
                tokenStream.Next(skipIgnored);
                list.Add(tokenStream.ActiveToken);
            }

            return list;
        }
    }
}