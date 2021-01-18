// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Source
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// A snapshot representation of a location of a read block within a source text.
    /// </summary>
    [DebuggerDisplay("Index: {AbsoluteIndex}, Line: ({LineNumber}:{LineIndex})")]
    public class SourceLocation
    {
        /// <summary>
        /// Gets a single source location pointing to no location in the source file.
        /// </summary>
        /// <value>The none.</value>
        public static SourceLocation None { get; } = new SourceLocation();

        /// <summary>
        /// Initializes static members of the <see cref="SourceLocation"/> class.
        /// </summary>
        static SourceLocation()
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="SourceLocation"/> class from being created.
        /// </summary>
        private SourceLocation()
        {
            this.LineNumber = -1;
            this.AbsoluteIndex = -1;
            this.LineIndex = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceLocation" /> class.
        /// </summary>
        /// <param name="absoluteIndex">The absolute overall position pointed at by this location in the source material.</param>
        /// <param name="line">A reference to the line of text pointed at by this location.</param>
        /// <param name="lineNumber">The line number pointed at by this position.</param>
        /// <param name="lineIndex">The relative index into the line of the location.</param>
        public SourceLocation(int absoluteIndex, ReadOnlyMemory<char> line, int lineNumber, int lineIndex)
        {
            this.LineText = line;
            this.AbsoluteIndex = absoluteIndex;
            this.LineNumber = lineNumber;
            this.LineIndex = lineIndex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceLocation" /> class.
        /// </summary>
        /// <param name="absoluteIndex">The absolute overall position pointed at by this location in the source material.</param>
        /// <param name="lineNumber">The line number pointed at by this position.</param>
        /// <param name="lineIndex">The relative index into the line of the location.</param>
        public SourceLocation(int absoluteIndex, int lineNumber, int lineIndex)
        {
            this.AbsoluteIndex = absoluteIndex;
            this.LineText = ReadOnlyMemory<char>.Empty;
            this.LineNumber = lineNumber;
            this.LineIndex = lineIndex;
        }

        /// <summary>
        /// Creates an equvilant <see cref="SourceOrigin"/> out of this <see cref="SourceLocation"/>.
        /// </summary>
        /// <returns>SourceOrigin.</returns>
        public SourceOrigin AsOrigin()
        {
            return new SourceOrigin(this);
        }

        /// <summary>
        /// Gets the absolute position pointed at in the source text by this location.
        /// </summary>
        /// <value>The position.</value>
        public int AbsoluteIndex { get; private set; }

        /// <summary>
        /// Gets a reference to the line text that this location is contained in.
        /// </summary>
        /// <value>The line text.</value>
        public ReadOnlyMemory<char> LineText { get; }

        /// <summary>
        /// Gets the Offset in scope of the line by this location.
        /// </summary>
        /// <value>The position in line.</value>
        public int LineIndex { get; private set; }

        /// <summary>
        /// Gets the character position of the location in the line (i.e. LineIndex + 1).
        /// </summary>
        /// <value>The line position.</value>
        public int LinePosition => this.LineIndex + 1;

        /// <summary>
        /// Gets the line number the source is currently pointed at (this number is '1-based').
        /// </summary>
        /// <value>The line number.</value>
        public int LineNumber { get; private set; }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"Line: {this.LineNumber}, Column: {this.LinePosition}";
        }
    }
}