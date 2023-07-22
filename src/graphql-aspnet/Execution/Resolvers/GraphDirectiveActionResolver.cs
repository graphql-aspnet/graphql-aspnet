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
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Controllers.ActionResults;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A special resolver specifically for invoking controller actions
    /// on <see cref="GraphDirective"/>s. Provides extra options and meta-data used by
    /// directives to ensure they are processed correctly.
    /// </summary>
    internal class GraphDirectiveActionResolver : GraphControllerActionResolverBase, IGraphDirectiveResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDirectiveActionResolver" /> class.
        /// </summary>
        /// <param name="directiveMetadataItems">The directive metadata items, per location,
        /// that this resolver will use for executing the directive.</param>
        public GraphDirectiveActionResolver(IReadOnlyDictionary<DirectiveLocation, IGraphFieldResolverMetaData> directiveMetadataItems)
        {
            this.MetaData = Validation.ThrowIfNullOrReturn(directiveMetadataItems, nameof(directiveMetadataItems));
        }

        /// <inheritdoc />
        public async Task ResolveAsync(DirectiveResolutionContext context, CancellationToken cancelToken = default)
        {
            // if no action is found skip processing of this directive
            if (!this.MetaData.ContainsKey(context.Request.InvocationContext.Location))
                return;

            var action = this.MetaData[context.Request.InvocationContext.Location];

            IGraphActionResult result;
            try
            {
                // create a directive instance for this invocation
                var directive = context
                    .ServiceProvider?
                    .GetService(action.ParentObjectType) as GraphDirective;

                if (directive == null)
                {
                    // fallback: attempt to create the directive if it has no constructor parameters
                    try
                    {
                        directive = InstanceFactory.CreateInstance(action.ParentObjectType) as GraphDirective;
                    }
                    catch (InvalidOperationException)
                    {
                        // unable to create the instance with a parameterless constructor
                    }
                }

                if (directive == null)
                {
                    result = new RouteNotFoundGraphActionResult(
                        $"The directive '{action.InternalName}' " +
                        "was not found in the scoped service provider. Any directives that have constructor parameters " +
                        $"must also be registered to the service provider; Try using '{nameof(SchemaOptions.AddGraphType)}' " +
                        $"with the type of your directive at startup.");
                }
                else
                {
                    // invoke the right action method and set a result.
                    var task = directive.InvokeActionAsync(action, context);

                    var returnedItem = await task.ConfigureAwait(false);
                    result = this.EnsureGraphActionResult(returnedItem);
                }
            }
            catch (Exception ex)
            {
                // :(
                result = new InternalServerErrorGraphActionResult("Operation failed.", ex);
            }

            // resolve the final graph action output using the provided field context
            // in what ever manner is appropriate for the result itself
            await result.CompleteAsync(context);
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<DirectiveLocation, IGraphFieldResolverMetaData> MetaData { get; }
    }
}