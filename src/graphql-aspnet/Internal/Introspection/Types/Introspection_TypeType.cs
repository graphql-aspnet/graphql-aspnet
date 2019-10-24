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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Internal.Introspection.Model;
    using GraphQL.AspNet.Internal.Introspection.Resolvers;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Represents the meta-type called "__Type".
    /// </summary>
    [DebuggerDisplay("INTROSPECTION TYPE __Type")]
    internal class Introspection_TypeType : BaseIntrospectionObjectType
    {
        /// <summary>
        /// Gets the instance of this meta-type.
        /// </summary>
        /// <value>The instance.</value>
        public static Introspection_TypeType Instance { get; } = new Introspection_TypeType();

        /// <summary>
        /// Initializes static members of the <see cref="Introspection_TypeType"/> class.
        /// </summary>
        static Introspection_TypeType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Introspection_TypeType" /> class.
        /// </summary>
        private Introspection_TypeType()
            : base(Constants.ReservedNames.TYPE_TYPE)
        {
            // "__Type" type definition
            // https://graphql.github.io/graphql-spec/June2018/#sec-Introspection
            // -------------------------------------------------------------------------
            this.GraphFieldCollection.AddField<IntrospectedType, TypeKind>(
                "kind",
                new GraphTypeExpression(Constants.ReservedNames.TYPE_KIND_ENUM, MetaGraphTypes.IsNotNull),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "kind"),
                (gt) => Task.FromResult(gt?.Kind ?? TypeKind.NONE),
                $"The specific {Constants.ReservedNames.TYPE_KIND_ENUM} of this graph type.");

            this.GraphFieldCollection.AddField<IntrospectedType, string>(
                "name",
                new GraphTypeExpression(Constants.ScalarNames.STRING),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "name"),
                (gt) => Task.FromResult(gt?.Name),
                "The case sensitive name of this graph type as it appears in the object graph");

            this.GraphFieldCollection.AddField<IntrospectedType, string>(
                "description",
                new GraphTypeExpression(Constants.ScalarNames.STRING),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "description"),
                (gt) => Task.FromResult(gt?.Description),
                "a human-readable phrase describing what this type represents.");

            // fields
            var fieldsField = new MethodGraphField(
                "fields",
                new GraphTypeExpression(Constants.ReservedNames.FIELD_TYPE, MetaGraphTypes.IsList, MetaGraphTypes.IsNotNull),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "fields"),
                FieldResolutionMode.PerSourceItem,
                new Type_TypeFieldResolver())
            {
                Description = "A list of navigable fields, if any, this type declares.",
            };

            fieldsField.Arguments.AddArgument(
                Constants.ReservedNames.DEPRECATED_ARGUMENT_NAME,
                Constants.ReservedNames.DEPRECATED_ARGUMENT_NAME,
                new GraphTypeExpression(Constants.ScalarNames.BOOLEAN),
                typeof(bool),
                false);
            this.GraphFieldCollection.AddField(fieldsField);

            // interfaces
            this.GraphFieldCollection.AddField<IntrospectedType, IReadOnlyList<IntrospectedType>>(
                "interfaces",
                new GraphTypeExpression(Constants.ReservedNames.TYPE_TYPE, MetaGraphTypes.IsList, MetaGraphTypes.IsNotNull),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "interfaces"),
                (gt) => Task.FromResult(gt?.Interfaces),
                $"For {TypeKind.OBJECT.ToString()} types, contains a list of interface this type implements; otherwise null.");

            // possibleTypes
            this.GraphFieldCollection.AddField<IntrospectedType, IReadOnlyList<IntrospectedType>>(
                "possibleTypes",
                new GraphTypeExpression(Constants.ReservedNames.TYPE_TYPE, MetaGraphTypes.IsList, MetaGraphTypes.IsNotNull),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "possibleTypes"),
                (gt) => Task.FromResult(gt?.PossibleTypes),
                $"For {TypeKind.INTERFACE.ToString()} and {TypeKind.UNION.ToString()} types, declares the possible types that implement the interface or are included " +
                "in the union; otherwise, null.");

            // enumValues
            var enumValuesField = new MethodGraphField(
                "enumValues",
                new GraphTypeExpression(Constants.ReservedNames.ENUM_VALUE_TYPE, MetaGraphTypes.IsList, MetaGraphTypes.IsNotNull),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, Constants.ReservedNames.ENUM_VALUE_TYPE),
                FieldResolutionMode.PerSourceItem,
                new Type_EnumValuesResolver())
            {
                Description = $"For {TypeKind.ENUM.ToString()} types, declares the possible values that can be used for the enumeration; otherwise null.",
            };

            enumValuesField.Arguments.AddArgument(
                Constants.ReservedNames.DEPRECATED_ARGUMENT_NAME,
                Constants.ReservedNames.DEPRECATED_ARGUMENT_NAME,
                new GraphTypeExpression(Constants.ScalarNames.BOOLEAN),
                typeof(bool),
                false);
            this.GraphFieldCollection.AddField(enumValuesField);

            // inputFields
            this.GraphFieldCollection.AddField<IntrospectedType, IReadOnlyList<IntrospectedInputValueType>>(
                "inputFields",
                new GraphTypeExpression(Constants.ReservedNames.INPUT_VALUE_TYPE, MetaGraphTypes.IsList, MetaGraphTypes.IsNotNull),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "inputFields"),
                (gt) => Task.FromResult(gt?.InputFields),
                $"For {TypeKind.INPUT_OBJECT.ToString()} types, declares the fields that need to be supplied when submitting the value on a query; otherwise null.");

            // ofType
            this.GraphFieldCollection.AddField<IntrospectedType, IntrospectedType>(
                "ofType",
                new GraphTypeExpression(Constants.ReservedNames.TYPE_TYPE),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "ofType"),
                (gt) => Task.FromResult(gt?.OfType),
                $"For {TypeKind.NON_NULL.ToString()} and {TypeKind.LIST.ToString()} meta types, declare the underlying type that is " +
                "wrapped by this type; otherwise null.");
        }
    }
}