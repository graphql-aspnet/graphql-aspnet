// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Resolvers
{
    using System;
    using System.Diagnostics;
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
    /// resulting in the configured <see cref="IGraphFieldResolverMetaData"/> handling the request.
    /// </summary>
    [DebuggerDisplay("Prop Resolver: {MetaData.InternalName}")]
    internal class ObjectMethodGraphFieldResolver : IGraphFieldResolver
    {
        private readonly MethodInfo _methodInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectMethodGraphFieldResolver" /> class.
        /// </summary>
        /// <param name="resolverMetadata">A resolver method that points to a .NET method.</param>
        public ObjectMethodGraphFieldResolver(IGraphFieldResolverMetaData resolverMetadata)
        {
            this.MetaData = Validation.ThrowIfNullOrReturn(resolverMetadata, nameof(resolverMetadata));
            _methodInfo = this.MetaData.Method;
        }

        /// <inheritdoc />
        public virtual async Task ResolveAsync(FieldResolutionContext context, CancellationToken cancelToken = default)
        {
            var sourceData = context.SourceData;
            if (sourceData == null)
            {
                context.Messages.Critical(
                    "No source data was provided to the field resolver " +
                    $"for '{context.Request.Field.ItemPath.Path}'. Unable to complete the request.",
                    Constants.ErrorCodes.INVALID_OBJECT,
                    context.Request.Origin);

                return;
            }

            var typeCheck = _methodInfo.ReflectedType ?? _methodInfo.DeclaringType;
            if (context.SourceData?.GetType() != typeCheck)
            {
                context.Messages.Critical(
                   "The source data provided to the field resolver " +
                   $"for '{context.Request.Field.ItemPath.Path}' could not be coerced into the expected source graph type. See exception for details.",
                   Constants.ErrorCodes.INVALID_OBJECT,
                   context.Request.Origin,
                   new GraphExecutionException(
                       $"The method '{this.MetaData.InternalName}' expected source data of type " +
                       $"'{this.MetaData.ParentObjectType.FriendlyName()}' but received '{sourceData.GetType().FriendlyName()}' " +
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
                        $"Unable to determine the isolation requirements for the resolver of field '{context.Request.InvocationContext.Field.ItemPath.Path}'");
                }

                var shouldIsolate = isolationManager.ShouldIsolate(context.Schema, context.Request.Field.FieldSource);
                if (shouldIsolate)
                {
                    await isolationManager.WaitAsync();
                    isolationObtained = true;
                }

                object data = null;

                var paramSet = context.ExecutionSuppliedArguments.PrepareArguments(this.MetaData);
                var invoker = InstanceFactory.CreateInstanceMethodInvoker(this.MetaData.Method);

                var invokableObject = context.SourceData as object;
                var invokeReturn = invoker(ref invokableObject, paramSet);
                if (this.MetaData.IsAsyncField)
                {
                    if (invokeReturn is Task task)
                    {
                        await task.ConfigureAwait(false);
                        data = task.ResultOfTypeOrNull(this.MetaData.ExpectedReturnType);
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
                    $"An unknown error occured atttempting to resolve the field '{context.Request.Field.ItemPath.Path}'. " +
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
        public IGraphFieldResolverMetaData MetaData { get; }
    }
}