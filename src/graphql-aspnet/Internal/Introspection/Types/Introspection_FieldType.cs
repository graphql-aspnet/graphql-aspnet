// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Introspection.Types
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Internal.Introspection.Model;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Represents the meta graph type '__Field.
    /// </summary>
    [DebuggerDisplay("INTROSPECTION TYPE __Field")]
    internal class Introspection_FieldType : BaseIntrospectionObjectType
    {
        /// <summary>
        /// Gets the instance of this meta-type.
        /// </summary>
        /// <value>The instance.</value>
        public static Introspection_FieldType Instance { get; } = new Introspection_FieldType();

        /// <summary>
        /// Initializes static members of the <see cref="Introspection_FieldType" /> class.
        /// </summary>
        static Introspection_FieldType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Introspection_FieldType"/> class.
        /// </summary>
        private Introspection_FieldType()
            : base(Constants.ReservedNames.FIELD_TYPE)
        {
            // "__Field" type definition
            // https://graphql.github.io/graphql-spec/June2018/#sec-Introspection
            // -------------------------------------------------------------------------
            this.GraphFieldCollection.AddField<IntrospectedField, string>(
                "name",
                new GraphTypeExpression(Constants.ScalarNames.STRING, MetaGraphTypes.IsNotNull),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "name"),
                (field) => field.Name.AsCompletedTask(),
                "The case-sensitive name of this field as it should be used in a query.");

            this.GraphFieldCollection.AddField<IntrospectedField, string>(
                "description",
                new GraphTypeExpression(Constants.ScalarNames.STRING),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "description"),
                (field) => field.Description.AsCompletedTask(),
                "Indiates if this field is deprecated. Any deprecated field should not be used and " +
                "may be removed at a future date.");

            this.GraphFieldCollection.AddField<IntrospectedField, IReadOnlyList<IntrospectedInputValueType>>(
                "args",
                new GraphTypeExpression(Constants.ReservedNames.INPUT_VALUE_TYPE, GraphTypeExpression.RequiredListRequiredItem),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "args"),
                (field) => field.Arguments.AsCompletedTask(),
                "A collection of input values that can be passed to this field to alter its behavior when used in a query.");

            this.GraphFieldCollection.AddField<IntrospectedField, IntrospectedType>(
                "type",
                new GraphTypeExpression(Constants.ReservedNames.TYPE_TYPE, MetaGraphTypes.IsNotNull),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "type"),
                (field) => field.IntrospectedGraphType.AsCompletedTask(),
                "The graph type returned by this field.");

            this.GraphFieldCollection.AddField<IntrospectedField, bool>(
                "isDeprecated",
                new GraphTypeExpression(Constants.ScalarNames.BOOLEAN, MetaGraphTypes.IsNotNull),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "isDeprecated"),
                (field) => field.IsDeprecated.AsCompletedTask(),
                "Indiates if this field is deprecated. Any deprecated field should not be used and " +
                "may be removed at a future date.");

            this.GraphFieldCollection.AddField<IntrospectedField, string>(
                "deprecationReason",
                new GraphTypeExpression(Constants.ScalarNames.STRING),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "deprecationReason"),
                (field) => field.DeprecationReason.AsCompletedTask(),
                "A human-friendly reason as to why this field has been deprecated.");
        }
    }
}