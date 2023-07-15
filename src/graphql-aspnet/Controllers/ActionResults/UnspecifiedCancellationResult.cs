// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Controllers.ActionResults
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Controllers;

    /// <summary>
    /// A action result that generates a a canceled context without indicating any specific error.
    /// </summary>
    public class UnspecifiedCancellationResult : IGraphActionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnspecifiedCancellationResult" /> class.
        /// </summary>
        public UnspecifiedCancellationResult()
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