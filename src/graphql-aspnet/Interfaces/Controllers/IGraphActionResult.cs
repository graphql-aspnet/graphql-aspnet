// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Controllers
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Middleware.FieldExecution;

    /// <summary>
    /// An action result rendered by a graph controller method that can be resolved against a <see cref="IGraphFieldRequest"/>
    /// to automatically to produce an output for a graph field.
    /// </summary>
    public interface IGraphActionResult
    {
        /// <summary>
        /// Processes the provided resolution context against this action result instance to
        /// generate the expected response in accordance with this instance's rule set.
        /// </summary>
        /// <param name="context">The context being processed.</param>
        /// <returns>Task.</returns>
        Task Complete(ResolutionContext context);
    }
}