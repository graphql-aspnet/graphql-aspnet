// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;

    /// <summary>
    /// A representation of a single, simple piece of data.
    /// </summary>
    [DebuggerDisplay("{Value} (Scalar Type: {ValueType})")]
    public class ScalarValueNode : InputValueNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScalarValueNode"/> class.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="valueType">Type of the value.</param>
        /// <param name="value">The value.</param>
        public ScalarValueNode(
            SourceLocation location,
            ScalarValueType valueType,
            ReadOnlyMemory<char> value)
            : base(location, value)
        {
            this.ValueType = valueType;
        }

        /// <summary>
        /// Gets the qualifying type of the <see cref="InputValueNode.Value"/>.
        /// </summary>
        /// <value>The type of the value.</value>
        public ScalarValueType ValueType { get; }
    }
}