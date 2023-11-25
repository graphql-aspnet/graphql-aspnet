﻿// *************************************************************
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
    /// Represents the graph type '__Field.
    /// </summary>
    [DebuggerDisplay("INTROSPECTION TYPE __Field")]
    internal class Introspection_FieldType : BaseIntrospectionObjectType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Introspection_FieldType"/> class.
        /// </summary>
        public Introspection_FieldType()
            : base(Constants.ReservedNames.FIELD_TYPE, nameof(Introspection_FieldType))
        {
            // "__Field" type definition
            // https://graphql.github.io/graphql-spec/October2021/#sec-Introspection
            // -------------------------------------------------------------------------
            this.AddField<IntrospectedField, string>(
                "name",
                $"{this.InternalName}.{nameof(IntrospectedField.Name)}",
                new GraphTypeExpression(Constants.ScalarNames.STRING, MetaGraphTypes.IsNotNull),
                new IntrospectedRoutePath(SchemaItemPathCollections.Types, this.Name, "name"),
                (field) => field.Name.AsCompletedTask(),
                "The case-sensitive name of this field as it should be used in a query.");

            this.AddField<IntrospectedField, string>(
                "description",
                $"{this.InternalName}.{nameof(IntrospectedField.Description)}",
                new GraphTypeExpression(Constants.ScalarNames.STRING),
                new IntrospectedRoutePath(SchemaItemPathCollections.Types, this.Name, "description"),
                (field) => field.Description.AsCompletedTask(),
                "Indiates if this field is deprecated. Any deprecated field should not be used and " +
                "may be removed at a future date.");

            this.AddField<IntrospectedField, IReadOnlyList<IntrospectedInputValueType>>(
                "args",
                $"{this.InternalName}.{nameof(IntrospectedField.Arguments)}",
                new GraphTypeExpression(Constants.ReservedNames.INPUT_VALUE_TYPE, GraphTypeExpression.RequiredListRequiredItem),
                new IntrospectedRoutePath(SchemaItemPathCollections.Types, this.Name, "args"),
                (field) => field.Arguments.AsCompletedTask(),
                "A collection of input values that can be passed to this field to alter its behavior when used in a query.");

            this.AddField<IntrospectedField, IntrospectedType>(
                "type",
                $"{this.InternalName}.{nameof(IntrospectedField.IntrospectedGraphType)}",
                new GraphTypeExpression(Constants.ReservedNames.TYPE_TYPE, MetaGraphTypes.IsNotNull),
                new IntrospectedRoutePath(SchemaItemPathCollections.Types, this.Name, "type"),
                (field) => field.IntrospectedGraphType.AsCompletedTask(),
                "The graph type returned by this field.");

            this.AddField<IntrospectedField, bool>(
                "isDeprecated",
                $"{this.InternalName}.{nameof(IntrospectedField.IsDeprecated)}",
                new GraphTypeExpression(Constants.ScalarNames.BOOLEAN, MetaGraphTypes.IsNotNull),
                new IntrospectedRoutePath(SchemaItemPathCollections.Types, this.Name, "isDeprecated"),
                (field) => field.IsDeprecated.AsCompletedTask(),
                "Indiates if this field is deprecated. Any deprecated field should not be used and " +
                "may be removed at a future date.");

            this.AddField<IntrospectedField, string>(
                "deprecationReason",
                $"{this.InternalName}.{nameof(IntrospectedField.DeprecationReason)}",
                new GraphTypeExpression(Constants.ScalarNames.STRING),
                new IntrospectedRoutePath(SchemaItemPathCollections.Types, this.Name, "deprecationReason"),
                (field) => field.DeprecationReason.AsCompletedTask(),
                "A human-friendly reason as to why this field has been deprecated.");
        }
    }
}