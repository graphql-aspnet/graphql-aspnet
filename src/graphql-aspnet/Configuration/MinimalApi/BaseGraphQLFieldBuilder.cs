// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration.MinimalApi
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// An abstract class containing all the common elements across minimal field builders and
    /// their supporting classes.
    /// </summary>
    internal abstract class BaseGraphQLFieldBuilder : Dictionary<string, object>
    {
        private IGraphQLFieldGroupBuilder _parent;
        private string _partialPathTemplate;

        /// <summary>
        /// Prevents a default instance of the <see cref="BaseGraphQLFieldBuilder"/> class from being created.
        /// </summary>
        private BaseGraphQLFieldBuilder()
        {
            this.Attributes = new List<Attribute>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseGraphQLFieldBuilder" /> class.
        /// </summary>
        /// <param name="options">The schema options where this field item
        /// is being defined.</param>
        /// <param name="partialPathTemplate">The partial path template defined for this
        /// individual entity.</param>
        protected BaseGraphQLFieldBuilder(
            SchemaOptions options,
            string partialPathTemplate)
            : this()
        {
            _parent = null;
            this.Options = Validation.ThrowIfNullOrReturn(options, nameof(options));

            _partialPathTemplate = partialPathTemplate?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseGraphQLFieldBuilder"/> class.
        /// </summary>
        /// <param name="groupBuilder">The group builder from which this entity is being created.</param>
        /// <param name="partialPathTemplate">The partial path template defined for this
        /// individual entity.</param>
        protected BaseGraphQLFieldBuilder(
            IGraphQLFieldGroupBuilder groupBuilder,
            string partialPathTemplate)
            : this()
        {
            _parent = Validation.ThrowIfNullOrReturn(groupBuilder, nameof(groupBuilder));
            this.Options = Validation.ThrowIfNullOrReturn(groupBuilder?.Options, nameof(groupBuilder.Options));

            _partialPathTemplate = Validation.ThrowIfNullWhiteSpaceOrReturn(partialPathTemplate, nameof(partialPathTemplate));
        }

        /// <inheritdoc cref="IGraphQLFieldBuilder.Options" />
        public virtual SchemaOptions Options { get; protected set; }

        /// <inheritdoc cref="IGraphQLFieldBuilder.Attributes" />
        public IList<Attribute> Attributes { get; }

        /// <inheritdoc cref="IGraphQLFieldBuilder.Template" />
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