// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing2
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Parsing2.Lexing.Source;

    /// <summary>
    /// A struct representing a block of characters from a query text.
    /// </summary>
    public readonly struct SynNodeValue : IEquatable<SynNodeValue>
    {
        public static SynNodeValue None { get; } = new SynNodeValue(SourceTextBlockPointer.None);

        /// <summary>
        /// Initializes a new instance of the <see cref="SynNodeValue"/> struct.
        /// </summary>
        [DebuggerStepperBoundary]
        public SynNodeValue()
        {
            this.TextBlock = SourceTextBlockPointer.None;
            this.ValueType = ScalarValueType.Unknown;
        }

        [DebuggerStepperBoundary]
        public SynNodeValue(SourceTextBlockPointer textBlock)
        {
            this.TextBlock = textBlock;
            this.ValueType = ScalarValueType.Unknown;
        }

        [DebuggerStepperBoundary]
        public SynNodeValue(SourceTextBlockPointer textBlock, ScalarValueType valueType)
        {
            this.TextBlock = textBlock;
            this.ValueType = valueType;
        }

        /// <summary>
        /// Gets the block of characters representing a value.
        /// </summary>
        /// <value>The value.</value>
        public SourceTextBlockPointer TextBlock { get; }

        /// <summary>
        /// Gets the scalared value type of text pointed at by <see cref="TextBlock"/>, if any.
        /// </summary>
        /// <value>The type of the value.</value>
        public ScalarValueType ValueType { get; }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (this.ValueType, this.TextBlock).GetHashCode();
        }

        /// <inheritdoc />
        public bool Equals(SynNodeValue other)
        {
            return this.ValueType.Equals(other.ValueType)
            && this.TextBlock.Equals(other.TextBlock);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is SynNodeValue snv
                && this.Equals(snv);
        }

        /// <summary>
        /// Implements the == operator.
        /// </summary>
        /// <param name="lhs">The left hand side of the equality check.</param>
        /// <param name="rhs">The right hand side of the equality check.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(SynNodeValue lhs, SynNodeValue rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Implements the != operator.
        /// </summary>
        /// <param name="lhs">The left hand side of the equality check.</param>
        /// <param name="rhs">The right hand side of the equality check.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(SynNodeValue lhs, SynNodeValue rhs) => !(lhs == rhs);
    }
}