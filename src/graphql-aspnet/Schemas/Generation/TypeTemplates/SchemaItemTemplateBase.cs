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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A base template defining common functionality across all template definitions.
    /// </summary>
    public abstract class SchemaItemTemplateBase : ISchemaItemTemplate
    {
        private bool? _isExplicitlyDeclared;
        private bool _isParsed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaItemTemplateBase"/> class.
        /// </summary>
        /// <param name="attributeProvider">The attribute provider.</param>
        protected SchemaItemTemplateBase(ICustomAttributeProvider attributeProvider)
        {
            this.AttributeProvider = Validation.ThrowIfNullOrReturn(attributeProvider, nameof(attributeProvider));
        }

        /// <summary>
        /// Parses the template contents according to the rules of the template.
        /// </summary>
        public void Parse()
        {
            if (_isParsed)
                return;

            _isParsed = true;
            this.ParseTemplateDefinition();
        }

        /// <summary>
        /// When overridden in a child class this method builds out the template according to its own individual requirements.
        /// </summary>
        protected virtual void ParseTemplateDefinition()
        {
            this.AppliedDirectives = this.ParseAppliedDiretives();
        }

        /// <summary>
        /// Inspects the attributes applied to this template for any directives that should
        /// be applied to the created schema item.
        /// </summary>
        /// <returns>IEnumerable&lt;IAppliedDirectiveTemplate&gt;.</returns>
        protected virtual IEnumerable<IAppliedDirectiveTemplate> ParseAppliedDiretives()
        {
            return this.ExtractAppliedDirectiveTemplates();
        }

        /// <inheritdoc />
        public virtual IEnumerable<DependentType> RetrieveRequiredTypes()
        {
            if (this.AppliedDirectives != null)
            {
                return this.AppliedDirectives
                    .Where(x => x.DirectiveType != null)
                    .Select(x => new DependentType(x.DirectiveType, TypeKind.DIRECTIVE));
            }

            return Enumerable.Empty<DependentType>();
        }

        /// <inheritdoc />
        public virtual void ValidateOrThrow()
        {
            if (!_isParsed)
            {
                throw new InvalidOperationException(
                    $"The graph item has not been parsed and cannot pass validation. Be sure to call {nameof(this.Parse)}() before attempting to " +
                    "validate this instance.");
            }

            if (this.AttributeProvider.SingleAttributeOfTypeOrDefault<GraphSkipAttribute>() != null)
            {
                throw new GraphTypeDeclarationException(
                    $"The graph item {this.InternalFullName} defines a {nameof(GraphSkipAttribute)}. It cannot be parsed or added " +
                    "to the object graph.",
                    this.ObjectType);
            }

            if (this.Route == null || !this.Route.IsValid)
            {
                throw new GraphTypeDeclarationException(
                        $"The template item '{this.InternalFullName}' declares an invalid route of '{this.Route?.Path ?? "<null>"}'. " +
                        $"Each segment of the route must conform to standard graphql naming rules. (Regex: {Constants.RegExPatterns.NameRegex} )",
                        this.ObjectType);
            }

            foreach (var directive in this.AppliedDirectives)
                directive.ValidateOrThrow();
        }

        /// <inheritdoc />
        public virtual Type ObjectType { get; protected set; }

        /// <inheritdoc />
        public virtual string Description { get; protected set; }

        /// <inheritdoc />
        public virtual string Name => this.Route?.Name;

        /// <inheritdoc />
        public SchemaItemPath Route { get; protected set; }

        /// <inheritdoc />
        public abstract string InternalFullName { get; }

        /// <inheritdoc />
        public abstract string InternalName { get; }

        /// <inheritdoc />
        public ICustomAttributeProvider AttributeProvider { get; }

        /// <inheritdoc />
        public virtual bool IsExplicitDeclaration
        {
            get
            {
                // this may be called prior to parsing
                // and this value needs to be correct regardless
                // of parse status
                if (_isExplicitlyDeclared.HasValue)
                    return _isExplicitlyDeclared.Value;

                _isExplicitlyDeclared = false;
                foreach (var attribute in this.AttributeProvider.GetCustomAttributes(false))
                {
                    if (attribute is GraphAttributeBase)
                    {
                        _isExplicitlyDeclared = true;
                        break;
                    }
                }

                return _isExplicitlyDeclared.Value;
            }
        }

        /// <inheritdoc />
        public IEnumerable<IAppliedDirectiveTemplate> AppliedDirectives { get; private set; }
    }
}