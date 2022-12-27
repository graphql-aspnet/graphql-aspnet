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

        /// <summary>
        /// When overridden in a child class this method builds out the template according to its own individual requirements.
        /// </summary>
        protected override void ParseTemplateDefinition()
        {
            base.ParseTemplateDefinition();

            // account for type level field declaration overrides
            var graphTypeDeclaration = this.AttributeProvider.SingleAttributeOrDefault<GraphTypeAttribute>();
            if (graphTypeDeclaration != null)
            {
                this.Publish = graphTypeDeclaration.Publish;
                if (graphTypeDeclaration.RequirementsWereDeclared)
                {
                    _fieldDeclarationOverrides = graphTypeDeclaration.FieldDeclarationRequirements;
                }
            }
        }

        /// <inheritdoc />
        public abstract AppliedSecurityPolicyGroup SecurityPolicies { get; }

        /// <inheritdoc />
        public abstract TypeKind Kind { get; }

        /// <inheritdoc />
        public TemplateDeclarationRequirements? DeclarationRequirements => _fieldDeclarationOverrides;

        /// <inheritdoc />
        public bool Publish { get; private set; }
    }
}