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
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// <para>A directive, applicable to custom SCALAR, that
    /// indicates a url pointing to the specification of the behavior for the target SCALAR. This
    /// url is shown in the type system as a <c>specifiedByUrl</c> field on
    /// the type definition.</para>
    /// <para>Spec: <see href="https://spec.graphql.org/October2021/#sec--specifiedBy" />.</para>
    /// </summary>
    [GraphType(Constants.ReservedNames.SPECIFIED_BY_DIRECTIVE)]
    [Description("A directive that points to additional details about the target SCALAR.")]
    public sealed class SpecifiedByDirective : GraphDirective
    {
        /// <summary>
        /// Executes the directive on the target document part.
        /// </summary>
        /// <param name="url">The URL pointing to the specification for the custom scalar.</param>
        /// <returns>IGraphActionResult.</returns>
        [DirectiveLocations(DirectiveLocation.SCALAR)]
        public IGraphActionResult Execute(
            [FromGraphQL("url", TypeExpression = "Type!")]
            [Description("A url pointing to the documentation about the target scalar.")]
            string url)
        {
            var scalarItem = this.DirectiveTarget as IScalarGraphType;

            if (scalarItem == null)
            {
                string currentTarget;

                if (this.DirectiveTarget is IGraphType gt)
                    currentTarget = $"{gt.Name} | Type: {gt.Kind}";
                else if (this.DirectiveTarget is ISchemaItem sitem)
                    currentTarget = $"{sitem.Name} | Class: {sitem.GetType().FriendlyName()}";
                else if (this.DirectiveTarget != null)
                    currentTarget = $"Class: {this.DirectiveTarget?.GetType().FriendlyName()}";
                else
                    currentTarget = "-unknown-";

                throw new GraphTypeDeclarationException(
                    $"Invalid schema item. The directive @{Constants.ReservedNames.SPECIFIED_BY_DIRECTIVE} must target " +
                    $"a {TypeKind.SCALAR}. (Current Target: {currentTarget})");
            }

            url = url?.Trim();
            if (url == null)
            {
                throw new GraphTypeDeclarationException(
                    $"A non-null url must be provided with @{Constants.ReservedNames.SPECIFIED_BY_DIRECTIVE}. (Target: {scalarItem.ItemPath.Path})");
            }

            scalarItem.SpecifiedByUrl = url;

            return this.Ok();
        }
    }
}