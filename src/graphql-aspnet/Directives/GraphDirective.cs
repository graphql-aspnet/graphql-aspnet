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
    /// executable directive.
    /// </summary>
    public abstract partial class GraphDirective : GraphControllerBase<IGraphDirectiveRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDirective"/> class.
        /// </summary>
        protected GraphDirective()
        {
        }

        // Sample Method declarations that will be recognized by the type system
        /*
        public IGraphActionResult BeforeFieldResolution(string someArgument)
        public IGraphActionResult AfterFieldResolution(string someArgument);
        public void AlterSchemaItem(ISchemaItem item);
        */
    }
}