// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts.QueryInputValues
{
    using System;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// An input value representing a single enumeration value of data.
    /// </summary>
    public class QueryEnumInputValue : QueryInputValue, IResolvableValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryEnumInputValue" /> class.
        /// </summary>
        /// <param name="value">The value parsed from a query document.</param>
        public QueryEnumInputValue(EnumValueNode value)
            : base(value)
        {
            this.Value = value.Value;
        }

        /// <summary>
        /// Adds the argument to the collection of arguments on this instance.
        /// </summary>
        /// <param name="child">The child.</param>
        public override void AddChild(IDocumentPart child)
        {
            throw new InvalidOperationException($"{nameof(QueryEnumInputValue)} cannot contain children of type '{child?.GetType()}'");
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
        ReadOnlySpan<char> IResolvableValue.ResolvableValue => this.Value.Span;
    }
}