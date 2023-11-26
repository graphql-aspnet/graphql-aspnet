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
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// An action result returned when the requested field on a type
    /// was not found or was otherwise not navigable.
    /// </summary>
    /// <seealso cref="IGraphActionResult" />
    public class PathNotFoundGraphActionResult : IGraphActionResult
    {
        private readonly IGraphFieldResolverMetaData _invokeDef;
        private readonly Exception _thrownException;
        private readonly string _message;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathNotFoundGraphActionResult" /> class.
        /// </summary>
        /// <param name="targetResolver">The resolver that was being targeted at item path location.</param>
        /// <param name="thrownException">The thrown exception that occured when invoking the action, if any.</param>
        public PathNotFoundGraphActionResult(IGraphFieldResolverMetaData targetResolver, Exception thrownException = null)
        {
            _invokeDef = targetResolver;
            _thrownException = thrownException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathNotFoundGraphActionResult"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public PathNotFoundGraphActionResult(string message)
        {
            _message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathNotFoundGraphActionResult"/> class.
        /// </summary>
        public PathNotFoundGraphActionResult()
        {
        }

        /// <inheritdoc />
        public Task CompleteAsync(SchemaItemResolutionContext context)
        {
            context.Cancel();

            if (!string.IsNullOrWhiteSpace(_message))
            {
                context.Messages.Critical(
                    _message,
                    Constants.ErrorCodes.INVALID_PATH,
                    context.Request.Origin);

                return Task.CompletedTask;
            }

            string fieldName = context.ItemPath?.Path;
            if (string.IsNullOrWhiteSpace(fieldName))
                fieldName = "~Unknown~";

            if (_invokeDef != null)
            {
                var exception = new Exception(
                    $"The resolver '{_invokeDef.InternalName}' with {_invokeDef.Parameters.Count} was not invocable with the provided data on " +
                    $"the request.",
                    _thrownException);

                context.Messages.Critical(
                    $"The field '{fieldName}' was not found or the resolver could not be invoked.",
                    Constants.ErrorCodes.INVALID_PATH,
                    context.Request.Origin,
                    exception);
            }
            else
            {
                context.Messages.Critical(
                    $"The field '{fieldName}' was not routable or otherwise not available.",
                    Constants.ErrorCodes.INVALID_PATH,
                    context.Request.Origin,
                    _thrownException);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the message that was supplied, if any.
        /// </summary>
        /// <value>The message.</value>
        public string Message => _message;
    }
}