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
    }
}