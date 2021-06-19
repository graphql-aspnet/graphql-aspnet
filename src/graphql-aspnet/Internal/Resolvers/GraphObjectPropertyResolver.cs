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
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Middleware.FieldExecution;

    /// <summary>
    /// A resolver that extracts a property from an object and returns it as a field value.
    /// </summary>
    /// <seealso cref="IGraphFieldResolver" />
    public class GraphObjectPropertyResolver : IGraphFieldResolver
    {
        private readonly IGraphMethod _graphMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphObjectPropertyResolver" /> class.
        /// </summary>
        /// <param name="propertyGetMethod">The property method.</param>
        public GraphObjectPropertyResolver(IGraphMethod propertyGetMethod)
        {
            _graphMethod = Validation.ThrowIfNullOrReturn(propertyGetMethod, nameof(propertyGetMethod));
        }

        /// <summary>
        /// Processes the given <see cref="IGraphFieldRequest" /> against this instance
        /// performing the operation as defined by this entity and generating a response.
        /// </summary>
        /// <param name="context">The field context containing the necessary data to resolve
        /// the field and produce a reslt.</param>
        /// <param name="cancelToken">The cancel token monitoring the execution of a graph request.</param>
        /// <returns>Task&lt;IGraphPipelineResponse&gt;.</returns>
        public async Task Resolve(FieldResolutionContext context, CancellationToken cancelToken = default)
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
            // ensure the provided
            // source data can be converted otherwise ensure the types match exactly.
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

            try
            {
                var invoker = InstanceFactory.CreateInstanceMethodInvoker(_graphMethod.Method);
                var invokeReturn = invoker(sourceData, new object[0]);
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
            catch (Exception ex)
            {
                context.Messages.Critical(
                    $"An unknown error occured atttempting to resolve the field '{_graphMethod.Route.Path}'. See exception for details.",
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