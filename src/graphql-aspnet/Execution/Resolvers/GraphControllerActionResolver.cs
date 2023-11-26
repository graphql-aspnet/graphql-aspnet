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
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Controllers.ActionResults;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A special resolver specifically for actions on a <see cref="GraphController"/>. Provides extra
    /// properties, options and meta-data used by controllers to resolve a field through user code.
    /// </summary>
    internal class GraphControllerActionResolver : GraphControllerActionResolverBase, IGraphFieldResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphControllerActionResolver"/> class.
        /// </summary>
        /// <param name="actionResolverMetadata">The metadata describing an action
        /// method that this resolver will invoke.</param>
        public GraphControllerActionResolver(IGraphFieldResolverMetaData actionResolverMetadata)
        {
            this.MetaData = Validation.ThrowIfNullOrReturn(actionResolverMetadata, nameof(actionResolverMetadata));
        }

        /// <inheritdoc />
        public async Task ResolveAsync(FieldResolutionContext context, CancellationToken cancelToken = default)
        {
            IGraphActionResult result;
            var isolationObtained = false;
            IGraphQLFieldResolverIsolationManager isolationManager = null;

            try
            {
                // create a scoped controller instance for this invocation
                var controller = context
                    .ServiceProvider?
                    .GetService(this.MetaData.ParentObjectType) as GraphController;

                isolationManager = context
                    .ServiceProvider?
                    .GetService<IGraphQLFieldResolverIsolationManager>();

                if (controller == null)
                {
                    result = new PathNotFoundGraphActionResult(
                        $"The controller assigned to process the field '{context.Request.InvocationContext.Field.ItemPath.Path}' " +
                        "was not found.");
                }
                else if (isolationManager == null)
                {
                    throw new GraphExecutionException(
                        $"No {nameof(IGraphQLFieldResolverIsolationManager)} was configured for the request. " +
                        $"Unable to determine the isolation requirements for the resolver of field '{context.Request.InvocationContext.Field.ItemPath.Path}'");
                }
                else
                {
                    var shouldIsolate = isolationManager.ShouldIsolate(context.Schema, context.Request.Field.FieldSource);
                    if (shouldIsolate)
                    {
                        await isolationManager.WaitAsync();
                        isolationObtained = true;
                    }

                    // invoke the right action method and set a result.
                    var task = controller.InvokeActionAsync(this.MetaData, context);
                    var returnedItem = await task.ConfigureAwait(false);
                    result = this.EnsureGraphActionResult(returnedItem);
                }
            }
            catch (Exception ex)
            {
                // :(
                result = new InternalServerErrorGraphActionResult("Operation failed.", ex);
            }
            finally
            {
                if (isolationObtained)
                    isolationManager.Release();
            }

            // resolve the final graph action output using the provided field context
            // in what ever manner is appropriate for the result itself
            await result.CompleteAsync(context).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public IGraphFieldResolverMetaData MetaData { get; }
    }
}