// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts.SuppliedValues
{
    using System;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// An input value representing a single enumeration value of data.
    /// </summary>
    public class DocumentEnumSuppliedValue : DocumentSuppliedValue, IResolvableValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentEnumSuppliedValue" /> class.
        /// </summary>
        /// <param name="value">The value parsed from a query document.</param>
        public DocumentEnumSuppliedValue(EnumValueNode value)
            : base(value)
        {
            this.Value = value.Value;
        }

        /// <summary>
        /// Gets the value literal of the data passed to the input value.
        /// </summary>
        /// <value>The value.</value>
        public ReadOnlyMemory<char> Value { get; }

        /// <summary>
        /// Gets the value to be used to resolve to some .NET type.
        /// </summary>
        /// <value>The resolvable value.</value>
        public ReadOnlySpan<char> ResolvableValue => this.Value.Span;
    }
}