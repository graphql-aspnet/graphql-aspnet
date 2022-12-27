// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Parsing.SyntaxNodes
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// An identified location within the node pool of a master syntax tree
    /// where this node is located.
    /// </summary>
    [DebuggerDisplay("{BlockIndex}, {BlockPosition}, {ChildBlockIndex}, {ChildBlockLength}")]
    internal readonly struct SyntaxNodeCoordinates : IEquatable<SyntaxNodeCoordinates>
    {
        /// <summary>
        /// Gets a set of coordinates indicating no position.
        /// </summary>
        /// <value>A set of coordinates indicating no position within a tree.</value>
        public static SyntaxNodeCoordinates None { get; } = new SyntaxNodeCoordinates(-1, -1);

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxNodeCoordinates" /> struct.
        /// </summary>
        /// <param name="blockIndex">Index of the block in the master syntax tree
        /// where this node is located.</param>
        /// <param name="blockPosition">The 0-based position within the given
        /// block where this node is located.</param>
        /// <param name="childBlockIndex">Index of the child block in the master syntax tree.</param>
        /// <param name="childBlockLength">Length of the child block within the master syntax tree.</param>
        [DebuggerStepperBoundary]
        public SyntaxNodeCoordinates(
            int blockIndex,
            int blockPosition,
            int childBlockIndex = -1,
            int childBlockLength = 0)
        {
            this.BlockIndex = blockIndex;
            this.BlockPosition = blockPosition;
            this.ChildBlockIndex = childBlockIndex;
            this.ChildBlockLength = childBlockLength;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxNodeCoordinates"/> struct.
        /// </summary>
        [DebuggerStepperBoundary]
        public SyntaxNodeCoordinates()
        {
            this.BlockIndex = -1;
            this.BlockPosition = -1;
            this.ChildBlockIndex = -1;
            this.ChildBlockLength = 0;
        }

        /// <summary>
        /// Clones this instance into a new instance. For any supplied values (i.e. not null)
        /// those values will be used in the cloned instance.
        /// </summary>
        /// <param name="childBlockIndex">Index of the child block within the syntax tree.</param>
        /// <param name="childBlockLength">Length of the child block within the syntax tree.</param>
        /// <returns>SyntaxNodeCoordinates.</returns>
        public SyntaxNodeCoordinates Clone(
            int? childBlockIndex = null,
            int? childBlockLength = null)
        {
            return new SyntaxNodeCoordinates(
                this.BlockIndex,
                this.BlockPosition,
                childBlockIndex ?? this.ChildBlockIndex,
                childBlockLength ?? this.ChildBlockLength);
        }

        /// <inheritdoc />
        public bool Equals(SyntaxNodeCoordinates other)
        {
            return this.BlockIndex == other.BlockIndex
                && this.BlockPosition == other.BlockPosition
                && this.ChildBlockIndex == other.ChildBlockIndex
                && this.ChildBlockLength == other.ChildBlockLength;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is SyntaxNodeCoordinates snc
                && this.Equals(snc);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (this.BlockIndex, this.BlockPosition, this.ChildBlockIndex, this.ChildBlockLength).GetHashCode();
        }

        /// <summary>
        /// Gets the index of the block, within the master syntax tree's pool,
        /// where this node is located.
        /// </summary>
        /// <value>The master syntax tree pool index of this node.</value>
        public int BlockIndex { get; }

        /// <summary>
        /// Gets the 0-based position (within the block identified by <see cref="BlockIndex"/>
        /// where this node is located.
        /// </summary>
        /// <value>The pool index position.</value>
        public int BlockPosition { get; }

        /// <summary>
        /// Gets the index in hte master syntax tree's pool where the children of this
        /// node are defined.
        /// </summary>
        /// <value>The index of the child pool.</value>
        public int ChildBlockIndex { get; }

        /// <summary>
        /// Gets the number of nodes (within the assigned child block) that
        /// contain actual child nodes.
        /// </summary>
        /// <value>The length of the child block.</value>
        public int ChildBlockLength { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.BlockIndex}, {this.BlockPosition}, {this.ChildBlockIndex}, {this.ChildBlockLength}";
        }

        /// <summary>
        /// Implements the == operator.
        /// </summary>
        /// <param name="lhs">The left hand side of the equality check.</param>
        /// <param name="rhs">The right hand side of the equality check.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(SyntaxNodeCoordinates lhs, SyntaxNodeCoordinates rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Implements the != operator.
        /// </summary>
        /// <param name="lhs">The left hand side of the equality check.</param>
        /// <param name="rhs">The right hand side of the equality check.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(SyntaxNodeCoordinates lhs, SyntaxNodeCoordinates rhs) => !(lhs == rhs);
    }
}