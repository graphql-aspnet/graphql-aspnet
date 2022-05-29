﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Directives.Global
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// <para>A directive, applicable to a field, that defines additional logic to determine if
    /// the field should be included or not.</para>
    /// <para>Spec: https://graphql.github.io/graphql-spec/June2018/#sec--include .</para>
    /// </summary>
    [GraphType(Constants.ReservedNames.INCLUDE_DIRECTIVE)]
    [DirectiveInvocation(DirectiveInvocationPhase.BeforeFieldResolution)]
    public sealed class IncludeDirective : GraphDirective
    {
        /// <summary>
        /// Executes the directive returning back the provided argument (from the user's query)
        /// to determine if execution should continue in the location this directive was found.
        /// </summary>
        /// <param name="ifArgument">if set to <c>true</c> processing of the request, on this branch, will be allowed to continue.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        [DirectiveLocations(DirectiveLocation.FIELD | DirectiveLocation.FRAGMENT_SPREAD | DirectiveLocation.INLINE_FRAGMENT)]
        public IGraphActionResult Execute([FromGraphQL("if")] bool ifArgument)
        {
            return ifArgument ? this.Ok() : this.Cancel();
        }
    }
}