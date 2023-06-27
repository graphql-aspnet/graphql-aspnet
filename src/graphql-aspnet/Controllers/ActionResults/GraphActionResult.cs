// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Controllers.ActionResults
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Controllers.ActionResults.Batching;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A helper class to allow the use of common <see cref="IGraphActionResult"/> methods
    /// with non-controller based resolvers.
    /// </summary>
    public static class GraphActionResult
    {
        /// <summary>
        /// Returns an result with the given item as the resolved value for the field.
        /// </summary>
        /// <param name="item">The object to resolve the field with.</param>
        /// <returns>IGraphActionResult&lt;TResult&gt;.</returns>
        public static IGraphActionResult Ok(object item)
        {
            return new ObjectReturnedGraphActionResult(item);
        }

        /// <summary>
        /// Returns a result indicating that <c>null</c> is the resolved value
        /// for the field.
        /// </summary>
        /// <returns>IGraphActionResult&lt;TResult&gt;.</returns>
        public static IGraphActionResult Ok()
        {
            return Ok(null);
        }

        /// <summary>
        /// Returns an error indicating that the issue indicated by <paramref name="message"/> occured.
        /// </summary>
        /// <param name="message">The human-friendly error message to assign ot the reported error in the graph result.</param>
        /// <param name="code">The code to assign to the error entry in the graph result.</param>
        /// <param name="exception">An optional exception that generated this error.</param>
        /// <returns>IGraphActionResult.</returns>
        public static IGraphActionResult Error(
            string message,
            string code = null,
            Exception exception = null)
        {
            return new GraphFieldErrorActionResult(message, code, exception);
        }

        /// <summary>
        /// Returns an error indicating that the issue indicated by <paramref name="message"/> occured.
        /// </summary>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="message">The human-friendly error message to assign ot the reported error in the graph result.</param>
        /// <param name="code">The error code to assign to the reported error in the graph result.</param>
        /// <param name="exception">An optional exception to be published if the query is configured to allow exposing exceptions.</param>
        /// <returns>IGraphActionResult.</returns>
        public static IGraphActionResult Error(
            GraphMessageSeverity severity,
            string message,
            string code = null,
            Exception exception = null)
        {
            var errorMessage = new GraphExecutionMessage(severity, message, code, exception: exception);
            return Error(errorMessage);
        }

        /// <summary>
        /// Returns an error indicating that the given issue occured.
        /// </summary>
        /// <param name="message">A custom generated error message.</param>
        /// <returns>IGraphActionResult.</returns>
        public static IGraphActionResult Error(IGraphMessage message)
        {
            return new GraphFieldErrorActionResult(message);
        }

        /// <summary>
        /// Returns an error result indicating that processing failed due to some internal process. An exception
        /// will be injected into the graph result and processing will be terminated.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>IGraphActionResult.</returns>
        public static IGraphActionResult InternalServerError(string errorMessage)
        {
            return new InternalServerErrorGraphActionResult(errorMessage);
        }

        /// <summary>
        /// Returns an error indicating that something could not be resolved correctly
        /// with the information provided.
        /// </summary>
        /// <param name="message">The message indicating what was not found.</param>
        /// <returns>IGraphActionResult.</returns>
        public static IGraphActionResult NotFound(string message)
        {
            return new RouteNotFoundGraphActionResult(message);
        }

        /// <summary>
        /// Returns an negative result, indicating the data supplied on the request was bad or
        /// otherwise not usable by the controller method.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>IGraphActionResult.</returns>
        public static IGraphActionResult BadRequest(string message)
        {
            return new BadRequestGraphActionResult(message);
        }

        /// <summary>
        /// Returns a negative result, indicating that the action requested was unauthorized for the current context.
        /// </summary>
        /// <param name="message">The message to return to the client.</param>
        /// <param name="errorCode">The error code to apply to the error returned to the client.</param>
        /// <returns>IGraphActionResult.</returns>
        public static IGraphActionResult Unauthorized(string message = null, string errorCode = null)
        {
            return new UnauthorizedGraphActionResult(message, errorCode);
        }

        /// <summary>
        /// Begings building a result capable of mapping a batch result to the original source items to correctly resolve
        /// the field. (e.g. it will create a single item reference or a collection of children as the field
        /// requires). An <see cref="InternalServerErrorGraphActionResult" /> will be
        /// generated indicating an issue if the batch produced cannot fulfill the requirements of the field.
        /// This method will not throw an exception.
        /// </summary>
        /// <param name="targetField">The graph field the batch will be built for.</param>
        /// <returns>IGraphActionResult.</returns>
        public static BatchBuilder StartBatch(IGraphField targetField)
        {
            Validation.ThrowIfNull(targetField, nameof(targetField));
            return new BatchBuilder(targetField);
        }
    }
}