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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Middleware.FieldExecution;

    /// <summary>
    /// An action result thrown when the requested route was not found or otherwise
    /// not navigable.
    /// </summary>
    /// <seealso cref="IGraphActionResult" />
    public class RouteNotFoundGraphActionResult : IGraphActionResult
    {
        private readonly IGraphMethod _invokeDef;
        private readonly Exception _thrownException;
        private readonly string _message;

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteNotFoundGraphActionResult" /> class.
        /// </summary>
        /// <param name="invokedAction">The invoked action at the route location.</param>
        /// <param name="thrownException">The thrown exception that occured when invoking the action, if any.</param>
        public RouteNotFoundGraphActionResult(IGraphMethod invokedAction, Exception thrownException = null)
        {
            _invokeDef = invokedAction;
            _thrownException = thrownException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteNotFoundGraphActionResult"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public RouteNotFoundGraphActionResult(string message)
        {
            _message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteNotFoundGraphActionResult"/> class.
        /// </summary>
        public RouteNotFoundGraphActionResult()
        {
        }

        /// <summary>
        /// Processes the provided resolution context against this action result instance to
        /// generate the expected response in accordance with this instance's rule set.
        /// </summary>
        /// <param name="context">The context being processed.</param>
        /// <returns>Task.</returns>
        public Task Complete(ResolutionContext context)
        {
            if (_invokeDef != null)
            {
                context.Messages.Critical(
                    $"The field '{_invokeDef.Name}' was not found or could not be invoked.",
                    Constants.ErrorCodes.INVALID_ROUTE,
                    context.Request.Origin,
                    _thrownException);
            }
            else if (!string.IsNullOrWhiteSpace(_message))
            {
                context.Messages.Critical(
                    _message,
                    Constants.ErrorCodes.INVALID_ROUTE,
                    context.Request.Origin);
            }
            else
            {
                context.Messages.Critical(
                    "The item was not routable or otherwise not available.",
                    Constants.ErrorCodes.INVALID_ROUTE,
                    context.Request.Origin);
            }

            context.Cancel();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the message that was supplied, if any.
        /// </summary>
        /// <value>The message.</value>
        public string Message => _message;
    }
}