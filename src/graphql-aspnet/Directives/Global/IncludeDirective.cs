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
    using System.ComponentModel;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// <para>A directive, applicable to a field, that defines additional logic to determine if
    /// the field should be included or not.</para>
    /// <para>Spec: <see href="https://graphql.github.io/graphql-spec/October2021/#sec--include" /> .</para>
    /// </summary>
    [GraphType(Constants.ReservedNames.INCLUDE_DIRECTIVE)]
    [Description("A directive which potentially includes a field or fragment in the query results.")]
    public sealed class IncludeDirective : GraphDirective
    {
        /// <summary>
        /// Executes the directive on the target document part.
        /// </summary>
        /// <param name="ifArgument">If set to <c>true</c> the document part this
        /// directive is attached to WILL be included in the results set.</param>
        /// <returns>IGraphActionResult.</returns>
        [DirectiveLocations(DirectiveLocation.FIELD | DirectiveLocation.FRAGMENT_SPREAD | DirectiveLocation.INLINE_FRAGMENT)]
        public IGraphActionResult Execute(
            [FromGraphQL("if")]
            [Description("When true, the field or fragment is included in the query results.")]
            bool ifArgument)
        {
            if (this.DirectiveTarget is IIncludeableDocumentPart rdp)
                rdp.IsIncluded = rdp.IsIncluded && ifArgument;

            return this.Ok();
        }
    }
}