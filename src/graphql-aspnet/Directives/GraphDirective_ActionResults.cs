﻿// *************************************************************
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
    using GraphQL.AspNet.Directives.ActionResults;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A base class defining common requirements for any class wishing to be a directive
    /// that can be assigned at one of the <see cref="DirectiveLocation"/> at runtime.
    /// </summary>
    public partial class GraphDirective : GraphControllerBase<IGraphDirectiveRequest>
    {
        /// <summary>
        /// Returns an action result indicating the directive completed its intended operation correctly.
        /// </summary>
        /// <returns>IGraphActionResult&lt;TResult&gt;.</returns>
        protected virtual IGraphActionResult Ok()
        {
            return new DirectiveOkActionResult();
        }

        /// <summary>
        /// Returns an action result indicating the directive did NOT complete correctly. If this
        /// directive targets the type system, the schema being built will fail to build and the server
        /// will fail to start. If this is an execution directive, the query will be abandoned.
        /// </summary>
        /// <returns>IGraphActionResult.</returns>
        protected virtual IGraphActionResult Cancel()
        {
            return new DirectiveCancelPipelineResult();
        }
    }
}