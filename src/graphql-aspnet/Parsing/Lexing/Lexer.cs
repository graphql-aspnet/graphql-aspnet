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
    using System;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using SR = GraphQL.AspNet.Parsing.Lexing.Source.SourceRules.GraphQLSourceRule;

    /// <summary>
    /// A tokenizer programmed with the symbols of graphql, capable of walking a string
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
            var tokenSet = new TokenStream(source.Text);

            source.SkipWhitespace();
            while (source.HasData)
            {
                SourceLocation location;

                // Comments
                // -------------------
                if (source.CheckCursor(SR.IsCommentGlyph))
                {
                    var text = source.NextComment(out location);
                    tokenSet.Enqueue(new CommentToken(text, location));
                }

                // Flow Controler characer (non-text entities)
                // -------------------
                else if (source.CheckCursor(SR.IsControlGlyph))
                {
                    var text = source.NextControlPhrase(out location);
                    tokenSet.Enqueue(new ControlToken(text.ToTokenType(), text, location));
                }

                // Named fields
                // ---------------------------------
                else if (source.CheckCursor(SR.IsStartOfNameGlyph))
                {
                    var text = source.NextName(out location);
                    tokenSet.Enqueue(Lexer.CharactersToToken(text, location));
                }

                // Numbers
                // ----------------------------------
                else if (source.CheckCursor(SR.IsStartOfNumberGlyph))
                {
                    var text = source.NextNumber(out location);
                    tokenSet.Enqueue(NumberToken.FromSourceText(text, location));
                }

                // Strings
                // ----------------------------------
                else if (source.CheckCursor(SR.IsStringDelimiterGlyph))
                {
                    var text = source.NextString(out location);
                    tokenSet.Enqueue(new StringToken(text, location));
                }
                else
                {
                    // who the heck knows, just fail
                    location = source.RetrieveCurrentLocation();
                    throw new GraphQLSyntaxException(
                        location,
                        $"Unexpected character: '{source.Peek()}'");
                }

                source.SkipWhitespace();
            }

            tokenSet.Enqueue(EndOfFileToken.Instance);
            return tokenSet;
        }

        /// <summary>
        /// Perform custom processing on any potential <see cref="NameToken" /> to determine
        /// if it should be interpreted as a special case token (e.g. "null" phrases).
        /// </summary>
        /// <param name="tokenText">The token text to convert.</param>
        /// <param name="location">The location in hte source of hte given text.</param>
        /// <returns>LexicalToken.</returns>
        private static LexicalToken CharactersToToken(ReadOnlyMemory<char> tokenText, SourceLocation location)
        {
            if (tokenText.Span.Equals(ParserConstants.Keywords.Null.Span, StringComparison.Ordinal))
            {
                return new NullToken(location);
            }
            else
            {
                return new NameToken(tokenText, location);
            }
        }
    }
}