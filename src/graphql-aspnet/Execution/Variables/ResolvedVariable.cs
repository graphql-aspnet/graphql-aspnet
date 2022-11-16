// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Variables
{
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Variables;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// A representation of a user supplied variable AFTER it has been resolved against a
    /// graph type for a specific operation.
    /// </summary>
    public class ResolvedVariable : IResolvedVariable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResolvedVariable"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="typeExpression">The type expression.</param>
        /// <param name="value">The value.</param>
        public ResolvedVariable(string name, GraphTypeExpression typeExpression, object value)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(name, nameof(name));
            this.TypeExpression = Validation.ThrowIfNullOrReturn(typeExpression, nameof(typeExpression));
            this.Value = value;
        }

        /// <summary>
        /// Gets the name of the variable as it was declared in the user's supplied data.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the value of the variable as a fully resolved .NET object.
        /// </summary>
        /// <value>The value.</value>
        public object Value { get; }

        /// <summary>
        /// Gets the type expression this variable represents.
        /// </summary>
        /// <value>The type expression.</value>
        public GraphTypeExpression TypeExpression { get; }
    }
}