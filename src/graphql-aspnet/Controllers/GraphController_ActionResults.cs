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
    using GraphQL.AspNet.Controllers.InputModel;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A base class from which all graph controllers must inherit.
    /// </summary>
    public abstract partial class GraphController
    {
        /// <summary>
        /// Returns an result with the given item as the resolved value for the field.
        /// </summary>
        /// <param name="item">The object to resolve the field with.</param>
        /// <returns>IGraphActionResult&lt;TResult&gt;.</returns>
        protected virtual IGraphActionResult Ok(object item)
        {
            return GraphActionResult.Ok(item);
        }

        /// <summary>
        /// Returns a result indicating that <c>null</c> is the resolved value
        /// for the field.
        /// </summary>
        /// <returns>IGraphActionResult&lt;TResult&gt;.</returns>
        protected virtual IGraphActionResult Ok()
        {
            return GraphActionResult.Ok();
        }

        /// <summary>
        /// Returns an error indicating that the issue indicated by <paramref name="message"/> occured.
        /// </summary>
        /// <param name="message">The human-friendly error message to assign ot the reported error in the graph result.</param>
        /// <param name="code">The code to assign to the error entry in the graph result.</param>
        /// <param name="exception">An optional exception that generated this error.</param>
        /// <returns>IGraphActionResult.</returns>
        protected virtual IGraphActionResult Error(
            string message,
            string code = null,
            Exception exception = null)
        {
            return GraphActionResult.Error(message, code, exception);
        }

        /// <summary>
        /// Returns an error indicating that the issue indicated by <paramref name="message"/> occured.
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
            return GraphActionResult.Error(severity, message, code, exception: exception);
        }

        /// <summary>
        /// Returns an error indicating that the given issue occured.
        /// </summary>
        /// <param name="message">A custom generated error message.</param>
        /// <returns>IGraphActionResult.</returns>
        protected virtual IGraphActionResult Error(IGraphMessage message)
        {
            return GraphActionResult.Error(message);
        }

        /// <summary>
        /// Returns an error result indicating that processing failed due to some internal process. An exception
        /// will be injected into the graph result and processing will be terminated.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>IGraphActionResult.</returns>
        protected virtual IGraphActionResult InternalServerError(string errorMessage)
        {
            return GraphActionResult.InternalServerError(errorMessage);
        }

        /// <summary>
        /// Returns an error indicating that something could not be resolved correctly
        /// with the information provided.
        /// </summary>
        /// <param name="message">The message indicating what was not found.</param>
        /// <returns>IGraphActionResult.</returns>
        protected virtual IGraphActionResult NotFound(string message)
        {
            return GraphActionResult.NotFound(message);
        }

        /// <summary>
        /// Returns an negative result, indicating the data supplied on the request was bad or
        /// otherwise not usable by the controller method.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>IGraphActionResult.</returns>
        protected virtual IGraphActionResult BadRequest(string message)
        {
            return GraphActionResult.BadRequest(message);
        }

        /// <summary>
        /// Returns an negative result, indicating the data supplied on the request was bad or
        /// otherwise not usable by the controller method.
        /// </summary>
        /// <param name="modelState">The model state with its contained validation failures.</param>
        /// <returns>IGraphActionResult.</returns>
        protected virtual IGraphActionResult BadRequest(InputModelStateDictionary modelState)
        {
            return new BadRequestGraphActionResult(modelState);
        }

        /// <summary>
        /// Returns a negative result, indicating that the action requested was unauthorized for the current context.
        /// </summary>
        /// <param name="message">The message to return to the client.</param>
        /// <param name="errorCode">The error code to apply to the error returned to the client.</param>
        /// <returns>IGraphActionResult.</returns>
        protected virtual IGraphActionResult Unauthorized(string message = null, string errorCode = null)
        {
            return GraphActionResult.Unauthorized(message, errorCode);
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
            return GraphActionResult.StartBatch();
        }
    }
}