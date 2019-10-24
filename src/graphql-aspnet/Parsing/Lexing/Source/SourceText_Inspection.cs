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
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Parsing.Lexing.Source.SourceRules;
    using CHARS = GraphQL.AspNet.Parsing.ParserConstants.Characters;

    /// <summary>
    /// A wrapper for <see cref="Span{T}"/> to provide some context sensitive helper methods for parsing through it.
    /// </summary>
    public ref partial struct SourceText
    {
        /// <summary>
        /// Inspects the next character in the sequence to see if its considered whitespace.
        /// </summary>
        /// <returns>System.Boolean.</returns>
        [DebuggerStepThrough]
        public bool PeekNextIsWhitespace()
        {
            // WhiteSpace: https://graphql.github.io/graphql-spec/June2018/#sec-White-Space
            // Line Terminators: https://graphql.github.io/graphql-spec/June2018/#sec-Line-Terminators
            return CHARS.WhiteSpace.Span.IndexOf(this.Peek()) >= 0;
        }

        /// <summary>
        /// Inspects the next character in the sequence to see if its considered whitespace.
        /// </summary>
        /// <returns>System.Boolean.</returns>
        [DebuggerStepThrough]
        public bool PeekNextIsLineTerminator()
        {
            return this.PeekIsLineTerminator(this.Cursor);
        }

        /// <summary>
        /// Inspects the character at the supplied index into the source text to determine if its pointed at
        /// a line terminator or not.
        /// </summary>
        /// <param name="absoluteIndex">Index of the absolute.</param>
        /// <returns>System.Boolean.</returns>
        public bool PeekIsLineTerminator(int absoluteIndex)
        {
            absoluteIndex = absoluteIndex < 0 ? 0 : absoluteIndex;
            if (absoluteIndex >= _sourceText.Length)
                return false;

            return _sourceText[absoluteIndex] == CHARS.NL ||
                   (absoluteIndex + 1 < _sourceText.Length
                    && _sourceText[absoluteIndex] == CHARS.CR
                    && _sourceText[absoluteIndex + 1] == CHARS.NL);
        }

        /// <summary>
        /// Peeks at the next character in this instance..
        /// </summary>
        /// <returns>System.Char.</returns>
        [DebuggerStepThrough]
        public char Peek()
        {
            var slice = this.Peek(1);
            if (slice.Length == 0)
                return CHARS.NUL;

            return slice[0];
        }

        /// <summary>
        /// Peek at the Next number of characters without advancing the cursor.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns>System.ReadOnlySpan&lt;System.Char&gt;.</returns>
        [DebuggerStepThrough]
        public ReadOnlySpan<char> Peek(int length)
        {
            return this.Slice(this.Cursor, length);
        }

        /// <summary>
        /// Returns a span of characters from the current cursor position up until the next line break.
        /// Does not advance the cursor.Returns the remaining body of the source text if no line breaks are found.
        /// </summary>
        /// <returns>System.ReadOnlySpan&lt;System.Char&gt;.</returns>
        [DebuggerStepThrough]
        public ReadOnlySpan<char> PeekLine()
        {
            return this.PeekLine(this.Cursor);
        }

        /// <summary>
        /// Returns a span of characters from the supplied absolute position up until the next line break.
        /// Returns the remaining body of the source text if no line breaks are found.
        /// </summary>
        /// <param name="absoluteIndex">The absolute index within the soruce text to start inspecting.</param>
        /// <returns>System.ReadOnlySpan&lt;System.Char&gt;.</returns>
        public ReadOnlySpan<char> PeekLine(int absoluteIndex)
        {
            absoluteIndex = absoluteIndex < 0 ? 0 : absoluteIndex;
            if (absoluteIndex >= _sourceMemory.Length)
                return ReadOnlySpan<char>.Empty;

            var newLineIndex = this.Slice(absoluteIndex).IndexOf(CHARS.NL);
            if (newLineIndex == CHARS.NO_INDEX)
                return this.Slice(absoluteIndex);

            var slice = this.Slice(absoluteIndex, newLineIndex);

            // \r\n is considered a new line: https://graphql.github.io/graphql-spec/June2018/#sec-Line-Terminators
            return slice.TrimTrailingCarriageReturn();
        }

        /// <summary>
        /// Peeks at the complete line of text pointed at by the cursor, regardless of hte position
        /// in the line. If the cursor is situated at at index 3 of the line
        /// this function will automatically back up 3 characters to return the whole line of text.
        /// </summary>
        /// <returns>System.ReadOnlySpan&lt;System.Char&gt;.</returns>
        [DebuggerStepThrough]
        public ReadOnlySpan<char> PeekFullLine()
        {
            return this.PeekFullLine(this.Cursor);
        }

        /// <summary>
        /// Peeks at the complete line of text at the given index of hte source text, regardless of the position
        /// pointed at by said location. If the absolute index is at index 3 of its line
        /// this function will automatically back up 3 characters to return the whole line of text.
        /// </summary>
        /// <param name="absoluteIndex">An absolute index into the source text, not relating to the line of text said index
        /// resides on.</param>
        /// <returns>System.ReadOnlySpan&lt;System.Char&gt;.</returns>
        public ReadOnlySpan<char> PeekFullLine(int absoluteIndex)
        {
            return this.PeekFullLineInternal(absoluteIndex).Span;
        }

        /// <summary>
        /// Peeks at the complete line of text at the given index of hte source text, regardless of the position
        /// pointed at by said location. If the absolute index is at index 3 of its line
        /// this function will automatically back up 3 characters to return the whole line of text.
        ///
        /// Note: This private method returns a reference to the line in the originally supplied
        /// memory block such that it can then be exported and referenced else where vs. only
        /// in stack frames with ReadOnlySpan.
        /// </summary>
        /// <param name="absoluteIndex">Index of the absolute.</param>
        /// <returns>System.ReadOnlyMemory&lt;System.Char&gt;.</returns>
        [DebuggerStepThrough]
        private ReadOnlyMemory<char> PeekFullLineInternal(int absoluteIndex)
        {
            absoluteIndex = absoluteIndex < 0 ? 0 : absoluteIndex;
            if (absoluteIndex >= _sourceText.Length)
                return ReadOnlyMemory<char>.Empty;

            // are we in the middle of a line delimiter '\r\n'
            if (absoluteIndex > 0
                && absoluteIndex < _sourceText.Length - 1
                && _sourceText[absoluteIndex - 1] == CHARS.CR && _sourceText[absoluteIndex] == CHARS.NL)
            {
                absoluteIndex--;
            }

            var startIndex = this.Slice(0, absoluteIndex).LastIndexOf(CHARS.NL);
            startIndex = startIndex < 0 ? 0 : startIndex + 1;

            return this.SliceMemory(startIndex, this.PeekLine(startIndex).Length);
        }

        /// <summary>
        /// Retrieves an object representing the current location within this instance.
        /// </summary>
        /// <returns>GraphQL.AspNet.Parser.Lexing.SourceLocation.</returns>
        [DebuggerStepThrough]
        public SourceLocation RetrieveCurrentLocation()
        {
            return this.RetrieveLocationFromPosition(this.Cursor);
        }

        /// <summary>
        /// Retrieves a qualified location from the given index in the source text. Accounts for
        /// bound overruns.
        /// </summary>
        /// <param name="sourceIndex">Index into the source material to calculate positioning.</param>
        /// <returns>GraphQL.AspNet.Parser.Lexing.SourceLocation.</returns>
        public SourceLocation RetrieveLocationFromPosition(int sourceIndex)
        {
            if (_sourceText.Length == 0)
                return SourceLocation.None;

            // auto bound the index to within the text string
            sourceIndex = sourceIndex < 0 ? 0 : sourceIndex;
            sourceIndex = sourceIndex > _sourceText.Length ? _sourceText.Length : sourceIndex;

            var (lineNumber, lineIndex) = this.RetrieveLineInformation(sourceIndex);
            return new SourceLocation(sourceIndex, this.PeekFullLineInternal(sourceIndex), lineNumber, lineIndex);
        }

        /// <summary>
        /// Uses the provided source location as a starting point then calcualtes a new source location based
        /// on the number of characters to offset from.  Recalulates line number and position based on the new location.
        /// </summary>
        /// <param name="location">the starting location to calculate from.</param>
        /// <param name="offset">the number of characters from the supplied position to offset (can be negative).</param>
        /// <returns>a new source location.</returns>
        public SourceLocation OffsetLocation(SourceLocation location, int offset)
        {
            return this.RetrieveLocationFromPosition(location.AbsoluteIndex + offset);
        }

        /// <summary>
        /// Gets the index in scope of the line containing the absolute index in the source text.
        /// </summary>
        /// <param name="absoluteIndex">The overall index in the source text.</param>
        /// <returns>System.Int32.</returns>
        private (int lineNumber, int lineIndex) RetrieveLineInformation(int absoluteIndex)
        {
            if (_sourceText.Length == 0)
                return (CHARS.NO_INDEX, CHARS.NO_INDEX);

            var length = absoluteIndex >= _sourceText.Length ? _sourceText.Length : absoluteIndex;
            var slice = _sourceText.Slice(0, length);

            var lineNum = 1;
            var lastLineIndex = 0;
            while (slice.Length > 0)
            {
                var lineIndex = slice.IndexOf(CHARS.NL);
                lastLineIndex += lineIndex + 1;
                if (lineIndex < 0)
                    break;

                slice = slice.Slice(lineIndex + 1);
                lineNum++;
            }

            return (lineNum, absoluteIndex - lastLineIndex);
        }

        /// <summary>
        /// Checks to see if the text at the current cursor location
        /// matches all of the supplied rules...
        /// </summary>
        /// <param name="rules">The rules.</param>
        /// <returns>System.Boolean.</returns>
        [DebuggerStepThrough]
        public bool CheckCursor(params GraphQLSourceRule[] rules)
        {
            foreach (var rule in rules)
            {
                if (!SourceRuleFactory.FindRule(rule).Validate(this))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Extracts a slice of text from the source, does not advance the cursor position.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="length">The length.</param>
        /// <returns>System.ReadOnlySpan&lt;System.Char&gt;.</returns>
        [DebuggerStepThrough]
        public ReadOnlySpan<char> Slice(int start, int length)
        {
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "The start index cannot be negative");

            if (length < 0)
            {
                length = Math.Abs(length);
                start -= length;
                if (start < 0)
                {
                    length = length - (0 - start);
                    start = 0;
                }
            }

            if (start + length > _sourceText.Length)
                length = this.Length - start;

            if (length == 0)
                return ReadOnlySpan<char>.Empty;

            return _sourceText.Slice(start, length);
        }

        /// <summary>
        /// Extracts a slice of text from the source starting at the given index
        /// and continuing to the end of the source.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <returns>System.ReadOnlySpan&lt;System.Char&gt;.</returns>
        [DebuggerStepThrough]
        public ReadOnlySpan<char> Slice(int start)
        {
            return this.Slice(start, this.Length - start);
        }

        /// <summary>
        /// Retrieves a slice of the original memory being tracked by this instance..
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="length">The length.</param>
        /// <returns>System.ReadOnlyMemory&lt;System.Char&gt;.</returns>
        public ReadOnlyMemory<char> SliceMemory(int start, int length)
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return _sourceMemory.Slice(start, length);
        }

        /// <summary>
        /// Returns a string representation fo the remaining characters in the buffer from the
        /// current cursor position.
        /// </summary>
        /// <returns>The fully qualified type name.</returns>
        [DebuggerStepThrough]
        public override string ToString()
        {
            return this.HasData ? this.Slice(this.Cursor).ToString() : string.Empty;
        }
    }
}