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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;

    /// <summary>
    /// A managed collection of parsed <see cref="LexToken"/> items. Keeps a pointer to the
    /// token in scope for analysis and provides easy access methods for checking its properties without actually
    /// stripping it away from the stream.
    /// </summary>
    [DebuggerDisplay("Active = {ActiveToken.TokenType}, Count = {Count}")]
    public class TokenStream : IEnumerable<LexToken>
    {
        private readonly Queue<LexToken> _tokenQueue = new Queue<LexToken>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenStream"/> class.
        /// </summary>
        /// <param name="sourceText">The source text.</param>
        public TokenStream(ReadOnlyMemory<char> sourceText)
        {
            this.SourceText = sourceText;
        }

        /// <summary>
        /// Enqueues the specified token into the stream in the order received.
        /// </summary>
        /// <param name="token">The token.</param>
        public void Enqueue(LexToken token)
        {
            _tokenQueue.Enqueue(token);
        }

        /// <summary>
        /// Primes the stream ensuring that a token is available for analysis if one exists. Will return
        /// null if no tokens are found.
        /// </summary>
        /// <returns>LexicalToken.</returns>
        public LexToken Prime()
        {
            if (this.ActiveToken.TokenType == TokenType.None)
                return this.Next();

            return this.ActiveToken;
        }

        /// <summary>
        /// Advances this stream to the next token in and returns a usable reference to it. If no more tokens
        /// remain, null is returned.
        /// </summary>
        /// <param name="skipIgnored">if set to <c>true</c> any tokens deemed as "ignorable" according
        /// to the graphql spec are automatically skipped over. This includes token such as whitespace, commas and comments.</param>
        /// <returns>LexicalToken.</returns>
        public LexToken Next(bool skipIgnored = true)
        {
            this.ActiveToken = _tokenQueue.Count > 0 ? _tokenQueue.Dequeue() : LexToken.Empty;
            if (skipIgnored && this.ActiveToken.IsIgnored)
            {
                do
                {
                    this.ActiveToken = _tokenQueue.Count > 0 ? _tokenQueue.Dequeue() : LexToken.Empty;
                }
                while (this.ActiveToken.IsIgnored && _tokenQueue.Count > 0);
            }

            return this.ActiveToken;
        }

        /// <summary>
        /// Attempts to match the given text values against the active token if its a name token.
        /// Will always return false if the active token is not a name token.
        /// </summary>
        /// <param name="textValues">The text values.</param>
        /// <returns><c>true</c> if the token text matches a given text value, <c>false</c> otherwise.</returns>
        public bool Match(params ReadOnlyMemory<char>[] textValues)
        {
            if (this.ActiveToken.TokenType == TokenType.Name)
            {
                foreach (var text in textValues)
                {
                    if (this.ActiveToken.Text.Span.Equals(text.Span, StringComparison.Ordinal))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Peeks at the next token on the queue to see if its type matches that provided.
        /// </summary>
        /// <param name="tokenType">The type to check the top of the queue froro.</param>
        /// <param name="otherType">Another type of token to check, if any.</param>
        /// <returns><c>true</c> if the token type matches, <c>false</c> otherwise.</returns>
        public bool Match(TokenType tokenType, TokenType? otherType = null)
        {
            return this.ActiveToken.TokenType == tokenType
                || (otherType.HasValue && this.ActiveToken.TokenType == otherType.Value);
        }

        /// <summary>
        /// Matches the currently <see cref="ActiveToken" /> to the provided <see cref="TokenType" />
        /// or throws an exception.
        /// </summary>
        /// <param name="tokenType">Type of the token.</param>
        /// <param name="otherType">Another type of token to check, if any.</param>
        public void MatchOrThrow(TokenType tokenType, TokenType? otherType = null)
        {
            if (!this.Match(tokenType, otherType))
            {
                GraphQLSyntaxException.ThrowFromExpectation(
                    this.Location,
                    tokenType.Description(),
                    this.ActiveTokenTypeDescrption);
            }
        }

        /// <summary>
        /// Matchs the text of the currnetly <see cref="ActiveToken"/> to the given keyword
        /// or throws an exception.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        public void MatchOrThrow(ReadOnlyMemory<char> keyword)
        {
            if (!this.Match(keyword))
            {
                GraphQLSyntaxException.ThrowFromExpectation(
                    this.Location,
                    keyword,
                    this.ActiveToken.Text);
            }
        }

        /// <summary>
        /// Creates a block of text that can be used to describe the token type in an error message.
        /// </summary>
        /// <param name="tokenType">Type of the token.</param>
        /// <returns>ReadOnlyMemory&lt;System.Char&gt;.</returns>
        private ReadOnlyMemory<char> TokenTypeTextForErrorMessage(TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.Name:
                case TokenType.Float:
                case TokenType.Integer:
                case TokenType.String:
                    return this.ActiveToken.Text;

                default:
                    return tokenType.Description();
            }
        }

        /// <summary>
        /// Gets a reference to the token in scope for analysis.
        /// </summary>
        /// <value>The active token.</value>
        public LexToken ActiveToken { get; private set; }

        /// <summary>
        /// Gets the active token type descrption that can be used in error messages.
        /// </summary>
        /// <value>The active token type descrption.</value>
        private ReadOnlyMemory<char> ActiveTokenTypeDescrption
        {
            get
            {
                if (this.EndOfStream)
                    return TokenType.EndOfFile.Description();

                if (this.ActiveToken.TokenType == TokenType.None)
                    return ParserConstants.Keywords.Null;

                return this.TokenTypeTextForErrorMessage(this.ActiveToken.TokenType);
            }
        }

        /// <summary>
        /// Gets a reference to the location pointed to by this stream; the <see cref="SourceLocation"/>
        /// of the <see cref="ActiveToken"/>. Returns <see cref="SourceLocation.None"/> if no token is active.
        /// </summary>
        /// <value>The location.</value>
        public SourceLocation Location
        {
            get
            {
                if (this.ActiveToken.TokenType == TokenType.None)
                    return SourceLocation.None;
                else
                    return this.ActiveToken.Location;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the stream is flushed (no tokens left to process)
        /// or the active token is pointing at either an end of stream marker or nothing.
        /// </summary>
        /// <value><c>true</c> if at the end of stream; otherwise, <c>false</c>.</value>
        public bool EndOfStream
        {
            get
            {
                if (_tokenQueue.Count == 0)
                {
                    return this.ActiveToken.TokenType == TokenType.None
                        || this.ActiveToken.TokenType == TokenType.EndOfFile;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets the number of tokens currently queued in the stream.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _tokenQueue.Count;

        /// <summary>
        /// Gets the <see cref="TokenType"/> of the active token on the stream or EOF
        /// if no active token.
        /// </summary>
        /// <value>the <see cref="TokenType"/> at the top of the stream.</value>
        public TokenType TokenType
        {
            get
            {
                if (this.ActiveToken.TokenType == TokenType.None)
                    return TokenType.EndOfFile;

                return this.ActiveToken.TokenType;
            }
        }

        /// <summary>
        /// Gets the source text which provides the tokens in this stream.
        /// </summary>
        /// <value>The source text.</value>
        public ReadOnlyMemory<char> SourceText { get; }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<LexToken> GetEnumerator()
        {
            return _tokenQueue.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}