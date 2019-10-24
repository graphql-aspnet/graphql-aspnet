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
    using System.Reflection;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Controllers.ActionResults;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.InputModel;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Middleware.FieldExecution;

    /// <summary>
    /// A base object providing common method used by invocable method containers authored by developers.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request this controller item is expected to process.</typeparam>
    public abstract class GraphControllerBase<TRequest>
        where TRequest : class, IControllerActionInvocationRequest
    {
        private IGraphMethod _action;
        private IGraphEventLogger _logger;

        private BaseResolutionContext<TRequest> _context;

        /// <summary>
        /// Invoke the specified action method as an asynchronous operation.
        /// </summary>
        /// <param name="actionToInvoke">The action to invoke.</param>
        /// <param name="context">The context.</param>
        /// <returns>Task&lt;System.Object&gt;.</returns>
        public async Task<object> InvokeActionAsync(
            IGraphMethod actionToInvoke,
            BaseResolutionContext<TRequest> context)
        {
            // deconstruct the context for processing
            _action = actionToInvoke;

            var fieldRequest = context.Request;
            this.Request = Validation.ThrowIfNullOrReturn(fieldRequest, nameof(fieldRequest));
            _context = context;
            _logger = context?.Logger;
            _logger?.ActionMethodInvocationRequestStarted(_action, this.Request);

            if (_action?.Method == null)
            {
                return new InternalServerErrorGraphActionResult(
                    $"The definition for field '{this.GetType().Name}' defined no graph action to execute. Operation failed.");
            }

            try
            {
                var modelGenerator = new ModelStateGenerator(context.ServiceProvider);
                this.ModelState = modelGenerator.CreateStateDictionary(context.Arguments);

                _logger?.ActionMethodModelStateValidated(_action, this.Request, this.ModelState);

                if (_action.Method.DeclaringType != this.GetType())
                {
                    throw new TargetException($"Unable to invoke action '{_action.Route.Path}' on controller '{this.GetType().FriendlyName()}'. The controller " +
                                              "does not own the method.");
                }

                var invoker = InstanceFactory.CreateInstanceMethodInvoker(_action.Method);
                var invocationParameters = context.Arguments.PrepareArguments(_action);

                var invokeReturn = invoker(this, invocationParameters);
                if (_action.IsAsyncField)
                {
                    if (invokeReturn is Task task)
                    {
                        await task.ConfigureAwait(false);
                        if (task.IsFaulted)
                            throw task.UnwrapException();

                        invokeReturn = task.ResultOrDefault();
                    }
                    else
                    {
                        // given all the checking and parsing this should be imnpossible, but just in case
                        invokeReturn = new InternalServerErrorGraphActionResult($"The action '{_action.Route.Path}' is defined " +
                                                        $"as asyncronous but it did not return a {typeof(Task)}.");
                    }
                }

                _logger?.ActionMethodInvocationCompleted(_action, this.Request, invokeReturn);
                return invokeReturn;
            }
            catch (TargetInvocationException ti)
            {
                var innerException = ti.InnerException ?? ti;
                _logger?.ActionMethodInvocationException(_action, this.Request, innerException);

                return new InternalServerErrorGraphActionResult(_action, innerException);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    // catch any other invocation exceptions and treat them as an invalid route
                    // might happen if a method was declared differently than the actual call signature
                    case TargetException _:
                    case TargetParameterCountException _:
                        _logger?.ActionMethodInvocationException(_action, this.Request, ex);
                        return new RouteNotFoundGraphActionResult(_action, ex);

                    default:
                        // total failure by the user's action code.
                        // record and bubble
                        _logger?.ActionMethodUnhandledException(_action, this.Request, ex);
                        throw;
                }
            }
        }

        /// <summary>
        /// Returns an error result indicating that processing failed due to some internal process. An exception
        /// will be injected into the graph result and processing will be terminated.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>IGraphActionResult.</returns>
        protected virtual IGraphActionResult InternalServerError(string errorMessage)
        {
            return new InternalServerErrorGraphActionResult(errorMessage);
        }

        /// <summary>
        /// Returns an negative result, indicating the data supplied on the request was bad or
        /// otherwise not usable by the controller method.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>IGraphActionResult.</returns>
        protected virtual IGraphActionResult BadRequest(string message)
        {
            return new BadRequestGraphActionResult(message);
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
            return new UnauthorizedGraphActionResult(errorCode ?? "Unauthorized", message ?? string.Empty);
        }

        /// <summary>
        /// Gets the state of the model being represented on this controller action invocation.
        /// </summary>
        /// <value>The state of the model.</value>
        public InputModelStateDictionary ModelState { get; private set; }

        /// <summary>
        /// Gets the raw request for this operation provided by the action invoker.
        /// </summary>
        /// <value>The field context.</value>
        protected TRequest Request { get; private set; }

        /// <summary>
        /// Gets the resolved <see cref="ClaimsPrincipal"/> that was passed recieved on the request.
        /// </summary>
        /// <value>The user.</value>
        protected ClaimsPrincipal User => _context.User;

        /// <summary>
        /// Gets the scoped <see cref="IServiceProvider"/> supplied to the original controller action that is being invoked.
        /// </summary>
        /// <value>The request services.</value>
        protected IServiceProvider RequestServices => _context.ServiceProvider;
    }
}