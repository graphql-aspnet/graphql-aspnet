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
    using System.Diagnostics;
    using GraphQL.AspNet.Controllers.ActionResults;
    using GraphQL.AspNet.Interfaces.Controllers;

    /// <summary>
    /// A base class defining common functionality for resolvers that target action
    /// methods on a controller.
    /// </summary>
    internal abstract class GraphControllerActionResolverBase
    {
        /// <summary>
        /// Ensures the supplied result object is a <see cref="IGraphActionResult"/> or boxes
        /// the value into a valid result.
        /// </summary>
        /// <param name="result">The controller action result to inspect.</param>
        /// <returns>IGraphActionResult.</returns>
        [DebuggerStepThrough]
        protected virtual IGraphActionResult EnsureGraphActionResult(object result)
        {
            if (result is IGraphActionResult actionResult)
                return actionResult;

            return new OperationCompleteGraphActionResult(result);
        }
    }
}