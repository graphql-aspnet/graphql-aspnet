// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Variables
{
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// A representation of a user supplied variable AFTER it has been resolved against a
    /// graph type for a specific operation.
    /// </summary>
    public interface IResolvedVariable
    {
        /// <summary>
        /// Gets the name of the variable as it was declared in the user's supplied data.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the value of the variable as a fully resolved .NET object.
        /// </summary>
        /// <value>The value.</value>
        object Value { get; }

        /// <summary>
        /// Gets the type expression this variable represents.
        /// </summary>
        /// <value>The type expression.</value>
        GraphTypeExpression TypeExpression { get; }
    }
}