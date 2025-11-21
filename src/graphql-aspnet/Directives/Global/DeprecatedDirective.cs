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
    /// <para>A directive, applicable to a field definition or enum value, that
    /// indicates its usage has been deprecated and will be removed in the future.</para>
    /// <para>Spec: <see href="https://graphql.github.io/graphql-spec/October2021/#sec--deprecated"/>  .</para>
    /// </summary>
    [GraphType(Constants.ReservedNames.DEPRECATED_DIRECTIVE)]
    [Description("A directive that indicates the schema item should not be used and may be removed in the future.")]
    public sealed class DeprecatedDirective : GraphDirective
    {
        /// <summary>
        /// Executes the directive on the target document part.
        /// </summary>
        /// <param name="reason">An optional reason for the deprecation.</param>
        /// <returns>IGraphActionResult.</returns>
        [DirectiveLocations(DirectiveLocation.FIELD_DEFINITION | DirectiveLocation.ENUM_VALUE | DirectiveLocation.INPUT_FIELD_DEFINITION | DirectiveLocation.ARGUMENT_DEFINITION)]
        public IGraphActionResult Execute(
            [FromGraphQL("reason", TypeExpression = "Type!")]
            [Description("An optional human-friendly reason explaining why the schema item is being deprecated.")]
            string reason = "No longer supported")
        {
            reason = reason?.Trim();
            var item = this.DirectiveTarget as ISchemaItem;

            if (item is null || (item is not IGraphField && item is not IEnumValue && item is not IInputGraphField && item is not IGraphArgument))
            {
                throw new GraphTypeDeclarationException(
                    $"Invalid schema item. The @{Constants.ReservedNames.DEPRECATED_DIRECTIVE} directive  must target " +
                    $"an object that implements {typeof(IGraphField).FriendlyName()}, {typeof(IEnumValue).FriendlyName()}, {typeof(IInputGraphField).FriendlyName()} or {typeof(IGraphArgument).FriendlyName()}. (Current Target: {this.DirectiveTarget?.GetType().FriendlyName()})");
            }

            if (reason is null)
            {
                throw new GraphTypeDeclarationException(
                    $"Invalid schema item. Use of the @{Constants.ReservedNames.DEPRECATED_DIRECTIVE} directive requires a non-null reason.");
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