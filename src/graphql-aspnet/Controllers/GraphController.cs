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
    /// A base class from which all graph controllers must inherit.
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