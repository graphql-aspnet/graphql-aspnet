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
        protected abstract void ParseTemplateDefinition();

        /// <summary>
        /// When overridden in a child class, allows the template to perform some final validation checks
        /// on the integrity of itself. An exception should be thrown to stop the template from being
        /// persisted if the object is unusable or otherwise invalid in the manner its been built.
        /// </summary>
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
        }

        /// <summary>
        /// When overridden in a child class, this method builds the unique field path that will be assigned to this instance
        /// using the implementation rules of the concrete type.
        /// </summary>
        /// <returns>GraphRoutePath.</returns>
        protected abstract GraphFieldPath GenerateFieldPath();

        /// <summary>
        /// Gets or sets the singular concrete type this definition represents in the object graph.
        /// </summary>
        /// <value>The type of the object.</value>
        public Type ObjectType { get; protected set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; protected set; }

        /// <summary>
        /// Gets the name of the item on the object graph as it is conveyed in an introspection request.
        /// </summary>
        /// <value>The name.</value>
        public virtual string Name => this.Route?.Name;

        /// <summary>
        /// Gets or sets a the canonical path on the graph where this item sits.
        /// </summary>
        /// <value>The route.</value>
        public GraphFieldPath Route { get; protected set; }

        /// <summary>
        /// Gets the fully qualified name, including namespace, of this item as it exists in the .NET code (e.g. 'Namespace.ObjectType.MethodName').
        /// </summary>
        /// <value>The internal name given to this item.</value>
        public abstract string InternalFullName { get; }

        /// <summary>
        /// Gets the name that defines this item within the .NET code of the application; typically a method name or property name.
        /// </summary>
        /// <value>The internal name given to this item.</value>
        public abstract string InternalName { get; }

        /// <summary>
        /// Gets the attribute provider for this template item.
        /// </summary>
        /// <value>The attribute provider.</value>
        protected ICustomAttributeProvider AttributeProvider { get; }

        /// <summary>
        /// Gets a value indicating whether this instance was explictly declared as a graph item via acceptable attribution or
        /// if it was parsed as a matter of completeness.
        /// </summary>
        /// <value><c>true</c> if this instance is explictly declared; otherwise, <c>false</c>.</value>
        public virtual bool IsExplicitDeclaration
        {
            get
            {
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
    }
}