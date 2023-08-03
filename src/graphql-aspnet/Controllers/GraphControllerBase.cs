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
    using System.Data.SqlTypes;
    using System.Reflection;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Controllers.ActionResults;
    using GraphQL.AspNet.Controllers.InputModel;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A common object providing methods used by both directives and controllers.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request this instance is
    /// expected to process.</typeparam>
    public abstract class GraphControllerBase<TRequest>
        where TRequest : class, IDataRequest
    {
        private SchemaItemResolutionContext<TRequest> _schemaItemContext;
        private IGraphFieldResolverMetaData _resolverMetaData;

        /// <summary>
        /// Invoke the specified action method as an asynchronous operation.
        /// </summary>
        /// <param name="resolverMetadata">The metadata describing the method on this controller to invoke.</param>
        /// <param name="schemaItemContext">The invocation context to process.</param>
        /// <returns>Task&lt;System.Object&gt;.</returns>
        [GraphSkip]
        internal virtual async Task<object> InvokeActionAsync(
            IGraphFieldResolverMetaData resolverMetadata,
            SchemaItemResolutionContext<TRequest> schemaItemContext)
        {
            // deconstruct the context for processing
            Validation.ThrowIfNull(schemaItemContext, nameof(schemaItemContext));
            _schemaItemContext = schemaItemContext;
            _resolverMetaData = resolverMetadata;

            // ensure a field request is available
            var fieldRequest = schemaItemContext?.Request;
            Validation.ThrowIfNull(fieldRequest, nameof(fieldRequest));

            _schemaItemContext.Logger?.ActionMethodInvocationRequestStarted(_resolverMetaData, this.Request);

            if (_schemaItemContext.QueryRequest is IQueryExecutionWebRequest webRequest)
                this.HttpContext = webRequest.HttpContext;

            if (_resolverMetaData?.Method == null)
            {
                return new InternalServerErrorGraphActionResult(
                    $"The definition for field '{this.GetType().Name}' defined no resolver to execute. Operation failed.");
            }

            try
            {
                var modelGenerator = new ModelStateGenerator(schemaItemContext.ServiceProvider);
                this.ModelState = modelGenerator.CreateStateDictionary(schemaItemContext.ExecutionSuppliedArguments);

                _schemaItemContext.Logger?.ActionMethodModelStateValidated(_resolverMetaData, this.Request, this.ModelState);

                var invocationParameters = _schemaItemContext.ExecutionSuppliedArguments.PrepareArguments(_resolverMetaData);
                var invokeReturn = this.CreateAndInvokeAction(_resolverMetaData, invocationParameters);
                if (_resolverMetaData.IsAsyncField)
                {
                    if (invokeReturn is Task task)
                    {
                        await task.ConfigureAwait(false);
                        if (task.IsFaulted)
                            throw task.UnwrapException();

                        invokeReturn = task.ResultOfTypeOrNull(_resolverMetaData.ExpectedReturnType);
                    }
                    else
                    {
                        // given all the checking and parsing this should be impossible, but just in case
                        invokeReturn = new InternalServerErrorGraphActionResult(
                            $"The action '{_resolverMetaData.InternalName}' on controller '{_resolverMetaData.ParentInternalName}' is defined " +
                            $"as asyncronous but it did not return a {typeof(Task)}.");
                    }
                }

                _schemaItemContext.Logger?.ActionMethodInvocationCompleted(_resolverMetaData, this.Request, invokeReturn);
                return invokeReturn;
            }
            catch (TargetInvocationException ti)
            {
                var innerException = ti.InnerException ?? ti;
                _schemaItemContext.Logger?.ActionMethodInvocationException(_resolverMetaData, this.Request, innerException);

                return new InternalServerErrorGraphActionResult(_resolverMetaData, innerException);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    // catch any other invocation exceptions and treat them as an invalid route
                    // might happen if a method was declared differently than the actual call signature
                    case TargetException _:
                    case TargetParameterCountException _:
                        _schemaItemContext.Logger?.ActionMethodInvocationException(_resolverMetaData, this.Request, ex);
                        return new RouteNotFoundGraphActionResult(_resolverMetaData, ex);

                    default:

                        // total failure by the user's action code.
                        // record and bubble
                        _schemaItemContext.Logger?.ActionMethodUnhandledException(_resolverMetaData, this.Request, ex);
                        throw;
                }
            }
        }

        /// <summary>
        /// Invoke the actual C# method declared by the <paramref name="resolverMetaData"/>
        /// using this controller instance as the target object of the invocation.
        /// </summary>
        /// <param name="resolverMetaData">The resolver declaration that needs to be executed.</param>
        /// <param name="invocationArguments">The realized set of arguments that need
        /// to be passed to the invocable method instance.</param>
        /// <returns>The exact return value from the invoked resolver.</returns>
        protected virtual object CreateAndInvokeAction(IGraphFieldResolverMetaData resolverMetaData, object[] invocationArguments)
        {
            switch (resolverMetaData.DefinitionSource)
            {
                case ItemSource.DesignTime:
                    if (!Validation.IsCastable(this.GetType(), resolverMetaData.Method.DeclaringType))
                    {
                        throw new TargetException($"Unable to invoke action '{_resolverMetaData.InternalName}' on controller '{this.GetType().FriendlyName()}'. The controller " +
                                                  "does not own the method.");
                    }

                    if (resolverMetaData.Method.IsStatic)
                    {
                        throw new TargetException($"Unable to invoke action '{_resolverMetaData.InternalName}' on controller '{this.GetType().FriendlyName()}'. The method " +
                                                  "is static and cannot be directly invoked on this controller instance.");
                    }

                    var ctrlInvoker = InstanceFactory.CreateInstanceMethodInvoker(resolverMetaData.Method);
                    var controllerRef = this as object;
                    return ctrlInvoker(ref controllerRef, invocationArguments);

                case ItemSource.Runtime:
                    // minimal api resolvers are allowed to be static since there is no
                    // extra context to setup or make available such as 'this.User' etc.
                    if (resolverMetaData.Method.IsStatic)
                    {
                        var staticInvoker = InstanceFactory.CreateStaticMethodInvoker(resolverMetaData.Method);
                        return staticInvoker(invocationArguments);
                    }
                    else
                    {
                        var instanceInvoker = InstanceFactory.CreateInstanceMethodInvoker(resolverMetaData.Method);
                        var instance = InstanceFactory.CreateInstance(resolverMetaData.Method.DeclaringType);
                        return instanceInvoker(ref instance, invocationArguments);
                    }

                default:
                    throw new TargetException(
                        $"Unable to execute the target resolver {resolverMetaData.InternalName}. " +
                        $"Invalid or unsupported source location '{_resolverMetaData.DefinitionSource}'.");
            }
        }

        /// <summary>
        /// Gets the state of the model being represented on this controller action invocation.
        /// </summary>
        /// <value>The state of the model.</value>
        [GraphSkip]
        public InputModelStateDictionary ModelState { get; private set; }

        /// <summary>
        /// Gets the raw request for this request provided by the action invoker.
        /// </summary>
        /// <value>The field context.</value>
        [GraphSkip]
        public TRequest Request => _schemaItemContext.Request;

        /// <summary>
        /// Gets the resolved <see cref="ClaimsPrincipal"/> that was passed recieved on the request.
        /// </summary>
        /// <value>The user.</value>
        [GraphSkip]
        public ClaimsPrincipal User => _schemaItemContext.User;

        /// <summary>
        /// Gets the schema in scope for the currently executed operation.
        /// </summary>
        /// <value>The schema.</value>
        public ISchema Schema => _schemaItemContext.Schema;

        /// <summary>
        /// Gets the scoped <see cref="IServiceProvider"/> supplied to the original controller action that is being invoked.
        /// </summary>
        /// <value>The request services.</value>
        [GraphSkip]
        protected IServiceProvider RequestServices => _schemaItemContext.ServiceProvider;

        /// <summary>
        /// Gets the <see cref="HttpContext"/> for which this controller was invoked.
        /// </summary>
        /// <remarks>
        /// The is a convience property and may be <c>null</c> if this controller was not invoked from an ASP.NET pipeline.
        /// </remarks>
        /// <value>The HTTP context.</value>
        [GraphSkip]
        public HttpContext HttpContext { get; private set; }

        /// <summary>
        /// Gets the schema item context governing this controller's field resolution operation.
        /// </summary>
        /// <value>The context.</value>
        public SchemaItemResolutionContext<TRequest> Context => _schemaItemContext;
    }
}