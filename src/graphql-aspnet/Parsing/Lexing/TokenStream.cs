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
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;

    /// <summary>
    /// A managed collection of parsed <see cref="LexicalToken"/>. Keeps a pointer to the
    /// token in scope for analysis and provides easy access methods for checking its properties without actually
    /// stripping it away from the stream.
    /// </summary>
    [DebuggerDisplay("Active = {ActiveToken.TokenType}, Count = {Count}")]
    public class TokenStream : IEnumerable<LexicalToken>
    {
        private readonly Queue<LexicalToken> _tokenQueue = new Queue<LexicalToken>();

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
        public void Enqueue(LexicalToken token)
        {
            _tokenQueue.Enqueue(token);
        }

        /// <summary>
        /// Primes the stream ensuring that a token is available for analysis if one exists. Will return
        /// null if no tokens are found.
        /// </summary>
        /// <returns>LexicalToken.</returns>
        public LexicalToken Prime()
        {
            return this.ActiveToken ?? this.Next();
        }

        /// <summary>
        /// Advances this stream to the next token in and returns a usable reference to it. If no more tokens
        /// remain, null is returned.
        /// </summary>
        /// <param name="skipIgnored">if set to <c>true</c> any tokens deemed as "ignorable" according
        /// to the graphql spec are automatically skipped over. This includes token such as whitespace, commas and comments.</param>
        /// <returns>LexicalToken.</returns>
        public LexicalToken Next(bool skipIgnored = true)
        {
            this.ActiveToken = _tokenQueue.Count > 0 ? _tokenQueue.Dequeue() : null;
            if (skipIgnored)
            {
                while (this.ActiveToken != null && this.ActiveToken.IsIgnored)
                {
                    this.ActiveToken = _tokenQueue.Count > 0 ? _tokenQueue.Dequeue() : null;
                }
            }

            return this.ActiveToken;
        }

        /// <summary>
        /// Attempts to match the given text values against the active token if its a <see cref="NameToken"/>.
        /// Will always return false if the active token is not a <see cref="NameToken"/>.
        /// </summary>
        /// <param name="textValues">The text values.</param>
        /// <returns><c>true</c> if the token text matches a given text value, <c>false</c> otherwise.</returns>
        public bool Match(params ReadOnlyMemory<char>[] textValues)
        {
            if (this.ActiveToken == null)
                return false;

            if (this.ActiveToken is NameToken nt)
            {
                foreach (var text in textValues)
                {
                    if (nt.Text.Span.Equals(text.Span, StringComparison.Ordinal))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Peeks at the next token on the queue.
        /// </summary>
        /// <typeparam name="TLexicalType">The expected type of the token next in the queue.</typeparam>
        /// <returns><c>true</c> if the token is of the give type, <c>false</c> otherwise.</returns>
        public bool Match<TLexicalType>()
        where TLexicalType : LexicalToken
        {
            if (this.ActiveToken == null)
                return false;

            return this.ActiveToken is TLexicalType;
        }

        /// <summary>
        /// Matches the currently <see cref="ActiveToken" /> to any of the provided <see cref="TokenType" />. The active
        /// token must match one of the provided types for the match to succeed.
        /// </summary>
        /// <param name="primaryTokenType">Type of the primary token to check for.</param>
        /// <param name="additionalTypes">Any additional tokens to check for.</param>
        /// <returns><c>true</c> if a match is found, <c>false</c> otherwise.</returns>
        public bool Match(TokenType primaryTokenType, params TokenType[] additionalTypes)
        {
            if (additionalTypes == null || additionalTypes.Length == 0)
            {
                return this.Match(primaryTokenType);
            }
            else
            {
                return primaryTokenType.AsEnumerable().Concat(additionalTypes).Any(this.Match);
            }
        }

        /// <summary>
        /// Peeks at the next token on the queue to see if its type matches that provided.
        /// </summary>
        /// <param name="tokenType">The type to check the top of the queue froro.</param>
        /// <returns><c>true</c> if the token type matches, <c>false</c> otherwise.</returns>
        public bool Match(TokenType tokenType)
        {
            if (this.ActiveToken == null)
                return false;

            return this.ActiveToken.TokenType == tokenType;
        }

        /// <summary>
        /// Matches the currently <see cref="ActiveToken" /> to any of the provided <see cref="TokenType" />.
        /// The active token must match one of the provided values or an exception is thrown.
        /// </summary>
        /// <param name="primaryTokenType">Type of the primary token to check for.</param>
        /// <param name="additionalTypes">Any additional tokens to check for.</param>
        public void MatchOrThrow(TokenType primaryTokenType, params TokenType[] additionalTypes)
        {
            if (additionalTypes == null || additionalTypes.Length == 0)
            {
                this.MatchOrThrow(primaryTokenType);
            }
            else
            {
                var typeCollection = primaryTokenType.AsEnumerable().Concat(additionalTypes);
                var tokensMatched = typeCollection.Where(this.Match);
                if (!tokensMatched.Any())
                {
                    var errors = "[" + string.Join(", ", typeCollection.Select(TokenTypeTextForErrorMessage).Select(x => x.ToString())) + "]";
                    GraphQLSyntaxException.ThrowFromExpectation(
                        this.Location,
                        errors,
                        this.ActiveTokenTypeDescrption.ToString());
                }
            }
        }

        /// <summary>
        /// Matches the currently <see cref="ActiveToken"/> to the provided <see cref="TokenType"/>
        /// or throws an exception.
        /// </summary>
        /// <param name="tokenType">Type of the token.</param>
        public void MatchOrThrow(TokenType tokenType)
        {
            if (!this.Match(tokenType))
            {
                GraphQLSyntaxException.ThrowFromExpectation(
                    this.Location,
                    tokenType.Description(),
                    this.ActiveTokenTypeDescrption);
            }
        }

        /// <summary>
        /// Matches the type of the <see cref="ActiveToken"/> to the provided type or throws
        /// an exception.
        /// </summary>
        /// <typeparam name="TLexicalType">The type of the t lexical type.</typeparam>
        public void MatchOrThrow<TLexicalType>()
            where TLexicalType : LexicalToken
        {
            if (!this.Match<TLexicalType>())
            {
                var text = this.EndOfStream
                    ? TokenType.EndOfFile.Description().ToString()
                    : this.ActiveToken.GetType().FriendlyName();

                GraphQLSyntaxException.ThrowFromExpectation(
                    this.Location,
                    typeof(TLexicalType).FriendlyName(),
                    text);
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
                    this.ActiveToken?.Text ?? ReadOnlyMemory<char>.Empty);
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
        public LexicalToken ActiveToken { get; private set; }

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

                if (this.ActiveToken == null)
                    return ParserConstants.Keywords.Null;

                return this.TokenTypeTextForErrorMessage(this.ActiveToken.TokenType);
            }
        }

        /// <summary>
        /// Gets a reference to the location pointed to by this stream; the <see cref="SourceLocation"/>
        /// of the <see cref="ActiveToken"/>. Returns <see cref="SourceLocation.None"/> if no token is active.
        /// </summary>
        /// <value>The location.</value>
        public SourceLocation Location => this.ActiveToken?.Location ?? SourceLocation.None;

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
                    return this.ActiveToken == null || this.ActiveToken.TokenType == TokenType.EndOfFile;
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
        public TokenType TokenType => this.ActiveToken?.TokenType ?? TokenType.EndOfFile;

        /// <summary>
        /// Gets the source text which provides the tokens in this stream.
        /// </summary>
        /// <value>The source text.</value>
        public ReadOnlyMemory<char> SourceText { get; }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<LexicalToken> GetEnumerator()
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