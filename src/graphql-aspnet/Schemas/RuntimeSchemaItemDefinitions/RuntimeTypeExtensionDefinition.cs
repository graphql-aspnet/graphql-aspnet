// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.RuntimeSchemaItemDefinitions
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An internal implementation of the <see cref="IGraphQLRuntimeFieldDefinition"/>
    /// used to generate new type extensions via a minimal api style of coding.
    /// </summary>
    [DebuggerDisplay("{Template}")]
    internal class RuntimeTypeExtensionDefinition : RuntimeResolvedFieldDefinition, IGraphQLRuntimeTypeExtensionDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeTypeExtensionDefinition"/> class.
        /// </summary>
        /// <param name="schemaOptions">The schema options where this type extension is being declared.</param>
        /// <param name="typeToExtend">The target OBJECT or INTERFACE type to extend.</param>
        /// <param name="fieldName">Name of the field to add to the <paramref name="typeToExtend"/>.</param>
        /// <param name="resolutionMode">The resolution mode for the resolver implemented by this
        /// type extension.</param>
        public RuntimeTypeExtensionDefinition(
            SchemaOptions schemaOptions,
            Type typeToExtend,
            string fieldName,
            FieldResolutionMode resolutionMode)
            : base(
                  schemaOptions,
                  SchemaItemCollections.Types,
                  SchemaItemPath.Join(
                        GraphTypeNames.ParseName(typeToExtend, TypeKind.OBJECT),
                        fieldName))
        {
            this.ExecutionMode = resolutionMode;
            this.TargetType = typeToExtend;
        }

        /// <inheritdoc />
        protected override Attribute CreatePrimaryAttribute()
        {
            switch (this.ExecutionMode)
            {
                case FieldResolutionMode.PerSourceItem:
                    return new TypeExtensionAttribute(this.TargetType, this.Route.Name, this.ReturnType);

                case FieldResolutionMode.Batch:
                    return new BatchTypeExtensionAttribute(this.TargetType, this.Route.Name, this.ReturnType);

                default:
                    throw new NotSupportedException(
                        $"Unknown {nameof(FieldResolutionMode)}. cannot render " +
                        $"primary type extension attribute.");
            }
        }

        /// <inheritdoc />
        public FieldResolutionMode ExecutionMode { get; set; }

        /// <inheritdoc />
        public Type TargetType { get; }
    }
}