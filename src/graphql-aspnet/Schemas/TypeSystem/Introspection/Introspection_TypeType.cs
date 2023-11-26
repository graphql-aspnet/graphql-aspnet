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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Resolvers.Introspeection;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Introspection.Model;

    /// <summary>
    /// Represents graph type called "__Type".
    /// </summary>
    [DebuggerDisplay("INTROSPECTION TYPE __Type")]
    internal class Introspection_TypeType : BaseIntrospectionObjectType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Introspection_TypeType" /> class.
        /// </summary>
        public Introspection_TypeType()
            : base(Constants.ReservedNames.TYPE_TYPE, nameof(Introspection_TypeType))
        {
            // "__Type" type definition
            // https://graphql.github.io/graphql-spec/October2021/#sec-Introspection
            // -------------------------------------------------------------------------
            this.AddField<IntrospectedType, TypeKind>(
                "kind",
                $"{this.InternalName}.{nameof(IntrospectedType.Kind)}",
                new GraphTypeExpression(Constants.ReservedNames.TYPE_KIND_ENUM, MetaGraphTypes.IsNotNull),
                new IntrospectedItemPath(ItemPathRoots.Types, this.Name, "kind"),
                (gt) => Task.FromResult(gt?.Kind ?? TypeKind.NONE),
                $"The specific {Constants.ReservedNames.TYPE_KIND_ENUM} of this graph type.");

            this.AddField<IntrospectedType, string>(
                "name",
                $"{this.InternalName}.{nameof(IntrospectedType.Name)}",
                new GraphTypeExpression(Constants.ScalarNames.STRING),
                new IntrospectedItemPath(ItemPathRoots.Types, this.Name, "name"),
                (gt) => Task.FromResult(gt?.Name),
                "The case-sensitive name of this graph type as it appears in the object graph");

            this.AddField<IntrospectedType, string>(
                "description",
                $"{this.InternalName}.{nameof(IntrospectedType.Description)}",
                new GraphTypeExpression(Constants.ScalarNames.STRING),
                new IntrospectedItemPath(ItemPathRoots.Types, this.Name, "description"),
                (gt) => Task.FromResult(gt?.Description),
                "a human-readable phrase describing what this type represents.");

            // fields
            IGraphField fieldsField = new MethodGraphField(
                "fields",
                $"{this.InternalName}.{nameof(IntrospectedType.Fields)}",
                new GraphTypeExpression(Constants.ReservedNames.FIELD_TYPE, MetaGraphTypes.IsList, MetaGraphTypes.IsNotNull),
                new IntrospectedItemPath(ItemPathRoots.Types, this.Name, "fields"),
                declaredReturnType: typeof(IEnumerable<IntrospectedField>),
                objectType: typeof(IEnumerable<IntrospectedField>),
                mode: FieldResolutionMode.PerSourceItem,
                resolver: new Type_TypeGraphFieldResolver())
            {
                Description = "A list of navigable fields, if any, this type declares.",
            };

            fieldsField.Arguments.AddArgument(
                Constants.ReservedNames.DEPRECATED_ARGUMENT_NAME,
                Constants.ReservedNames.DEPRECATED_ARGUMENT_NAME,
                new GraphTypeExpression(Constants.ScalarNames.BOOLEAN),
                typeof(bool),
                false);

            fieldsField = fieldsField.Clone(this);
            this.GraphFieldCollection.AddField(fieldsField);

            // interfaces
            this.AddField<IntrospectedType, IReadOnlyList<IntrospectedType>>(
                "interfaces",
                $"{this.InternalName}.{nameof(IntrospectedType.Interfaces)}",
                new GraphTypeExpression(Constants.ReservedNames.TYPE_TYPE, MetaGraphTypes.IsList, MetaGraphTypes.IsNotNull),
                new IntrospectedItemPath(ItemPathRoots.Types, this.Name, "interfaces"),
                (gt) => Task.FromResult(gt?.Interfaces),
                $"For {TypeKind.OBJECT.ToString()} types, contains a list of interface this type implements; otherwise null.");

            // possibleTypes
            this.AddField<IntrospectedType, IReadOnlyList<IntrospectedType>>(
                "possibleTypes",
                $"{this.InternalName}.{nameof(IntrospectedType.PossibleTypes)}",
                new GraphTypeExpression(Constants.ReservedNames.TYPE_TYPE, MetaGraphTypes.IsList, MetaGraphTypes.IsNotNull),
                new IntrospectedItemPath(ItemPathRoots.Types, this.Name, "possibleTypes"),
                (gt) => Task.FromResult(gt?.PossibleTypes),
                $"For {TypeKind.INTERFACE.ToString()} and {TypeKind.UNION.ToString()} types, declares the possible types that implement the interface or are included " +
                "in the union; otherwise, null.");

            // enumValues
            IGraphField enumValuesField = new MethodGraphField(
                "enumValues",
                $"{this.InternalName}.{nameof(IntrospectedType.EnumValues)}",
                new GraphTypeExpression(Constants.ReservedNames.ENUM_VALUE_TYPE, MetaGraphTypes.IsList, MetaGraphTypes.IsNotNull),
                new IntrospectedItemPath(ItemPathRoots.Types, this.Name, Constants.ReservedNames.ENUM_VALUE_TYPE),
                declaredReturnType: typeof(IEnumerable<IntrospectedEnumValue>),
                objectType: typeof(IEnumerable<IntrospectedEnumValue>),
                mode: FieldResolutionMode.PerSourceItem,
                resolver: new Type_EnumValuesGraphFieldResolver())
            {
                Description = $"For {TypeKind.ENUM.ToString()} types, declares the possible values that can be used for the enumeration; otherwise null.",
            };

            enumValuesField.Arguments.AddArgument(
                Constants.ReservedNames.DEPRECATED_ARGUMENT_NAME,
                Constants.ReservedNames.DEPRECATED_ARGUMENT_NAME,
                new GraphTypeExpression(Constants.ScalarNames.BOOLEAN),
                typeof(bool),
                false);

            enumValuesField = enumValuesField.Clone(this);
            this.GraphFieldCollection.AddField(enumValuesField);

            // inputFields
            this.AddField<IntrospectedType, IReadOnlyList<IntrospectedInputValueType>>(
                "inputFields",
                $"{this.InternalName}.{nameof(IntrospectedType.InputFields)}",
                new GraphTypeExpression(Constants.ReservedNames.INPUT_VALUE_TYPE, MetaGraphTypes.IsList, MetaGraphTypes.IsNotNull),
                new IntrospectedItemPath(ItemPathRoots.Types, this.Name, "inputFields"),
                (gt) => Task.FromResult(gt?.InputFields),
                $"For {TypeKind.INPUT_OBJECT.ToString()} types, declares the fields that need to be supplied when submitting an object on a query; otherwise null.");

            // ofType
            this.AddField<IntrospectedType, IntrospectedType>(
                "ofType",
                $"{this.InternalName}.{nameof(IntrospectedType.OfType)}",
                new GraphTypeExpression(Constants.ReservedNames.TYPE_TYPE),
                new IntrospectedItemPath(ItemPathRoots.Types, this.Name, "ofType"),
                (gt) => Task.FromResult(gt?.OfType),
                $"For {TypeKind.NON_NULL.ToString()} and {TypeKind.LIST.ToString()} meta types, declares the underlying type that is " +
                "wrapped by this type; otherwise null.");

            // specifiedByURL
            this.AddField<IntrospectedType, string>(
                "specifiedByURL",
                $"{this.InternalName}.{nameof(IntrospectedType.SpecifiedByUrl)}",
                new GraphTypeExpression(Constants.ScalarNames.STRING),
                new IntrospectedItemPath(ItemPathRoots.Types, this.Name, "specifiedByURL"),
                (gt) => Task.FromResult(gt?.SpecifiedByUrl),
                "A string, in the form of a URL, pointing to a specification " +
                "if the graph type is a scalar, otherwise null.");
        }
    }
}