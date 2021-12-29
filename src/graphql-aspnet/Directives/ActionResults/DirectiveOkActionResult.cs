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
    /// An action reslt indicating a successful completion of a directive with no returned value.
    /// </summary>
    public class DirectiveOkActionResult : IGraphActionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveOkActionResult" /> class.
        /// </summary>
        public DirectiveOkActionResult()
        {
        }

        /// <inheritdoc />
        public Task Complete(ResolutionContext context)
        {
            return Task.CompletedTask;
        }
    }
}