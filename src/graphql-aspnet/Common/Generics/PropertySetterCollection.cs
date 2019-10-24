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

        /// <summary>
        /// Determines whether the read-only dictionary contains an element that has the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>true if the read-only dictionary contains an element that has the specified key; otherwise, false.</returns>
        public bool ContainsKey(string key)
        {
            return _setters.ContainsKey(key);
        }

        /// <summary>
        /// Gets the value that is associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns>true if the object that implements the <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2"></see> interface contains an element that has the specified key; otherwise, false.</returns>
        public bool TryGetValue(string key, out PropertySetterInvoker value)
        {
            return _setters.TryGetValue(key, out value);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<string, PropertySetterInvoker>> GetEnumerator()
        {
            return _setters.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _setters.Count;

        /// <summary>
        /// Gets the <see cref="PropertySetterInvoker"/> with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>PropertySetterInvoker.</returns>
        public PropertySetterInvoker this[string key] => _setters[key];

        /// <summary>
        /// Gets an enumerable collection that contains the keys in the read-only dictionary.
        /// </summary>
        /// <value>The keys.</value>
        public IEnumerable<string> Keys => _setters.Keys;

        /// <summary>
        /// Gets an enumerable collection that contains the values in the read-only dictionary.
        /// </summary>
        /// <value>The values.</value>
        public IEnumerable<PropertySetterInvoker> Values => _setters.Values;
    }
}