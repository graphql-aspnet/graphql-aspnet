// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution.Variables
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

        /// <summary>
        /// Gets a value indicating whether this resolved variable value represents a declared default value.
        /// </summary>
        /// <remarks>
        /// When <c>false</c>, indicates that the <see cref="Value"/> was explicitly supplied. When <c>true</c>, indicates that
        /// no variable value was supplied and that <see cref="Value"/> is the default value declared on the operation.
        /// </remarks>
        /// <value><c>true</c> if this instance is default value; otherwise, <c>false</c>.</value>
        bool IsDefaultValue { get; }
    }
}