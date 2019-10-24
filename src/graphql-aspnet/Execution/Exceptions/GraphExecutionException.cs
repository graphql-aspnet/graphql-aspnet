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
    using GraphQL.AspNet.Common.Source;

    /// <summary>
    /// An exception thrown in context of processing a graph pipeline action. The data contained
    /// in this exception is "internal" and not expected to be part of the graph query output.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class GraphExecutionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphExecutionException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="origin">The origin.</param>
        /// <param name="innerException">The exception that is the cause of the current exception if any.</param>
        public GraphExecutionException(
            string message,
            SourceOrigin origin = null,
            Exception innerException = null)
            : base(message, innerException)
        {
            this.Origin = origin;
        }

        /// <summary>
        /// Gets the origin location being processed in the source document where this exception was thrown.
        /// </summary>
        /// <value>The origin.</value>
        public SourceOrigin Origin { get; }
    }
}