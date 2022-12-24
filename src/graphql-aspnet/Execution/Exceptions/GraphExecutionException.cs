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
    using GraphQL.AspNet.Execution.Source;

    /// <summary>
    /// An exception thrown in context of processing a query. The data contained
    /// in this exception is "internal" and may contain sensitive information that
    /// should not be shared..
    /// </summary>
    public class GraphExecutionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphExecutionException" /> class.
        /// </summary>
        /// <param name="message">The message to capture on this exception.</param>
        /// <param name="origin">The origin in a query document or execution context where this exception
        /// occured.</param>
        /// <param name="innerException">The exception that is the cause of the current
        /// exception, if any.</param>
        public GraphExecutionException(
            string message,
            SourceOrigin origin = default,
            Exception innerException = null)
            : base(message, innerException)
        {
            this.Origin = origin;
        }

        /// <summary>
        /// Gets the location being processed in the source document when this exception was thrown.
        /// </summary>
        /// <value>The origin.</value>
        public SourceOrigin Origin { get; }
    }
}