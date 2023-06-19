// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration.Templates
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Configuration.Templates;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// An abstract class containing all the common elements across minimal field builders and
    /// their supporting classes.
    /// </summary>
    internal abstract class BaseRuntimeSchemaItemDefinition : Dictionary<string, object>, IGraphQLRuntimeSchemaItemDefinition
    {
        private IGraphQLRuntimeFieldDefinition _parent;
        private string _partialPathTemplate;

        /// <summary>
        /// Prevents a default instance of the <see cref="BaseRuntimeSchemaItemDefinition"/> class from being created.
        /// </summary>
        private BaseRuntimeSchemaItemDefinition()
        {
            this.Attributes = new List<Attribute>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRuntimeSchemaItemDefinition" /> class.
        /// </summary>
        /// <param name="options">The schema options where this field item
        /// is being defined.</param>
        /// <param name="partialPathTemplate">The partial path template defined for this
        /// individual entity.</param>
        protected BaseRuntimeSchemaItemDefinition(
            SchemaOptions options,
            string partialPathTemplate)
            : this()
        {
            _parent = null;
            this.Options = Validation.ThrowIfNullOrReturn(options, nameof(options));

            _partialPathTemplate = partialPathTemplate?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRuntimeSchemaItemDefinition"/> class.
        /// </summary>
        /// <param name="parentVirtualFieldBuilder">The group builder from which this entity is being created.</param>
        /// <param name="partialPathTemplate">The partial path template defined for this
        /// individual entity.</param>
        protected BaseRuntimeSchemaItemDefinition(
            IGraphQLRuntimeFieldDefinition parentVirtualFieldBuilder,
            string partialPathTemplate)
            : this()
        {
            _parent = Validation.ThrowIfNullOrReturn(parentVirtualFieldBuilder, nameof(parentVirtualFieldBuilder));
            this.Options = Validation.ThrowIfNullOrReturn(parentVirtualFieldBuilder?.Options, nameof(parentVirtualFieldBuilder.Options));

            _partialPathTemplate = Validation.ThrowIfNullWhiteSpaceOrReturn(partialPathTemplate, nameof(partialPathTemplate));
        }

        /// <inheritdoc />
        public SchemaItemPath CreatePath()
        {
            return new SchemaItemPath(this.Template);
        }

        /// <inheritdoc />
        public virtual SchemaOptions Options { get; protected set; }

        /// <inheritdoc />
        public IList<Attribute> Attributes { get; }

        /// <inheritdoc />
        public string Template
        {
            get
            {
                if (_parent != null)
                    return SchemaItemPath.Join(_parent.Template, _partialPathTemplate);

                return _partialPathTemplate;
            }
        }
    }
}