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
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Controllers.ActionResults;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Middleware.FieldExecution;

    /// <summary>
    /// A special resolver specifically for actions on <see cref="GraphController"/>s. Provides extra
    /// fields, options and meta-data used by controllers to field a request through user code.
    /// </summary>
    public class GraphControllerActionResolver : BaseInvocableActionResolver, IGraphFieldResolver
    {
        private readonly IGraphMethod _actionMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphControllerActionResolver"/> class.
        /// </summary>
        /// <param name="actionMethod">The action method that this resolver will invoke.</param>
        public GraphControllerActionResolver(IGraphMethod actionMethod)
        {
            _actionMethod = Validation.ThrowIfNullOrReturn(actionMethod, nameof(actionMethod));
        }

        /// <summary>
        /// Processes the given <see cref="IGraphFieldRequest" /> against this instance
        /// performing the operation as defined by this entity and generating a response.
        /// </summary>
        /// <param name="context">The field context containing the necessary data to resolve
        /// the field and produce a reslt.</param>
        /// <param name="cancelToken">The cancel token monitoring the execution of a graph request.</param>
        /// <returns>Task&lt;IGraphPipelineResponse&gt;.</returns>
        [DebuggerStepThrough]
        public async Task Resolve(FieldResolutionContext context, CancellationToken cancelToken = default)
        {
            IGraphActionResult result;
            try
            {
                // create a scoped controller instance for this invocation
                var controller = context
                    .ServiceProvider?
                    .GetService(_actionMethod.Parent.ObjectType) as GraphController;

                if (controller == null)
                {
                    result = new RouteNotFoundGraphActionResult(
                        $"The controller assigned to process the field '{context.Request.InvocationContext.Field.Route.Path}' " +
                        "was not found.");
                }
                else
                {
                    // invoke the right action method and set a result.
                    var task = controller.InvokeActionAsync(_actionMethod, context);
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
            await result.Complete(context).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the concrete type this resolver attempts to create as a during its invocation.
        /// </summary>
        /// <value>The type of the return.</value>
        public Type ObjectType => _actionMethod.ObjectType;
    }
}