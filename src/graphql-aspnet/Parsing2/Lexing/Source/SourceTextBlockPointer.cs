// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing2.Lexing.Source
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// A struct that contains information that points to a block of text in
    /// a <see cref="SourceText"/> object.
    /// </summary>
    [DebuggerDisplay("(Index: {StartIndex}, Length: {Length})")]
    public readonly struct SourceTextBlockPointer : IEquatable<SourceTextBlockPointer>
    {
        /// <summary>
        /// Gets a block pointer that points to nothing.
        /// </summary>
        /// <value>An empty block pointer.</value>
        public static SourceTextBlockPointer None { get; } = new SourceTextBlockPointer(-1, 0);

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceTextBlockPointer"/> struct.
        /// </summary>
        /// <param name="startIndex">The 0-based position within the related source text where
        /// this block starts.</param>
        /// <param name="length">The length of the block.</param>
        public SourceTextBlockPointer(int startIndex, int length)
        {
            this.StartIndex = startIndex;
            this.Length = length;
        }

        /// <summary>
        /// Gets the index where this block starts, in the related source text.
        /// </summary>
        /// <value>The start index.</value>
        public int StartIndex { get; }

        /// <summary>
        /// Gets the number of characters contained in the block, starting from the <see cref="StartIndex"/>
        /// in the related source text.
        /// </summary>
        /// <value>The length.</value>
        public int Length { get; }

        /// <inheritdoc />
        public bool Equals(SourceTextBlockPointer other)
        {
            return this.StartIndex == other.StartIndex
                && this.Length == other.Length;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is SourceTextBlockPointer snv
                && this.Equals(snv);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (this.StartIndex, this.Length).GetHashCode();
        }

        /// <summary>
        /// Implements the == operator.
        /// </summary>
        /// <param name="lhs">The left hand side of the equality check.</param>
        /// <param name="rhs">The right hand side of the equality check.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(SourceTextBlockPointer lhs, SourceTextBlockPointer rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Implements the != operator.
        /// </summary>
        /// <param name="lhs">The left hand side of the equality check.</param>
        /// <param name="rhs">The right hand side of the equality check.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(SourceTextBlockPointer lhs, SourceTextBlockPointer rhs) => !(lhs == rhs);
    }
}