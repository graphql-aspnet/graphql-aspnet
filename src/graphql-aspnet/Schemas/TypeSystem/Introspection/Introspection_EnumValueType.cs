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
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Introspection.Model;

    /// <summary>
    /// Represents the graph type called "__EnumValue".
    /// </summary>
    [DebuggerDisplay("INTROSPECTION TYPE __EnumValue")]
    internal class Introspection_EnumValueType : BaseIntrospectionObjectType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Introspection_EnumValueType"/> class.
        /// </summary>
        public Introspection_EnumValueType()
            : base(Constants.ReservedNames.ENUM_VALUE_TYPE, nameof(Introspection_EnumValueType))
        {
            // "__EnumValue" type definition
            // https://graphql.github.io/graphql-spec/October2021/#sec-Introspection
            // -------------------------------------------------------------------------
            this.AddField<IntrospectedEnumValue, string>(
                "name",
                $"{this.InternalName}.{nameof(EnumValue.Name)}",
                new GraphTypeExpression(Constants.ScalarNames.STRING, MetaGraphTypes.IsNotNull),
                new IntrospectedRoutePath(SchemaItemCollections.Types, this.Name, "name"),
                (ev) => ev.Name.AsCompletedTask(),
                "The case-sensitive name of this value as it should be used in a query.");

            this.AddField<IntrospectedEnumValue, string>(
                "description",
                $"{this.InternalName}.{nameof(EnumValue.Description)}",
                new GraphTypeExpression(Constants.ScalarNames.STRING),
                new IntrospectedRoutePath(SchemaItemCollections.Types, this.Name, "description"),
                (ev) => ev.Description.AsCompletedTask(),
                "A human-friendly description of the value and what it means.");

            this.AddField<IntrospectedEnumValue, bool>(
                "isDeprecated",
                $"{this.InternalName}.{nameof(EnumValue.IsDeprecated)}",
                new GraphTypeExpression(Constants.ScalarNames.BOOLEAN, MetaGraphTypes.IsNotNull),
                new IntrospectedRoutePath(SchemaItemCollections.Types, this.Name, "isDeprecatedame"),
                (ev) => ev.IsDeprecated.AsCompletedTask(),
                "Indiates if this value is deprecated. Any deprecated value should not be used and " +
                "may be removed at a future date.");

            this.AddField<IntrospectedEnumValue, string>(
                "deprecationReason",
                $"{this.InternalName}.{nameof(EnumValue.DeprecationReason)}",
                new GraphTypeExpression(Constants.ScalarNames.STRING),
                new IntrospectedRoutePath(SchemaItemCollections.Types, this.Name, "deprecationReason"),
                (ev) => ev.DeprecationReason.AsCompletedTask(),
                "A human-friendly reason as to why this value has been deprecated.");
        }
    }
}