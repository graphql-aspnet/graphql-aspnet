// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Resolvers
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Middleware.FieldExecution;

    /// <summary>
    /// A field resolver that will invoke a schema pipeline for whatever schema is beng processed
    /// resulting in the the configured <see cref="IGraphMethod"/> handling the request.
    /// </summary>
    public class GraphObjectMethodResolver : IGraphFieldResolver
    {
        private readonly IGraphMethod _graphMethod;
        private readonly MethodInfo _methodInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphObjectMethodResolver" /> class.
        /// </summary>
        /// <param name="graphMethod">The graph method.</param>
        public GraphObjectMethodResolver(IGraphMethod graphMethod)
        {
            _graphMethod = Validation.ThrowIfNullOrReturn(graphMethod, nameof(graphMethod));
            _methodInfo = _graphMethod.Method;
        }

        /// <summary>
        /// Processes the given <see cref="IGraphFieldRequest" /> against this instance
        /// performing the operation as defined by this entity and generating a response.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="cancelToken">The cancel token monitoring the execution of a graph request.</param>
        /// <returns>Task&lt;IGraphPipelineResponse&gt;.</returns>
        public virtual async Task Resolve(FieldResolutionContext context, CancellationToken cancelToken = default)
        {
            var sourceData = context.Arguments?.SourceData;
            if (sourceData == null)
            {
                context.Messages.Critical(
                    "No source data was provided to the field resolver " +
                    $"for '{_graphMethod.Route.Path}'. Unable to complete the request.",
                    Constants.ErrorCodes.INVALID_OBJECT,
                    context.Request.Origin);

                return;
            }

            var typeCheck = _methodInfo.ReflectedType ?? _methodInfo.DeclaringType;
            if (context.Arguments.SourceData.GetType() != typeCheck)
            {
                context.Messages.Critical(
                   "The source data provided to the field resolver " +
                   $"for '{_graphMethod.Route.Path}' could not be coerced into the expected source graph type. See exception for details.",
                   Constants.ErrorCodes.INVALID_OBJECT,
                   context.Request.Origin,
                   new GraphExecutionException(
                       $"The method '{_graphMethod.InternalFullName}' expected source data of type " +
                       $"'{_graphMethod.Parent.ObjectType.FriendlyName()}' but received '{sourceData.GetType().FriendlyName()}' " +
                       "which is not compatible."));

                return;
            }

            try
            {
                object data = null;

                var paramSet = context.Arguments.PrepareArguments(_graphMethod);
                var invoker = InstanceFactory.CreateInstanceMethodInvoker(_graphMethod.Method);
                var invokeReturn = invoker(context.Arguments.SourceData, paramSet);
                if (_graphMethod.IsAsyncField)
                {
                    if (invokeReturn is Task task)
                    {
                        await task.ConfigureAwait(false);
                        data = task.ResultOrDefault();
                    }
                }
                else
                {
                    data = invokeReturn;
                }

                context.Result = data;
            }
            catch (GraphExecutionException gee)
            {
                context.Messages.Critical(
                    gee.Message,
                    Constants.ErrorCodes.EXECUTION_ERROR,
                    context.Request.Origin);
            }
            catch (Exception ex)
            {
                context.Messages.Critical(
                    $"An unknown error occured atttempting to resolve the field '{_graphMethod.Route.Path}'. " +
                    $"See exception for details.",
                    Constants.ErrorCodes.UNHANDLED_EXCEPTION,
                    context.Request.Origin,
                    ex);
            }
        }

        /// <summary>
        /// Gets the concrete type this resolver attempts to create during its operation.
        /// </summary>
        /// <value>The type of the return.</value>
        public Type ObjectType => _graphMethod.ObjectType;
    }
}