// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Directives
{
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A base class defining common requirements for any class wishing to be a graphql
    /// directive.
    /// </summary>
    public abstract partial class GraphDirective : GraphControllerBase<IGraphDirectiveRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDirective"/> class.
        /// </summary>
        protected GraphDirective()
        {
        }
    }
}