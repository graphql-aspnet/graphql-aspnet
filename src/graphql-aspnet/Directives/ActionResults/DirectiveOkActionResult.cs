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
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Middleware.FieldExecution;

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

        /// <summary>
        /// Processes the provided resolution context against this action result instance to
        /// generate the expected response in accordance with this instance's rule set.
        /// </summary>
        /// <param name="context">The context being processed.</param>
        /// <returns>Task.</returns>
        public Task Complete(ResolutionContext context)
        {
            return Task.CompletedTask;
        }
    }
}