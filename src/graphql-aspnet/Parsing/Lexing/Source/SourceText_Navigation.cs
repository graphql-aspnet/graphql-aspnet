// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.Lexing.Source
{
    using System;
    using CHARS = GraphQL.AspNet.Parsing.ParserConstants.Characters;

    /// <summary>
    /// A wrapper for a block of characters to provide some context sensitive inspection and navigation support
    /// for parsing through it.
    /// </summary>
    public ref partial struct SourceText
    {
        private const string AT_LEAST_ZERO_CHARACTERS = "The requested length of characters cannot be less than 0";
        private readonly ReadOnlySpan<char> _sourceText;
        private readonly ReadOnlyMemory<char> _sourceMemory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceText" /> struct.
        /// </summary>
        /// <param name="sourceText">The source text.</param>
        /// <param name="offset">The initial position to place the inspection cursor from the begining of the span.</param>
        public SourceText(ReadOnlyMemory<char> sourceText, int offset = 0)
        {
            _sourceMemory = sourceText;
            _sourceText = _sourceMemory.Span;
            this.Cursor = 0;
            if (_sourceText.Length > 0)
            {
                if (offset < 0 || offset >= _sourceText.Length)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(offset),
                        $"The requested starting index '{offset}' is outside of the range of the supplied {nameof(sourceText)} (Length: {sourceText.Length}).");
                }

                this.Cursor = offset;
            }
        }

        /// <summary>
        /// Seeks the specified number of characters forward.
        /// </summary>
        /// <param name="offset">The length of characters to seek forward (or backwards if null).</param>
        /// <param name="from">A location in the text to start seeking from.</param>
        public void Seek(int offset, SourceTextPosition from)
        {
            int startLocation;
            switch (from)
            {
                case SourceTextPosition.FromCurrentCursor:
                    startLocation = this.Cursor;
                    break;

                case SourceTextPosition.FromStart:
                    startLocation = 0;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(from), from, "The chosen from value is not supported by this method.");
            }

            if (startLocation + offset < 0 || startLocation + offset > _sourceText.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), offset, "The chosen offset is outside the range of this text.");

            this.Cursor = startLocation + offset;
        }

        /// <summary>
        /// Begins processing characters, building a span of text until the predicate condition is not met
        /// at which point the characters are returned and the cursor advanced to the character that did not meet the condition.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>System.ReadOnlySpan&lt;System.Char&gt;.</returns>
        public ReadOnlySpan<char> NextFilter(Func<char, bool> predicate)
        {
            var index = this.Cursor;
            while (index < _sourceText.Length && predicate(_sourceText[index]))
            {
                index += 1;
            }

            var slice = this.Slice(this.Cursor, index - this.Cursor);
            this.Cursor = index;
            return slice;
        }

        /// <summary>
        /// Begins processing characters, building a span of text until the predicate condition is not met
        /// at which point the characters are returned and the cursor advanced to the character that did not meet the condition.
        /// The predicate will recieve the entire set of characters in the phrase on each iteration. The predicate should inspect
        /// the phrase then return true when it matches an expected condition. This will cause the phraser to
        /// cease processing and return the phrase that was created while advancing the cursor.
        /// </summary>
        /// <param name="predicate">The predicate which recieves the entire set of characters to be returned so far.</param>
        /// <returns>System.ReadOnlySpan&lt;System.Char&gt;.</returns>
        public ReadOnlySpan<char> NextPhrase(SourceNextPhraseDelegate predicate)
        {
            var length = 1;
            var isProcessing = true;
            while (isProcessing && this.Cursor + length < _sourceText.Length)
            {
                var result = predicate(this.Slice(this.Cursor, length));
                switch (result)
                {
                    // keep adding characters to the phrase
                    case NextPhraseResult.Continue:
                        break;

                    // we're done, exit out and extract the string
                    case NextPhraseResult.Complete:
                        isProcessing = false;
                        break;

                    // we're done but the previous iteration was correct
                    case NextPhraseResult.CompleteOnPrevious:
                        length = length >= 1 ? length - 1 : 0;
                        isProcessing = false;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException($"{typeof(NextPhraseResult).Name} value not supported ('{result}')");
                }

                length += isProcessing ? 1 : 0;
            }

            var slice = this.Slice(this.Cursor, length);
            this.Cursor += length;
            return slice;
        }

        /// <summary>
        /// Returns the next number of characters in the sequence. If the remaining characters is less
        /// than the value provided, the rest of the source text is returned.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns>System.ReadOnlySpan&lt;System.Char&gt;.</returns>
        public ReadOnlySpan<char> Next(int length = 1)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), AT_LEAST_ZERO_CHARACTERS);

            if (length == 0 || this.Cursor >= this.Length)
                return ReadOnlySpan<char>.Empty;

            if (this.Cursor + length >= this.Length)
                length = this.Length - this.Cursor;

            var span = this.Slice(this.Cursor, length);
            this.Cursor += length;
            return span;
        }

        /// <summary>
        /// Returns a span from the current cursor to the next instance of a whitespace
        /// character. The cursor is advanced to the whitespace character.
        /// </summary>
        /// <returns>System.ReadOnlySpan&lt;System.Char&gt;.</returns>
        public ReadOnlySpan<char> NextCharacterGroup()
        {
            if (this.HasData)
            {
                var slice = _sourceText.Slice(this.Cursor);
                var index = slice.IndexOfAny(CHARS.WhiteSpace.Span);
                if (index >= 0)
                {
                    slice = this.Slice(this.Cursor, index);
                }

                this.Cursor += slice.Length;
                return slice;
            }

            return ReadOnlySpan<char>.Empty;
        }

        /// <summary>
        /// Advances the cursor to the first non-whitespace character and returns the number of characters that were skipped.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int SkipWhitespace()
        {
            if (!this.HasData)
                return 0;

            int count = 0;
            while (CHARS.WhiteSpace.Span.IndexOf(this.Peek()) >= 0)
            {
                count++;
                this.Cursor += 1;
            }

            return count;
        }

        /// <summary>
        /// Returns all the characters from the current position up to, but not including, the next line then advances
        /// the cursor to the first character after the line terminator.
        /// </summary>
        /// <returns>System.ReadOnlySpan&lt;System.Char&gt;.</returns>
        public ReadOnlySpan<char> NextLine()
        {
            if (this.Cursor >= this.Length)
                return ReadOnlySpan<char>.Empty;

            var indexOfN = _sourceText.Slice(this.Cursor).IndexOf(CHARS.NL);
            ReadOnlySpan<char> slice;
            if (indexOfN < 0)
            {
                // consume the rest of the text
                slice = this.Slice(this.Cursor);
                this.Cursor = _sourceText.Length;
            }
            else
            {
                slice = this.Slice(this.Cursor, indexOfN);
                this.Cursor += slice.Length + 1;
            }

            // \r\n is considered a new line: https://graphql.github.io/graphql-spec/June2018/#sec-Line-Terminators
            return slice.TrimTrailingCarriageReturn();
        }

        /// <summary>
        /// Gets a value indicating whether there is data left in the original source text that has not already.
        /// been parsed or inspected.
        /// </summary>
        /// <value><c>true</c> if data is left to be read, otherwise; <c>false</c>.</value>
        public bool HasData => !this.EndOfFile;

        /// <summary>
        /// Gets the 0-based index into the source text where the tracking cursor is currently pointing.
        /// </summary>
        /// <value>the current location of the source text pointer.</value>
        public int Cursor { get; private set; }

        /// <summary>
        /// Gets the total length of the original source text.
        /// </summary>
        /// <value>The number of characters in the source text.</value>
        public int Length => _sourceText.Length;

        /// <summary>
        /// Gets a value indicating whether there is no more data left to be read from the source text.
        /// </summary>
        /// <value><c>true</c> if data is left to be read, otherwise; <c>false</c>.</value>
        public bool EndOfFile => this.Cursor >= _sourceText.Length;

        /// <summary>
        /// Gets the current line number the <see cref="Cursor"/> is located in. A line
        /// is delimited by a '\n' caracter in the source text.
        /// </summary>
        /// <value><c>true</c> if data is left to be read, otherwise; <c>false</c>.</value>
        public int CurrentLineNumber => this.RetrieveLineInformation(this.Cursor).lineNumber;

        /// <summary>
        /// Gets a reference to the source data being processed by this object.
        /// </summary>
        /// <value><c>true</c> if data is left to be read, otherwise; <c>false</c>.</value>
        public ReadOnlyMemory<char> Text => _sourceMemory;
    }
}