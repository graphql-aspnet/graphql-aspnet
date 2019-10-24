// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Controllers
{
    using System;
    using GraphQL.AspNet.Controllers.ActionResults;
    using GraphQL.AspNet.Controllers.ActionResults.Batching;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A base class to provide common functionality for register mutation functions in a graph query.
    /// </summary>
    public abstract partial class GraphController
    {
        /// <summary>
        /// Returns an action with the given object as the result provided to graphQL as the resolved object for the field query.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>IGraphActionResult&lt;TResult&gt;.</returns>
        protected virtual IGraphActionResult Ok(object obj)
        {
            return new ObjectReturnedGraphActionResult(obj);
        }

        /// <summary>
        /// Returns an error indicating that an issue occured.
        /// </summary>
        /// <param name="message">The human-friendly error message to assign ot the reported error in the graph result.</param>
        /// <param name="code">The error code to assign to the reported error in the graph result.</param>
        /// <param name="exception">An optional exception to be published if the query is configured to allow exposing exceptions.</param>
        /// <returns>IGraphActionResult.</returns>
        protected virtual IGraphActionResult Error(
            string message,
            string code = null,
            Exception exception = null)
        {
            return new GraphFieldErrorActionResult(message, code, exception);
        }

        /// <summary>
        /// Returns an error indicating that an issue occured.
        /// </summary>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="message">The human-friendly error message to assign ot the reported error in the graph result.</param>
        /// <param name="code">The error code to assign to the reported error in the graph result.</param>
        /// <param name="exception">An optional exception to be published if the query is configured to allow exposing exceptions.</param>
        /// <returns>IGraphActionResult.</returns>
        protected virtual IGraphActionResult Error(
            GraphMessageSeverity severity,
            string message,
            string code = null,
            Exception exception = null)
        {
            var errorMessage = new GraphExecutionMessage(severity, message, code, exception: exception);
            return this.Error(errorMessage);
        }

        /// <summary>
        /// Returns an error indicating that an issue occured.
        /// </summary>
        /// <param name="message">A custom generated message.</param>
        /// <returns>IGraphActionResult.</returns>
        protected virtual IGraphActionResult Error(
            IGraphMessage message)
        {
            return new GraphFieldErrorActionResult(message);
        }

        /// <summary>
        /// Returns an error indicating that the requested query projection was not found on the schema or could not be
        /// resolved correctly with the information provided.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>IGraphActionResult.</returns>
        protected virtual IGraphActionResult NotFound(string message)
        {
            return new RouteNotFoundGraphActionResult(message);
        }

        /// <summary>
        /// Begings building a result capable of mapping a batch result to the original source items to correctly resolve
        /// the field. (e.g. it will create a single item reference or a collection of children as the field
        /// requires). An <see cref="InternalServerErrorGraphActionResult" /> will be
        /// generated indicating an issue if the batch produced cannot fulfill the requirements of the field.
        /// This method will not throw an exception.
        /// </summary>
        /// <returns>IGraphActionResult.</returns>
        protected virtual BatchBuilder StartBatch()
        {
            return new BatchBuilder(this.Request.InvocationContext.Field);
        }
    }
}