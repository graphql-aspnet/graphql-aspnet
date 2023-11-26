﻿// *************************************************************
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
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A graph action result returned when the controller method encountered an error
    /// and was unable to recover or continue.
    /// </summary>
    /// <seealso cref="IGraphActionResult" />
    public class InternalServerErrorGraphActionResult : IGraphActionResult
    {
        private readonly string _errorMessage;
        private readonly IGraphFieldResolverMetaData _action;
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
        public InternalServerErrorGraphActionResult(IGraphFieldResolverMetaData action, Exception exception)
        {
            _action = action;
            _errorMessage = $"An unhandled exception was thrown during the execution of an action.  See inner exception for details.";

            _exception = exception;

            if (_exception == null)
                _exception = new Exception($"The action method '{action?.InternalName}' failed to complete successfully but did not record an exception.");
        }

        /// <inheritdoc />
        public Task CompleteAsync(SchemaItemResolutionContext context)
        {
            context.Messages.Critical(
                _errorMessage,
                Constants.ErrorCodes.INTERNAL_SERVER_ERROR,
                context.Request.Origin,
                _exception);

            context.Cancel();
            return Task.CompletedTask;
        }
    }
}