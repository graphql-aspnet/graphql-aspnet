﻿// *************************************************************
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
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;

    /// <summary>
    /// A struct representing a block of characters from a query text.
    /// </summary>
    internal readonly struct SyntaxNodeValue : IEquatable<SyntaxNodeValue>
    {
        /// <summary>
        /// Gets a node value that represents no typed text block within a source text.
        /// </summary>
        /// <value>The none.</value>
        public static SyntaxNodeValue None { get; } = new SyntaxNodeValue(SourceTextBlockPointer.None);

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxNodeValue"/> struct.
        /// </summary>
        [DebuggerStepperBoundary]
        public SyntaxNodeValue()
        {
            this.TextBlock = SourceTextBlockPointer.None;
            this.ValueType = ScalarValueType.Unknown;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxNodeValue"/> struct.
        /// </summary>
        /// <param name="textBlock">The text block this value points to.</param>
        [DebuggerStepperBoundary]
        public SyntaxNodeValue(SourceTextBlockPointer textBlock)
        {
            this.TextBlock = textBlock;
            this.ValueType = ScalarValueType.Unknown;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxNodeValue"/> struct.
        /// </summary>
        /// <param name="textBlock">The text block this value points to.</param>
        /// <param name="valueType">The type of text contained in the block (e.g. number, string etc.).</param>
        [DebuggerStepperBoundary]
        public SyntaxNodeValue(SourceTextBlockPointer textBlock, ScalarValueType valueType)
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
        public bool Equals(SyntaxNodeValue other)
        {
            return this.ValueType.Equals(other.ValueType)
            && this.TextBlock.Equals(other.TextBlock);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is SyntaxNodeValue snv
                && this.Equals(snv);
        }

        /// <summary>
        /// Implements the == operator.
        /// </summary>
        /// <param name="lhs">The left hand side of the equality check.</param>
        /// <param name="rhs">The right hand side of the equality check.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(SyntaxNodeValue lhs, SyntaxNodeValue rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Implements the != operator.
        /// </summary>
        /// <param name="lhs">The left hand side of the equality check.</param>
        /// <param name="rhs">The right hand side of the equality check.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(SyntaxNodeValue lhs, SyntaxNodeValue rhs) => !(lhs == rhs);
    }
}