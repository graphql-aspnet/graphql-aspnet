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
    /// <para>Spec: https://graphql.github.io/graphql-spec/June2018/#sec--deprecated .</para>
    /// </summary>
    [GraphType(Constants.ReservedNames.DEPRECATED_DIRECTIVE)]
    public sealed class DeprecatedDirective : GraphDirective
    {
        /// <summary>
        /// Executes the directive returning back the provided argument (from the user's query)
        /// to determine if execution should continue in the location this directive was found.
        /// </summary>
        /// <param name="reason">the reason for the deprecation, if any.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        [DirectiveLocations(DirectiveLocation.FIELD_DEFINITION | DirectiveLocation.ENUM_VALUE)]
        public IGraphActionResult Execute([FromGraphQL("reason")] string reason = "No longer supported")
        {
            var item = this.DirectiveTarget as ISchemaItem;
            if (item == null)
            {
                throw new GraphTypeDeclarationException(
                    $"Invalid schema item. The directive @{Constants.ReservedNames.DEPRECATED_ARGUMENT_NAME} must target " +
                    $"an object that implements {typeof(ISchemaItem).FriendlyName()}. (Current Target: {item?.GetType().FriendlyName()})");
            }

            if (reason == null)
            {
                throw new GraphTypeDeclarationException(
                    $"A non-null reason must be provided with @{Constants.ReservedNames.DEPRECATED_ARGUMENT_NAME}. (Target: {item.Route.Path})");
            }

            if (this.DirectiveTarget is IGraphField field)
            {
                field.IsDeprecated = true;
                field.DeprecationReason = reason;
            }
            else if (this.DirectiveTarget is IEnumValue enumValue)
            {
                enumValue.IsDeprecated = true;
                enumValue.DeprecationReason = reason;
            }

            return this.Ok();
        }
    }
}