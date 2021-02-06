// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Exceptions
{
    using System;

    /// <summary>
    /// Thrown when reading a graph type via reflection fails to complete.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class GraphTypeDeclarationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphTypeDeclarationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public GraphTypeDeclarationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphTypeDeclarationException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="failedType">The .net type that failed conversion to a
        /// graph type.</param>
        public GraphTypeDeclarationException(string message, Type failedType)
            : base(message)
        {
            this.FailedGraphType = failedType;
        }

        /// <summary>
        /// Gets the type that was being evaluated for conversion to a Graph Type.
        /// </summary>
        /// <value>The type of the failed graph.</value>
        public Type FailedGraphType { get; }
    }
}