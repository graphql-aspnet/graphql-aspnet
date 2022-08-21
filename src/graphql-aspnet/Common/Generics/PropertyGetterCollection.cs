// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Generics
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// A collection of compiled expression trees representing the necessary statements to call
    /// the setters of a given object and apply a value. This is used instead of reflective calls
    /// for speed and performance at the expensive of an initial setup cost.
    /// </summary>
    public class PropertyGetterCollection : IReadOnlyDictionary<string, PropertyGetterInvoker>
    {
        private readonly Dictionary<string, PropertyGetterInvoker> _getters;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGetterCollection"/> class.
        /// </summary>
        public PropertyGetterCollection()
        {
            _getters = new Dictionary<string, PropertyGetterInvoker>();
        }

        /// <summary>
        /// Adds the property setter to the collection keyed on its property name.
        /// </summary>
        /// <param name="propInfo">The property information.</param>
        /// <param name="invoker">The invoker.</param>
        public void Add(PropertyInfo propInfo, PropertyGetterInvoker invoker)
        {
            _getters.Add(propInfo.Name, invoker);
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return _getters.ContainsKey(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out PropertyGetterInvoker value)
        {
            return _getters.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, PropertyGetterInvoker>> GetEnumerator()
        {
            return _getters.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public int Count => _getters.Count;

        /// <inheritdoc />
        public PropertyGetterInvoker this[string key] => _getters[key];

        /// <inheritdoc />
        public IEnumerable<string> Keys => _getters.Keys;

        /// <inheritdoc />
        public IEnumerable<PropertyGetterInvoker> Values => _getters.Values;
    }
}