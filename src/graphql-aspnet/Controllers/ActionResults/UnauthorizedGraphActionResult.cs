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
    using GraphQL.AspNet.Middleware.FieldExecution;

    /// <summary>
    /// A result indicating the user executing hte query was not authorized to do so.
    /// </summary>
    /// <seealso cref="IGraphActionResult" />
    public class UnauthorizedGraphActionResult : IGraphActionResult
    {
        private readonly string _errorCode;
        private readonly string _errorMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthorizedGraphActionResult" /> class.
        /// </summary>
        /// <param name="errorMessage">The error message to apply.</param>
        /// <param name="errorCode">The error code to assign to the message in the response.</param>
        public UnauthorizedGraphActionResult(string errorMessage = "", string errorCode = Constants.ErrorCodes.ACCESS_DENIED)
        {
            _errorMessage = errorMessage ?? "Unauthorized Access";
            _errorCode = errorCode ?? Constants.ErrorCodes.ACCESS_DENIED;
        }

        /// <summary>
        /// Processes the provided resolution context against this action result instance to
        /// generate the expected response in accordance with this instance's rule set.
        /// </summary>
        /// <param name="context">The context being processed.</param>
        /// <returns>Task.</returns>
        public Task Complete(ResolutionContext context)
        {
            context.Messages.Critical(
                   _errorMessage,
                   _errorCode,
                   context.Request.Origin);

            context.Cancel();
            return Task.CompletedTask;
        }
    }
}