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
    using System.Diagnostics;
    using GraphQL.AspNet.Controllers.ActionResults;
    using GraphQL.AspNet.Interfaces.Controllers;

    /// <summary>
    /// A common base class defining common operations for the various action invokers that handle pipelne requests.
    /// </summary>
    public abstract class BaseInvocableActionResolver
    {
        /// <summary>
        /// Ensures the supplied result is a <see cref="IGraphActionResult"/> or boxes
        /// the value into a valid result.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>IGraphActionResult.</returns>
        [DebuggerStepThrough]
        protected virtual IGraphActionResult EnsureGraphActionResult(object result)
        {
            if (result is IGraphActionResult actionResult)
                return actionResult;

            return new ObjectReturnedGraphActionResult(result);
        }
    }
}