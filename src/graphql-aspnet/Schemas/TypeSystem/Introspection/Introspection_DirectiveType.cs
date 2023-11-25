// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem.Introspection
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Introspection.Model;

    /// <summary>
    /// Represents the graph type called "__Directive".
    /// </summary>
    [DebuggerDisplay("INTROSPECTION TYPE __Directive")]
    internal class Introspection_DirectiveType : BaseIntrospectionObjectType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Introspection_DirectiveType"/> class.
        /// </summary>
        public Introspection_DirectiveType()
            : base(Constants.ReservedNames.DIRECTIVE_TYPE, nameof(Introspection_DirectiveType))
        {
            // "__Directive" type definition
            // https://graphql.github.io/graphql-spec/October2021/#sec-Introspection
            // -------------------------------------------------------------------------
            this.AddField<IntrospectedDirective, string>(
                "name",
                $"{this.InternalName}.{nameof(Directive.Name)}",
                new GraphTypeExpression(
                    Constants.ScalarNames.STRING,
                    GraphTypeExpression.RequiredSingleItem),
                new IntrospectedItemPath(ItemPathRoots.Types, this.Name, "name"),
                (directive) => directive.Name.AsCompletedTask(),
                "The case-sensitive name of this directive as it should appear in a query.");

            this.AddField<IntrospectedDirective, string>(
                "description",
                $"{this.InternalName}.{nameof(Directive.Description)}",
                new GraphTypeExpression(Constants.ScalarNames.STRING),
                new IntrospectedItemPath(ItemPathRoots.Types, this.Name, "description"),
                (directive) => directive.Description.AsCompletedTask(),
                "A human-friendly description of the directive and how it functions.");

            this.AddField<IntrospectedDirective, IReadOnlyList<DirectiveLocation>>(
                "locations",
                $"{this.InternalName}.{nameof(Directive.Locations)}",
                new GraphTypeExpression(
                    Constants.ReservedNames.DIRECTIVE_LOCATION_ENUM,
                    GraphTypeExpression.RequiredListRequiredItem),
                new IntrospectedItemPath(ItemPathRoots.Types, this.Name, "locations"),
                (directive) => directive.Locations.AsCompletedTask(),
                "A collection of locations where this directive can be used.");

            this.AddField<IntrospectedDirective, IReadOnlyList<IntrospectedInputValueType>>(
                "args",
                $"{this.InternalName}.{nameof(Directive.Arguments)}",
                new GraphTypeExpression(
                    Constants.ReservedNames.INPUT_VALUE_TYPE,
                    GraphTypeExpression.RequiredListRequiredItem),
                new IntrospectedItemPath(ItemPathRoots.Types, this.Name, "args"),
                (directive) => directive.Arguments.AsCompletedTask(),
                "A collection of input values provided to the directive in order to properly invoke it.");

            this.AddField<IntrospectedDirective, bool>(
                "isRepeatable",
                $"{this.InternalName}.{nameof(Directive.IsRepeatable)}",
                new GraphTypeExpression(
                    Constants.ScalarNames.BOOLEAN,
                    GraphTypeExpression.RequiredSingleItem),
                new IntrospectedItemPath(ItemPathRoots.Types, this.Name, "isRepeatable"),
                (directive) => directive.IsRepeatable.AsCompletedTask(),
                "A value indicating if the directive is repeatable on its target entity.");
        }
    }
}