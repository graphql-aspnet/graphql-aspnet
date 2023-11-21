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
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A field resolver that will invoke a schema pipeline for whatever schema is beng processed
    /// resulting in the configured <see cref="IGraphFieldResolverMethod"/> handling the request.
    /// </summary>
    internal class ObjectMethodGraphFieldResolver : IGraphFieldResolver
    {
        private readonly IGraphFieldResolverMethod _graphMethod;
        private readonly MethodInfo _methodInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectMethodGraphFieldResolver" /> class.
        /// </summary>
        /// <param name="graphMethod">A resolver method that points to a .NET method.</param>
        public ObjectMethodGraphFieldResolver(IGraphFieldResolverMethod graphMethod)
        {
            _graphMethod = Validation.ThrowIfNullOrReturn(graphMethod, nameof(graphMethod));
            _methodInfo = _graphMethod.Method;
        }

        /// <inheritdoc />
        public virtual async Task ResolveAsync(FieldResolutionContext context, CancellationToken cancelToken = default)
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

            var isolationObtained = false;
            IGraphQLFieldResolverIsolationManager isolationManager = null;

            try
            {
                isolationManager = context
                    .ServiceProvider?
                    .GetService<IGraphQLFieldResolverIsolationManager>();

                if (isolationManager == null)
                {
                    throw new GraphExecutionException(
                        $"No {nameof(IGraphQLFieldResolverIsolationManager)} was configured for the request. " +
                        $"Unable to determine the isolation requirements for the resolver of field '{context.Request.InvocationContext.Field.Route.Path}'");
                }

                var shouldIsolate = isolationManager.ShouldIsolate(context.Schema, context.Request.Field.FieldSource);
                if (shouldIsolate)
                {
                    await isolationManager.WaitAsync();
                    isolationObtained = true;
                }

                object data = null;

                var paramSet = context.Arguments.PrepareArguments(_graphMethod);
                var invoker = InstanceFactory.CreateInstanceMethodInvoker(_graphMethod.Method);

                var invokableObject = context.Arguments.SourceData as object;
                var invokeReturn = invoker(ref invokableObject, paramSet);
                if (_graphMethod.IsAsyncField)
                {
                    if (invokeReturn is Task task)
                    {
                        await task.ConfigureAwait(false);
                        data = task.ResultOfTypeOrNull(_graphMethod.ExpectedReturnType);
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
                    "See exception for details.",
                    Constants.ErrorCodes.UNHANDLED_EXCEPTION,
                    context.Request.Origin,
                    ex);
            }
            finally
            {
                if (isolationObtained)
                    isolationManager.Release();
            }
        }

        /// <inheritdoc />
        public Type ObjectType => _graphMethod.ObjectType;
    }
}