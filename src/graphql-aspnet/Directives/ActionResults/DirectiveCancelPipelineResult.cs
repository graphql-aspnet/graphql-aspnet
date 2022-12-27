// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Directives.ActionResults
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Controllers;

    /// <summary>
    /// A directive action result that generates a response indicating the in-progress pipeline
    /// should be abandoned.
    /// </summary>
    public class DirectiveCancelPipelineResult : IGraphActionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveCancelPipelineResult" /> class.
        /// </summary>
        public DirectiveCancelPipelineResult()
        {
        }

        /// <inheritdoc />
        public Task CompleteAsync(SchemaItemResolutionContext context)
        {
            context.Cancel();
            return Task.CompletedTask;
        }
    }
}