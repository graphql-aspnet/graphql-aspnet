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
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// <para>A directive, applicable to a field definition or enum value, that
    /// indicates its usage has been deprecated and will be removed in the future.</para>
    /// <para>Spec: https://graphql.github.io/graphql-spec/October2021/#sec--deprecated .</para>
    /// </summary>
    [GraphType(Constants.ReservedNames.DEPRECATED_DIRECTIVE)]
    public sealed class DeprecatedDirective : GraphDirective
    {
        /// <summary>
        /// Executes the directive and marking the target as being deprecated with the given reason.
        /// </summary>
        /// <param name="reason">the reason for the deprecation, if any.</param>
        /// <returns>IGraphActionResult.</returns>
        [DirectiveLocations(DirectiveLocation.FIELD_DEFINITION | DirectiveLocation.ENUM_VALUE)]
        public IGraphActionResult Execute([FromGraphQL("reason")] string reason = "No longer supported")
        {
            reason = reason?.Trim();
            var item = this.DirectiveTarget as ISchemaItem;
            if (item == null || (!(item is IGraphField) && !(item is IEnumValue)))
            {
                throw new GraphTypeDeclarationException(
                    $"Invalid schema item. The directive @{Constants.ReservedNames.DEPRECATED_DIRECTIVE} must target " +
                    $"an object that implements {typeof(IGraphField).FriendlyName()} or {typeof(IEnumValue).FriendlyName()}. (Current Target: {this.DirectiveTarget?.GetType().FriendlyName()})");
            }

            if (reason == null)
            {
                throw new GraphTypeDeclarationException(
                    $"A non-null reason must be provided with @{Constants.ReservedNames.DEPRECATED_DIRECTIVE}. (Target: {item.Route.Path})");
            }

            if (this.DirectiveTarget is IDeprecatable deprecatable)
            {
                deprecatable.IsDeprecated = true;
                deprecatable.DeprecationReason = reason;
            }

            return this.Ok();
        }
    }
}