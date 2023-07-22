// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.TypeTemplates
{
    using System.Reflection;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A base template defining common functionality across all graph type template definitions.
    /// </summary>
    public abstract class GraphTypeTemplateBase : SchemaItemTemplateBase, IGraphTypeTemplate
    {
        private TemplateDeclarationRequirements? _fieldDeclarationOverrides;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphTypeTemplateBase"/> class.
        /// </summary>
        /// <param name="attributeProvider">The attribute provider.</param>
        protected GraphTypeTemplateBase(ICustomAttributeProvider attributeProvider)
            : base(attributeProvider)
        {
            this.Publish = true;
        }

        /// <inheritdoc />
        protected override void ParseTemplateDefinition()
        {
            base.ParseTemplateDefinition();

            // account for type level field declaration overrides
            var graphTypeDeclaration = this.AttributeProvider.SingleAttributeOrDefault<GraphTypeAttribute>();
            if (graphTypeDeclaration != null)
            {
                this.Publish = graphTypeDeclaration.Publish;

                if (string.IsNullOrEmpty(this.InternalName))
                    this.InternalName = graphTypeDeclaration.InternalName;
                if (graphTypeDeclaration.RequirementsWereDeclared)
                {
                    _fieldDeclarationOverrides = graphTypeDeclaration.FieldDeclarationRequirements;
                }
            }

            if (string.IsNullOrWhiteSpace(this.InternalName))
                this.InternalName = this.ObjectType?.FriendlyName();
        }

        /// <inheritdoc />
        public abstract AppliedSecurityPolicyGroup SecurityPolicies { get; }

        /// <inheritdoc />
        public abstract TypeKind Kind { get; }

        /// <inheritdoc />
        public virtual TemplateDeclarationRequirements? DeclarationRequirements => _fieldDeclarationOverrides;

        /// <inheritdoc />
        public virtual bool Publish { get; private set; }
    }
}