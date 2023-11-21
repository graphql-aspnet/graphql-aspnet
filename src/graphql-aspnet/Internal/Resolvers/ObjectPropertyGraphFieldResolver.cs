﻿// *************************************************************
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
    using System.Diagnostics;
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
    /// A resolver that extracts a property from an object and returns it as a field value.
    /// </summary>
    /// <seealso cref="IGraphFieldResolver" />
    [DebuggerDisplay("Prop Resolver: {_graphMethod.Name}")]
    internal class ObjectPropertyGraphFieldResolver : IGraphFieldResolver
    {
        private readonly IGraphFieldResolverMethod _graphMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPropertyGraphFieldResolver" /> class.
        /// </summary>
        /// <param name="propertyGetMethod">A resolver method that points to a .NET property getter.</param>
        public ObjectPropertyGraphFieldResolver(IGraphFieldResolverMethod propertyGetMethod)
        {
            _graphMethod = Validation.ThrowIfNullOrReturn(propertyGetMethod, nameof(propertyGetMethod));
        }

        /// <inheritdoc />
        public async Task ResolveAsync(FieldResolutionContext context, CancellationToken cancelToken = default)
        {
            var sourceData = context.Arguments.SourceData;
            if (sourceData == null)
            {
                context.Messages.Critical(
                    "No source data was provided to the field resolver " +
                    $"for '{_graphMethod.Name}'. Unable to complete the request.",
                    Constants.ErrorCodes.INVALID_OBJECT,
                    context.Request.Origin);

                return;
            }

            // valdidate the incoming source data to ensure its process-able by this property
            // resolver. If the data is being resolved through an interface or object reference
            // ensure the provided source data can be converted otherwise ensure the types match exactly.
            if (_graphMethod.Parent.ObjectType.IsInterface || _graphMethod.Parent.ObjectType.IsClass)
            {
                if (!Validation.IsCastable(sourceData.GetType(), _graphMethod.Parent.ObjectType))
                {
                    context.Messages.Critical(
                        "The source data provided to the field resolver " +
                        $"for '{_graphMethod.Route.Path}' could not be coerced into the expected source graph type. See exception for details.",
                        Constants.ErrorCodes.INVALID_OBJECT,
                        context.Request.Origin,
                        new GraphExecutionException(
                            $"The property '{_graphMethod.InternalFullName}' expected source data that implements the interface " +
                            $"'{_graphMethod.Parent.ObjectType.FriendlyName()}' but received '{sourceData.GetType().FriendlyName()}' which " +
                            "is not compatible."));

                    return;
                }
            }
            else if (sourceData.GetType() != _graphMethod.Parent.ObjectType)
            {
                context.Messages.Critical(
                    "The source data provided to the field resolver " +
                    $"for '{_graphMethod.Route.Path}' could not be coerced into the expected source graph type. See exception for details.",
                    Constants.ErrorCodes.INVALID_OBJECT,
                    context.Request.Origin,
                    new GraphExecutionException(
                        $"The property '{_graphMethod.InternalFullName}' expected source data of type " +
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

                var invoker = InstanceFactory.CreateInstanceMethodInvoker(_graphMethod.Method);
                var invokeReturn = invoker(ref sourceData, new object[0]);
                if (_graphMethod.IsAsyncField)
                {
                    if (invokeReturn is Task task)
                    {
                        await task.ConfigureAwait(false);
                        if (task.IsFaulted)
                            throw task.UnwrapException();

                        invokeReturn = task.ResultOfTypeOrNull(_graphMethod.ExpectedReturnType);
                    }
                    else
                    {
                        context.Messages.Critical(
                            "The source data provided to the field resolver " +
                            $"for '{_graphMethod.Route.Path}' could not be coerced into the expected source graph type. See exception for details.",
                            Constants.ErrorCodes.INVALID_OBJECT,
                            context.Request.Origin,
                            new GraphExecutionException(
                                $"The method '{_graphMethod.Route.Path}' is defined " +
                                $"as asyncronous but it did not return a {typeof(Task)}."));
                        invokeReturn = null;
                    }
                }

                context.Result = invokeReturn;
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
                    $"An unknown error occured atttempting to resolve the field '{_graphMethod.Route.Path}'. See exception for details.",
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