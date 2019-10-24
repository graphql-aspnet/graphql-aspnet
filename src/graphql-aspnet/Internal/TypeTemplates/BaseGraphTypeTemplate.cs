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
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A base representation of a template for an any graph type containing common elements amongst all types.
    /// </summary>
    public abstract class BaseGraphTypeTemplate : BaseItemTemplate, IGraphTypeTemplate
    {
        private TemplateDeclarationRequirements? _fieldDeclarationOverrides;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseGraphTypeTemplate"/> class.
        /// </summary>
        /// <param name="attributeProvider">The attribute provider.</param>
        protected BaseGraphTypeTemplate(ICustomAttributeProvider attributeProvider)
            : base(attributeProvider)
        {
            this.Publish = true;
        }

        /// <summary>
        /// When overridden in a child class this method builds out the template according to its own individual requirements.
        /// </summary>
        protected override void ParseTemplateDefinition()
        {
            // account for type level field declaration overrides
            var graphTypeDeclaration = this.SingleAttributeOrDefault<GraphTypeAttribute>();
            if (graphTypeDeclaration != null)
            {
                this.Publish = graphTypeDeclaration.Publish;
                if (graphTypeDeclaration.RequirementsWereDeclared)
                {
                    _fieldDeclarationOverrides = graphTypeDeclaration.FieldDeclarationRequirements;
                }
            }
        }

        /// <summary>
        /// Gets the security policies found via defined attributes on the item that need to be enforced.
        /// </summary>
        /// <value>The security policies.</value>
        public abstract FieldSecurityGroup SecurityPolicies { get; }

        /// <summary>
        /// Gets the kind of graph type that can be made from this template.
        /// </summary>
        /// <value>The kind.</value>
        public abstract TypeKind Kind { get; }

        /// <summary>
        /// Gets the declaration requirements, if any, that this template defines as needing to be inforced for its specific templated
        /// type.
        /// </summary>
        /// <value>The declaration requirements.</value>
        public TemplateDeclarationRequirements? DeclarationRequirements => _fieldDeclarationOverrides;

        /// <summary>
        /// Gets a value indicating whether this instance is marked such that graph types
        /// made from it are published in introspection queries.
        /// </summary>
        /// <value><c>true</c> if publish; otherwise, <c>false</c>.</value>
        public bool Publish { get; private set; }
    }
}