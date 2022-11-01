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

    /// <summary>
    /// A struct representing a block of characters from a query text.
    /// </summary>
    public readonly struct SynNodeValue : IEquatable<SynNodeValue>
    {
        public static SynNodeValue None { get; } = new SynNodeValue(ReadOnlyMemory<char>.Empty);

        /// <summary>
        /// Initializes a new instance of the <see cref="SynNodeValue"/> struct.
        /// </summary>
        [DebuggerStepperBoundary]
        public SynNodeValue()
        {
            this.Value = ReadOnlyMemory<char>.Empty;
            this.ValueType = ScalarValueType.Unknown;
        }

        [DebuggerStepperBoundary]
        public SynNodeValue(ReadOnlyMemory<char> value)
        {
            this.Value = value;
            this.ValueType = ScalarValueType.Unknown;
        }

        [DebuggerStepperBoundary]
        public SynNodeValue(ReadOnlyMemory<char> value, ScalarValueType valueType)
        {
            this.Value = value;
            this.ValueType = valueType;
        }

        /// <summary>
        /// Gets the block of characters representing a value.
        /// </summary>
        /// <value>The value.</value>
        public ReadOnlyMemory<char> Value { get; }

        /// <summary>
        /// Gets the scalared value type of <see cref="Value"/>, if any.
        /// </summary>
        /// <value>The type of the value.</value>
        public ScalarValueType ValueType { get; }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (this.ValueType, this.Value).GetHashCode();
        }

        /// <inheritdoc />
        public bool Equals(SynNodeValue other)
        {
            return this.ValueType.Equals(other.ValueType)
            && this.Value.Span.SequenceEqual(other.Value.Span);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is SynNodeValue snv
                && this.Equals(snv);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Value.ToString();
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