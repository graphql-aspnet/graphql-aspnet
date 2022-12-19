// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Parsing.Lexing
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution.Parsing.Exceptions;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Tokens;
    using CHARS = GraphQL.AspNet.Execution.Parsing.ParserConstants.Characters;
    using SR = GraphQL.AspNet.Execution.Parsing.Lexing.Source.SourceRules.GraphQLSourceRule;

    /// <summary>
    /// A continuous flow of parsed <see cref="LexicalToken"/> items. Keeps a pointer to the
    /// token in scope for analysis and provides easy access methods for checking its properties without actually
    /// stripping it away from the stream.
    /// </summary>
    [DebuggerDisplay("Active = {ActiveToken.TokenType}")]
    internal ref struct TokenStream
    {
        private SourceText _sourceText;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenStream"/> struct.
        /// </summary>
        /// <param name="sourceText">The source text.</param>
        public TokenStream(SourceText sourceText)
        {
            _sourceText = sourceText;
            this.ActiveToken = new LexicalToken(TokenType.StartOfFile, false);
        }

        /// <summary>
        /// Primes the stream ensuring that a token is available for analysis if one exists. Will return
        /// null if no tokens are found.
        /// </summary>
        /// <param name="skipIgnored">if set to <c>true</c> any tokens deemed as "ignorable" according
        /// to the graphql spec are automatically skipped over. This includes token such as whitespace, commas and comments.</param>
        /// <returns>LexicalToken.</returns>
        public LexicalToken Prime(bool skipIgnored = true)
        {
            if (this.ActiveToken.TokenType == TokenType.StartOfFile)
                return this.Next(skipIgnored);

            return this.ActiveToken;
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
            this.ActiveToken = this.FetchNextTokenFromStream();
            if (skipIgnored && this.ActiveToken.IsIgnored)
            {
                do
                {
                    this.ActiveToken = this.FetchNextTokenFromStream();
                }
                while (this.ActiveToken.IsIgnored && this.ActiveToken.TokenType != TokenType.EndOfFile);
            }

            return this.ActiveToken;
        }

        /// <summary>
        /// Attempts to match the given text values against the active token if its a name token.
        /// Will always return false if the active token is not a name token.
        /// </summary>
        /// <param name="textToMatch">The text value to match against.</param>
        /// <returns><c>true</c> if the token text matches a given text value, <c>false</c> otherwise.</returns>
        public bool Match(ReadOnlySpan<char> textToMatch)
        {
            if (this.ActiveToken.TokenType == TokenType.Name)
            {
                var actualText = _sourceText.Slice(this.ActiveToken.Block);
                if (actualText.Equals(textToMatch, StringComparison.Ordinal))
                    return true;
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
                    tokenType.Description().Span,
                    this.ActiveTokenTypeDescrption);
            }
        }

        /// <summary>
        /// Matchs the text of the currnetly <see cref="ActiveToken"/> to the given keyword
        /// or throws an exception.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        public void MatchOrThrow(ReadOnlySpan<char> keyword)
        {
            if (!this.Match(keyword))
            {
                GraphQLSyntaxException.ThrowFromExpectation(
                    this.Location,
                    keyword,
                    _sourceText.Slice(this.ActiveToken.Block));
            }
        }

        /// <summary>
        /// Creates a block of text that can be used to describe the token type in an error message.
        /// </summary>
        /// <param name="tokenType">Type of the token.</param>
        /// <returns>ReadOnlyMemory&lt;System.Char&gt;.</returns>
        private ReadOnlySpan<char> TokenTypeTextForErrorMessage(TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.Name:
                case TokenType.Float:
                case TokenType.Integer:
                case TokenType.String:
                    return _sourceText.Slice(this.ActiveToken.Block);

                default:
                    return tokenType.Description().Span;
            }
        }

        private LexicalToken FetchNextTokenFromStream()
        {
            _sourceText.SkipWhitespace();
            if (_sourceText.HasData)
            {
                SourceLocation location;

                // Comments
                // -------------------
                if (_sourceText.CheckCursor(SR.IsCommentGlyph))
                {
                    var text = _sourceText.NextComment(out location);
                    return new LexicalToken(TokenType.Comment, text, location, true);
                }

                // Flow Controler characer (non-text entities)
                // -------------------
                else if (_sourceText.CheckCursor(SR.IsControlGlyph))
                {
                    var textBlock = _sourceText.NextControlPhrase(out location);
                    var text = _sourceText.Slice(textBlock);
                    var tokenType = text.ToTokenType();
                    var shouldSkipControlToken = tokenType == TokenType.Comma;
                    return new LexicalToken(tokenType, textBlock, location, shouldSkipControlToken);
                }

                // Named fields
                // ---------------------------------
                else if (_sourceText.CheckCursor(SR.IsStartOfNameGlyph))
                {
                    var textBlock = _sourceText.NextName(out location);
                    return _sourceText.CharactersToToken(textBlock, location);
                }

                // Numbers
                // ----------------------------------
                else if (_sourceText.CheckCursor(SR.IsStartOfNumberGlyph))
                {
                    var textBlock = _sourceText.NextNumber(out location);
                    var text = _sourceText.Slice(textBlock);
                    var tokenType = text.IndexOfAny(CHARS.FloatIndicatorChars.Span) >= 0 ? TokenType.Float : TokenType.Integer;
                    return new LexicalToken(tokenType, textBlock, location);
                }

                // Strings
                // ----------------------------------
                else if (_sourceText.CheckCursor(SR.IsStringDelimiterGlyph))
                {
                    var textBlock = _sourceText.NextString(out location);
                    return new LexicalToken(TokenType.String, textBlock, location);
                }
                else
                {
                    // who the heck knows, just fail
                    location = _sourceText.RetrieveCurrentLocation();
                    throw new GraphQLSyntaxException(
                        location,
                        $"Unexpected character: '{_sourceText.Peek()}'");
                }
            }

            return LexicalToken.EoF;
        }

        /// <summary>
        /// Gets a reference to the token in scope for analysis.
        /// </summary>
        /// <value>The active token.</value>
        public LexicalToken ActiveToken { get; private set; }

        /// <summary>
        /// Gets the actual text pointed at by the current <see cref="ActiveToken"/>.
        /// </summary>
        /// <value>The active token text.</value>
        public ReadOnlySpan<char> ActiveTokenText => _sourceText.Slice(this.ActiveToken.Block);

        /// <summary>
        /// Gets the active token type descrption that can be used in error messages.
        /// </summary>
        /// <value>The active token type descrption.</value>
        private ReadOnlySpan<char> ActiveTokenTypeDescrption
        {
            get
            {
                if (this.EndOfStream)
                    return TokenType.EndOfFile.Description().Span;

                if (this.ActiveToken.TokenType == TokenType.None)
                    return ParserConstants.Keywords.Null.Span;

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
                return this.ActiveToken.TokenType == TokenType.EndOfFile;
            }
        }

        /// <summary>
        /// Gets the <see cref="TokenType"/> of the active token on the stream or EOF
        /// if no active token.
        /// </summary>
        /// <value>the <see cref="TokenType"/> at the top of the stream.</value>
        public TokenType TokenType => this.ActiveToken.TokenType;

        /// <summary>
        /// Gets the original, unedited source this stream is iterating over.
        /// </summary>
        /// <value>The source.</value>
        public SourceText Source => _sourceText;
    }
}