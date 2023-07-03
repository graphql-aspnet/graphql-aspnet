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

    /// <summary>
    /// A field resolver that will invoke a schema pipeline for whatever schema is beng processed
    /// resulting in the configured <see cref="IGraphFieldResolverMetaData"/> handling the request.
    /// </summary>
    internal class ObjectMethodGraphFieldResolver : IGraphFieldResolver
    {
        private readonly IGraphFieldResolverMetaData _resolverMetadata;
        private readonly MethodInfo _methodInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectMethodGraphFieldResolver" /> class.
        /// </summary>
        /// <param name="resolverMetadata">A resolver method that points to a .NET method.</param>
        public ObjectMethodGraphFieldResolver(IGraphFieldResolverMetaData resolverMetadata)
        {
            _resolverMetadata = Validation.ThrowIfNullOrReturn(resolverMetadata, nameof(resolverMetadata));
            _methodInfo = _resolverMetadata.Method;
        }

        /// <inheritdoc />
        public virtual async Task ResolveAsync(FieldResolutionContext context, CancellationToken cancelToken = default)
        {
            var sourceData = context.Arguments?.SourceData;
            if (sourceData == null)
            {
                context.Messages.Critical(
                    "No source data was provided to the field resolver " +
                    $"for '{context.Request.Field.Route.Path}'. Unable to complete the request.",
                    Constants.ErrorCodes.INVALID_OBJECT,
                    context.Request.Origin);

                return;
            }

            var typeCheck = _methodInfo.ReflectedType ?? _methodInfo.DeclaringType;
            if (context.Arguments.SourceData.GetType() != typeCheck)
            {
                context.Messages.Critical(
                   "The source data provided to the field resolver " +
                   $"for '{context.Request.Field.Route.Path}' could not be coerced into the expected source graph type. See exception for details.",
                   Constants.ErrorCodes.INVALID_OBJECT,
                   context.Request.Origin,
                   new GraphExecutionException(
                       $"The method '{_resolverMetadata.InternalFullName}' expected source data of type " +
                       $"'{_resolverMetadata.ParentObjectType.FriendlyName()}' but received '{sourceData.GetType().FriendlyName()}' " +
                       "which is not compatible."));

                return;
            }

            try
            {
                object data = null;

                var paramSet = context.Arguments.PrepareArguments(_resolverMetadata);
                var invoker = InstanceFactory.CreateInstanceMethodInvoker(_resolverMetadata.Method);

                var invokableObject = context.Arguments.SourceData as object;
                var invokeReturn = invoker(ref invokableObject, paramSet);
                if (_resolverMetadata.IsAsyncField)
                {
                    if (invokeReturn is Task task)
                    {
                        await task.ConfigureAwait(false);
                        data = task.ResultOfTypeOrNull(_resolverMetadata.ExpectedReturnType);
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
                    $"An unknown error occured atttempting to resolve the field '{context.Request.Field.Route.Path}'. " +
                    "See exception for details.",
                    Constants.ErrorCodes.UNHANDLED_EXCEPTION,
                    context.Request.Origin,
                    ex);
            }
        }

        /// <inheritdoc />
        public Type ObjectType => _resolverMetadata.ObjectType;
    }
}