// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Structural
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A collection of enum values on a single enum graph type.
    /// </summary>
    public class EnumValueCollection : IEnumValueCollection
    {
        private Dictionary<string, IEnumValue> _enumValuesByName;
        private Dictionary<string, IEnumValue> _enumValuesByInternalLabel;
        private IEnumGraphType _graphType;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumValueCollection" /> class.
        /// </summary>
        /// <param name="graphType">the graph type that owns this collection.</param>
        public EnumValueCollection(IEnumGraphType graphType)
        {
            _graphType = Validation.ThrowIfNullOrReturn(graphType, nameof(graphType));
            _enumValuesByName = new Dictionary<string, IEnumValue>(StringComparer.OrdinalIgnoreCase);
            _enumValuesByInternalLabel = new Dictionary<string, IEnumValue>();
        }

        /// <summary>
        /// Adds the specified value to the collection.
        /// </summary>
        /// <param name="value">The value to add.</param>
        public void Add(IEnumValue value)
        {
            Validation.ThrowIfNull(value, nameof(value));
            if (_enumValuesByName.ContainsKey(value.Name))
            {
                throw new GraphTypeDeclarationException(
                    $"Duplciate enum value detected. The graph type '{_graphType.Name}' already " +
                    $"declares an enum value named '{value.Name}'.");
            }

            _enumValuesByName.Add(value.Name, value);
            _enumValuesByInternalLabel.Add(value.InternalLabel, value);
        }

        /// <summary>
        /// Removes the specified name if it exists. When found, the removed item is returned.
        /// </summary>
        /// <param name="name">The name of the item to remove.</param>
        /// <returns>IEnumValue.</returns>
        public IEnumValue Remove(string name)
        {
            if (_enumValuesByName.ContainsKey(name))
            {
                var removedOption = _enumValuesByName[name];
                _enumValuesByName.Remove(name);

                if (_enumValuesByInternalLabel.ContainsKey(removedOption.InternalLabel))
                    _enumValuesByInternalLabel.Remove(removedOption.InternalLabel);

                return removedOption;
            }

            return null;
        }

        /// <inheritdoc />
        public IEnumValue FindByEnumValue(object enumValue)
        {
            if (enumValue == null)
                return null;

            if (_graphType.ValidateObject(enumValue))
            {
                var internalLabel = Enum.GetName(_graphType.ObjectType, enumValue);
                if (_enumValuesByInternalLabel.ContainsKey(internalLabel))
                    return _enumValuesByInternalLabel[internalLabel];
            }

            return null;
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out IEnumValue value)
        {
            return _enumValuesByName.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public bool ContainsKey(string key) => _enumValuesByName.ContainsKey(key);

        /// <inheritdoc />
        public IEnumValue this[string key] => _enumValuesByName[key];

        /// <inheritdoc />
        public IEnumerable<string> Keys => _enumValuesByName.Keys;

        /// <inheritdoc />
        public IEnumerable<IEnumValue> Values => _enumValuesByName.Values;

        /// <inheritdoc />
        public int Count => _enumValuesByName.Count;

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, IEnumValue>> GetEnumerator() => _enumValuesByName.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}