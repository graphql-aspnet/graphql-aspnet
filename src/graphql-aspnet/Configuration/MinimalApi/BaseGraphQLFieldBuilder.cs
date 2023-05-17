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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Schemas.Structural;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// An abstract class containing all the common elements across minimal field builders and
    /// their supporting classes.
    /// </summary>
    internal abstract class BaseGraphQLFieldBuilder : IReadOnlyDictionary<string, object>
    {
        private Dictionary<string, object> _dataElements;
        private IGraphQLFieldGroupBuilder _parent;
        private string _partialPathTemplate;

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
        {
            _parent = null;
            this.Options = Validation.ThrowIfNullOrReturn(options, nameof(options));

            _partialPathTemplate = partialPathTemplate?.Trim() ?? string.Empty;
            _dataElements = new Dictionary<string, object>();
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
        {
            _parent = Validation.ThrowIfNullOrReturn(groupBuilder, nameof(groupBuilder));
            this.Options = Validation.ThrowIfNullOrReturn(groupBuilder?.Options, nameof(groupBuilder.Options));

            _partialPathTemplate = Validation.ThrowIfNullWhiteSpaceOrReturn(partialPathTemplate, nameof(partialPathTemplate));
            _dataElements = new Dictionary<string, object>();
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return _dataElements.ContainsKey(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out object value)
        {
            return _dataElements.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _dataElements.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public IEnumerable<string> Keys => _dataElements.Keys;

        /// <inheritdoc />
        public IEnumerable<object> Values => _dataElements.Values;

        /// <inheritdoc />
        public int Count => _dataElements.Count;

        /// <inheritdoc />
        public object this[string key] => _dataElements[key];

        /// <inheritdoc cref="IGraphQLFieldBuilder.Options" />
        public virtual SchemaOptions Options { get; protected set; }

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