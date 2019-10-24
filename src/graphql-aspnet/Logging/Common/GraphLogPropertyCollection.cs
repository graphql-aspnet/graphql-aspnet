// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.Common
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.Logging;

    /// <summary>
    /// A collection of keyvalue pairs used in log entries and their children.
    /// </summary>
    [DebuggerDisplay("Count = {Properties.Count}")]
    public class GraphLogPropertyCollection : IGraphLogPropertyCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphLogPropertyCollection"/> class.
        /// </summary>
        public GraphLogPropertyCollection()
        {
            this.Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// Retrieves a single property from the collection or null if not found.
        /// </summary>
        /// <param name="key">The key of the property.</param>
        /// <returns>System.Object.</returns>
        public object RetrieveProperty(string key)
        {
            return this.Properties.ContainsKey(key) ? this.Properties[key] : null;
        }

        /// <summary>
        /// Retrieves a single property from the collection or null if not found. If the
        /// found object cannot be explictly cast to the given type an exception will be thrown.
        /// </summary>
        /// <typeparam name="TType">The type to cast the found property value to.</typeparam>
        /// <param name="key">The key of the property.</param>
        /// <returns>System.Object.</returns>
        public TType RetrieveProperty<TType>(string key)
        {
            var obj = this.RetrieveProperty(key);
            return obj == null ? default : (TType)obj;
        }

        /// <summary>
        /// Adds the given key/value pair to the collection. This method will
        /// automatically suffix any provided key values to prevent a conflict.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The key under which the value was inserted.</returns>
        public string AddProperty(string key, object value)
        {
            var keyToUse = key;
            var i = 1;
            while (this.Properties.ContainsKey(keyToUse))
                keyToUse = $"{key}_{i++}";

            this.Properties.Add(keyToUse, value);
            return keyToUse;
        }

        /// <summary>
        /// Adds the property if it doesnt exist otherwise updates the existing value.
        /// </summary>
        /// <typeparam name="T">The type to return the property as.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>The typed property value.</returns>
        protected T GetProperty<T>(string key)
        {
            return this.RetrieveProperty<T>(key);
        }

        /// <summary>
        /// Adds the property if it doesnt exist otherwise updates the existing value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        protected void SetProperty(string key, object value)
        {
            this.Properties[key] = value;
        }

        /// <summary>
        /// Determines whether this property collection contains a log property with the given
        /// key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the key is found; otherwise, <c>false</c>.</returns>
        public bool ContainsKey(string key)
        {
            return this.Properties.ContainsKey(key);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this.Properties.GetEnumerator();
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
        /// Flattens the property list of this collection. If any entries are they themselves
        /// <see cref="IGraphLogPropertyCollection" />; those keys are unnested and prefixed
        /// with the key of this entry in the parent collection.
        /// </summary>
        /// <returns>IEnumerable&lt;KeyValuePair&lt;System.String, System.Object&gt;&gt;.</returns>
        public IEnumerable<KeyValuePair<string, object>> FlattenProperties()
        {
            foreach (var kvp in this)
            {
                if (kvp.Value is IGraphLogPropertyCollection childCollection)
                {
                    var childItems = childCollection.FlattenProperties();
                    foreach (var child in childItems)
                    {
                        yield return new KeyValuePair<string, object>($"{kvp.Key}_{child.Key}", child.Value);
                    }
                }
                else
                {
                    yield return kvp;
                }
            }
        }

        /// <summary>
        /// Gets the collection of key/value pairs managed by this instance.
        /// </summary>
        /// <value>The properties.</value>
        protected Dictionary<string, object> Properties { get; }

        /// <summary>
        /// Gets or sets the property value with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>the propert value.</returns>
        public object this[string key]
        {
            get
            {
                return this.Properties[key];
            }

            set
            {
                this.Properties[key] = value;
            }
        }
    }
}