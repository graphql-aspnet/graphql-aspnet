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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A base template defining common attribute across all template definitions.
    /// </summary>
    public abstract class BaseItemTemplate : IGraphItemTemplate
    {
        private bool? _isExplicitlyDeclared;
        private bool _isParsed;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseItemTemplate"/> class.
        /// </summary>
        /// <param name="attributeProvider">The attribute provider.</param>
        protected BaseItemTemplate(ICustomAttributeProvider attributeProvider)
        {
            this.AttributeProvider = Validation.ThrowIfNullOrReturn(attributeProvider, nameof(attributeProvider));
        }

        /// <summary>
        /// Retrieves the single attribute of the given type (or castable to the given type) if it is declared on this instance; otherwise null. This method
        /// limits itself to only those attributes that are decalred once (as is the case with all required graph attributes).  If an attribute
        /// is declared on the provider more than once no instances will not be returned.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the graph attribute to return.</typeparam>
        /// <returns>TAttribute.</returns>
        protected TAttribute SingleAttributeOfTypeOrDefault<TAttribute>()
            where TAttribute : Attribute
        {
            return this.AttributeProvider.SingleAttributeOfTypeOrDefault<TAttribute>();
        }

        /// <summary>
        /// Retrieves the single attribute of the given type if it is declared on this instance; otherwise null. This method
        /// limits itself to only those attributes that are decalred once (as is the case with all required graph attributes).  If an attribute
        /// is declared on the provider more than once no instances will not be returned.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the graph attribute to return.</typeparam>
        /// <returns>TAttribute.</returns>
        protected TAttribute SingleAttributeOrDefault<TAttribute>()
            where TAttribute : Attribute
        {
            return this.AttributeProvider.SingleAttributeOrDefault<TAttribute>();
        }

        /// <summary>
        /// Retrieves a set of attribute that match the provided filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="includeInheritedAttributes">if set to <c>true</c> attributes inherited in the dependency chain of the attribute provider will also be inspected.</param>
        /// <returns>TAttribute.</returns>
        protected IEnumerable<Attribute> RetrieveAttributes(Func<Attribute, bool> filter, bool includeInheritedAttributes = true)
        {
            return this.AttributeProvider.GetCustomAttributes(includeInheritedAttributes).Where(x => x is Attribute attrib && filter(attrib)).Cast<Attribute>();
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
            this.Directives = this.ExtractAppliedDirectiveTemplates();
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

            if (this.SingleAttributeOfTypeOrDefault<GraphSkipAttribute>() != null)
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

            foreach (var directive in this.Directives)
                directive.ValidateOrThrow();
        }

        /// <inheritdoc />
        public virtual Type ObjectType { get; protected set; }

        /// <inheritdoc />
        public virtual string Description { get; protected set; }

        /// <inheritdoc />
        public virtual string Name => this.Route?.Name;

        /// <inheritdoc />
        public GraphFieldPath Route { get; protected set; }

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
                    if (attribute is BaseGraphAttribute)
                    {
                        _isExplicitlyDeclared = true;
                        break;
                    }
                }

                return _isExplicitlyDeclared.Value;
            }
        }

        /// <inheritdoc />
        public IEnumerable<IAppliedDirectiveTemplate> Directives { get; private set; }
    }
}