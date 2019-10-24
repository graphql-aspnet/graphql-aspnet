// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Controllers
{
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A base class to provide common functionality for register mutation functions in a graph query.
    /// </summary>
    public abstract partial class GraphController : GraphControllerBase<IGraphFieldRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphController"/> class.
        /// </summary>
        protected GraphController()
            : base()
        {
        }
    }
}