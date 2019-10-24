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
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Internal.Introspection.Model;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Represents the meta-type called "__EnumValue".
    /// </summary>
    [DebuggerDisplay("INTROSPECTION TYPE __EnumValue")]
    internal class Introspection_EnumValueType : BaseIntrospectionObjectType
    {
        /// <summary>
        /// Gets the instance of this meta-type.
        /// </summary>
        /// <value>The instance.</value>
        public static Introspection_EnumValueType Instance { get; } = new Introspection_EnumValueType();

        /// <summary>
        /// Initializes static members of the <see cref="Introspection_EnumValueType"/> class.
        /// </summary>
        static Introspection_EnumValueType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Introspection_EnumValueType"/> class.
        /// </summary>
        private Introspection_EnumValueType()
            : base(Constants.ReservedNames.ENUM_VALUE_TYPE)
        {
            // "__EnumValue" type definition
            // https://graphql.github.io/graphql-spec/June2018/#sec-Introspection
            // -------------------------------------------------------------------------
            this.GraphFieldCollection.AddField<IntrospectedEnumValue, string>(
                "name",
                new GraphTypeExpression(Constants.ScalarNames.STRING, MetaGraphTypes.IsNotNull),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "name"),
                (ev) => ev.Name.AsCompletedTask(),
                "The case-sensitive name of this value as it should be used in a query.");

            this.GraphFieldCollection.AddField<IntrospectedEnumValue, string>(
                "description",
                new GraphTypeExpression(Constants.ScalarNames.STRING),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "description"),
                (ev) => ev.Description.AsCompletedTask(),
                "A human-friendly description of the value and what it means.");

            this.GraphFieldCollection.AddField<IntrospectedEnumValue, bool>(
                "isDeprecated",
                new GraphTypeExpression(Constants.ScalarNames.BOOLEAN, MetaGraphTypes.IsNotNull),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "isDeprecatedame"),
                (ev) => ev.IsDeprecated.AsCompletedTask(),
                "Indiates if this value is deprecated. Any deprecated value should not be used and " +
                "may be removed at a future date.");

            this.GraphFieldCollection.AddField<IntrospectedEnumValue, string>(
                "deprecationReason",
                new GraphTypeExpression(Constants.ScalarNames.STRING),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "deprecationReason"),
                (ev) => ev.DeprecationReason.AsCompletedTask(),
                "A human-friendly reason as to why this value has been deprecated.");

            this.ObjectType = typeof(Introspection_EnumValueType);
            this.InternalName = this.ObjectType.FriendlyName();
        }

        /// <summary>
        /// Gets the type of the object this graph type was made from.
        /// </summary>
        /// <value>The type of the object.</value>
        public Type ObjectType { get; }

        /// <summary>
        /// Gets a fully qualified name of the type as it exists on the server (i.e.  Namespace.ClassName). This name
        /// is used in many exceptions and internal error messages.
        /// </summary>
        /// <value>The name of the internal.</value>
        public string InternalName { get; }
    }
}