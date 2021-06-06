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
    /// An action result indicating that the <see cref="IGraphMethod"/> errored or in some way did
    /// not fulfill its request for data in an acceptable manner. This result is always interpreted as a
    /// critical error and stops execution of the current query.
    /// </summary>
    public class GraphFieldErrorActionResult : IGraphActionResult
    {
        private readonly string _message;
        private readonly string _code;
        private readonly Exception _exception;
        private readonly IGraphMessage _customMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldErrorActionResult"/> class.
        /// </summary>
        /// <param name="message">A custom generated graph message to add to the result.</param>
        public GraphFieldErrorActionResult(IGraphMessage message)
        {
            _customMessage = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldErrorActionResult" /> class.
        /// </summary>
        /// <param name="message">The human-friendly error message to assign ot the reported error in the graph result.</param>
        /// <param name="code">The error code to assign to the reported error in the graph result.</param>
        /// <param name="exception">The exception that was captured and should be carried with this result for logging and further processing. May be null.</param>
        public GraphFieldErrorActionResult(string message, string code = null, Exception exception = null)
        {
            _message = message;
            _code = code;
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
            if (_customMessage != null)
            {
                context.Messages.Add(_customMessage);
            }
            else
            {
                context.Messages.Critical(
                        _message ?? "An unhandled exception occured.",
                        _code,
                        context.Request.Origin,
                        _exception);
            }

            context.Cancel();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the custom message provided for this result.
        /// </summary>
        /// <value>The message.</value>
        public string Message => _message;

        /// <summary>
        /// Gets the error code that was given for this result.
        /// </summary>
        /// <value>The code.</value>
        public string Code => _code;

        /// <summary>
        /// Gets the exception that was supplied to this error result, if any.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception => _exception;
    }
}