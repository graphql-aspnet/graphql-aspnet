// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.Exceptions
{
    using System;

    /// <summary>
    /// Thrown when a middleware component fails to complete its required operation correctly.
    /// </summary>
    public class GraphPipelineMiddlewareInvocationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphPipelineMiddlewareInvocationException"/> class.
        /// </summary>
        /// <param name="componentName">Name of the component.</param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public GraphPipelineMiddlewareInvocationException(string componentName, string message, Exception innerException = null)
            : base(message, innerException)
        {
            this.ComponentName = componentName;
        }

        /// <summary>
        /// Gets the name friendly name of the middleware component that caused the exception.
        /// </summary>
        /// <value>The name of the component.</value>
        public string ComponentName { get; }
    }
}