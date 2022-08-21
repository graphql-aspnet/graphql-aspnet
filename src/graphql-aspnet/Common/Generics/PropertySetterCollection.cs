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
    public class PropertySetterCollection : IReadOnlyDictionary<string, PropertySetterInvoker>
    {
        private readonly Dictionary<string, PropertySetterInvoker> _setters;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertySetterCollection"/> class.
        /// </summary>
        public PropertySetterCollection()
        {
            _setters = new Dictionary<string, PropertySetterInvoker>();
        }

        /// <summary>
        /// Adds the property setter to the collection keyed on its property name.
        /// </summary>
        /// <param name="propInfo">The property information.</param>
        /// <param name="invoker">The invoker.</param>
        public void Add(PropertyInfo propInfo, PropertySetterInvoker invoker)
        {
            _setters.Add(propInfo.Name, invoker);
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return _setters.ContainsKey(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out PropertySetterInvoker value)
        {
            return _setters.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, PropertySetterInvoker>> GetEnumerator()
        {
            return _setters.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public int Count => _setters.Count;

        /// <inheritdoc />
        public PropertySetterInvoker this[string key] => _setters[key];

        /// <inheritdoc />
        public IEnumerable<string> Keys => _setters.Keys;

        /// <inheritdoc />
        public IEnumerable<PropertySetterInvoker> Values => _setters.Values;
    }
}