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
    using GraphQL.AspNet.Controllers.ActionResults;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Middleware.FieldExecution;

    /// <summary>
    /// The default resolver for processing directive requests.
    /// </summary>
    public class GraphDirectiveActionResolver : BaseInvocableActionResolver, IGraphDirectiveResolver
    {
        private readonly IGraphDirectiveTemplate _directiveTemplate;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDirectiveActionResolver"/> class.
        /// </summary>
        /// <param name="directiveTemplate">The directive template from which this resolver will
        /// query for lifecycle methods.</param>
        public GraphDirectiveActionResolver(IGraphDirectiveTemplate directiveTemplate)
        {
            _directiveTemplate = Validation.ThrowIfNullOrReturn(directiveTemplate, nameof(directiveTemplate));
        }

        /// <summary>
        /// Processes the given <see cref="IGraphDirectiveRequest" /> against this instance
        /// performing the operation as defined by this entity and generating a response.
        /// </summary>
        /// <param name="context">The  context containing the necessary data to resolve
        /// the directive and produce a result.</param>
        /// <param name="cancelToken">The cancel token monitoring the execution of a graph request.</param>
        /// <returns>Task&lt;IGraphPipelineResponse&gt;.</returns>
        public async Task Resolve(DirectiveResolutionContext context, CancellationToken cancelToken = default)
        {
            var action = _directiveTemplate.FindMethod(context.Request.LifeCycle);

            // if no action is found skip processing of this directive
            if (action == null)
                return;

            IGraphActionResult result;
            try
            {
                // create a scoped controller instance for this invocation
                var directive = context
                    .ServiceProvider?
                    .GetService(_directiveTemplate.ObjectType) as GraphDirective;

                if (directive == null)
                {
                    result = new RouteNotFoundGraphActionResult(
                        $"The directive '{_directiveTemplate.InternalFullName}' " +
                        $"was not found in the scoped service provider.");
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
            await result.Complete(context);
        }
    }
}