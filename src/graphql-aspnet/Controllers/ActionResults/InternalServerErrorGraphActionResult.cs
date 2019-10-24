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
    /// A graph action result returned when the controller method encountered an error
    /// and was unable to recover or continue.
    /// </summary>
    /// <seealso cref="IGraphActionResult" />
    public class InternalServerErrorGraphActionResult : IGraphActionResult
    {
        private readonly string _errorMessage;
        private readonly IGraphMethod _action;
        private readonly Exception _exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalServerErrorGraphActionResult"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message sent to the client.</param>
        public InternalServerErrorGraphActionResult(string errorMessage)
        {
            _errorMessage = errorMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalServerErrorGraphActionResult"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message sent to the client.</param>
        /// <param name="exception">The exception, if any, that was thrown. Useful for logging or other intermediate actions.</param>
        public InternalServerErrorGraphActionResult(string errorMessage, Exception exception)
            : this(errorMessage)
        {
            _exception = exception;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalServerErrorGraphActionResult" /> class.
        /// </summary>
        /// <param name="action">The action that was invoked to cause this internal error, if any.</param>
        /// <param name="exception">The exception, if any, that was thrown. Useful for logging or other intermediate actions.</param>
        public InternalServerErrorGraphActionResult(IGraphMethod action, Exception exception)
        {
            _action = action;
            _exception = exception;
        }

        /// <summary>
        /// Processes the provided resolution context against this action result instance to
        /// generate the expected response in accordance with this instance's rule set.
        /// </summary>
        /// <param name="context">The context being processed.</param>
        /// <returns>Task.</returns>
        public Task Complete(ResolutionContext context)
        {
            var message = _errorMessage ?? $"An unhandled exception was thrown during the execution of field '{_action?.Name ?? "-unknown-"}'.";
            context.Messages.Critical(
                message,
                Constants.ErrorCodes.UNHANDLED_EXCEPTION,
                context.Request.Origin,
                _exception);

            context.Cancel();
            return Task.CompletedTask;
        }
    }
}