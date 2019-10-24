// *************************************************************
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

    /// <summary>
    /// <para>A directive, applicable to a field, that defines additional logic to determine if
    /// the field inclusion should be skipped or not.</para>
    /// <para>Spec: https://graphql.github.io/graphql-spec/June2018/#sec--skip .</para>
    /// </summary>
    [GraphType(Constants.ReservedNames.SKIP_DIRECTIVE)]
    [DirectiveLocations(ExecutableDirectiveLocation.AllFieldSelections)]
    public sealed class SkipDirective : GraphDirective
    {
        /// <summary>
        /// Executes the directive returning back the provided argument (from the user's query)
        /// to determine if execution should continue in the location this directive was found.
        /// </summary>
        /// <param name="ifArgument">if set to <c>true</c> processing of the request, on this branch, will stop.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        public IGraphActionResult BeforeFieldResolution([FromGraphQL("if")] bool ifArgument)
        {
            return ifArgument ? this.Cancel() : this.Ok();
        }
    }
}