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
    using System.Diagnostics;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Controllers;

    /// <summary>
    /// A result indicating an ok return status and an object to be rendered to the graph.
    /// </summary>
    [DebuggerDisplay("Has Object: {_result?.GetType().FriendlyName()}")]
    public class ObjectReturnedGraphActionResult : IGraphActionResult
    {
        private readonly object _result;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectReturnedGraphActionResult"/> class.
        /// </summary>
        /// <param name="objectToReturn">The object to return.</param>
        public ObjectReturnedGraphActionResult(object objectToReturn)
        {
            _result = objectToReturn;
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        public virtual Task Complete(BaseResolutionContext context)
        {
            if (context is FieldResolutionContext frc)
                frc.Result = _result;

            return Task.CompletedTask;
        }
    }
}