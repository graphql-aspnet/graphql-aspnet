// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Variables
{
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// A representation of a user supplied variable AFTER it has been resolved against a
    /// graph type for a specific operation.
    /// </summary>
    internal class ResolvedVariable : IResolvedVariable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResolvedVariable" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="typeExpression">The type expression.</param>
        /// <param name="value">The value.</param>
        /// <param name="isDefaultValue">if set to <c>true</c>, indicates that <paramref name="value"/>
        /// is the default value declared on the original variable definition..</param>
        public ResolvedVariable(string name, GraphTypeExpression typeExpression, object value, bool isDefaultValue = false)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(name, nameof(name));
            this.TypeExpression = Validation.ThrowIfNullOrReturn(typeExpression, nameof(typeExpression));
            this.Value = value;
            this.IsDefaultValue = isDefaultValue;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public object Value { get; }

        /// <inheritdoc />
        public GraphTypeExpression TypeExpression { get; }

        /// <inheritdoc />
        public bool IsDefaultValue { get; }
    }
}