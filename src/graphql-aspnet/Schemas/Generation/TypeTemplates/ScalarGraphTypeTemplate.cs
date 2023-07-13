// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.TypeTemplates
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// An graph type template describing a SCALAR graph type.
    /// </summary>
    public class ScalarGraphTypeTemplate : GraphTypeTemplateBase, IScalarGraphTypeTemplate
    {
        private readonly Type _scalarType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScalarGraphTypeTemplate"/> class.
        /// </summary>
        /// <param name="typeToTemplate">The type to template.</param>
        public ScalarGraphTypeTemplate(Type typeToTemplate)
            : base(GlobalTypes.FindBuiltInScalarType(typeToTemplate) ?? typeToTemplate)
        {
            Validation.ThrowIfNull(typeToTemplate, nameof(typeToTemplate));
            _scalarType = GlobalTypes.FindBuiltInScalarType(typeToTemplate) ?? typeToTemplate;
        }

        /// <inheritdoc />
        protected override void ParseTemplateDefinition()
        {
            base.ParseTemplateDefinition();

            var instance = GlobalTypes.CreateScalarInstance(this.ScalarType);

            if (instance != null)
            {
                this.Route = new SchemaItemPath(SchemaItemPath.Join(SchemaItemCollections.Types, instance.Name));
                this.ObjectType = instance.ObjectType;
            }
        }

        /// <inheritdoc />
        public override void ValidateOrThrow()
        {
            base.ValidateOrThrow();
            GlobalTypes.ValidateScalarTypeOrThrow(this.ScalarType);
        }

        /// <inheritdoc />
        public override AppliedSecurityPolicyGroup SecurityPolicies { get; }

        /// <inheritdoc />
        public override TypeKind Kind => TypeKind.SCALAR;

        /// <inheritdoc />
        public override string InternalFullName => _scalarType.Name;

        /// <inheritdoc />
        public override string InternalName => _scalarType.Name;

        /// <inheritdoc />
        public Type ScalarType => _scalarType;
    }
}