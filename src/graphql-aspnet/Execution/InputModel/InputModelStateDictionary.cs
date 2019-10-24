// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.InputModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// A dictionary describing the model state for a given graph call to a graph controller.
    /// </summary>
    [DebuggerDisplay("Count = {Count}, IsValid = {IsValid}")]
    public class InputModelStateDictionary : IReadOnlyDictionary<string, InputModelStateEntry>
    {
        private readonly Dictionary<string, InputModelStateEntry> _entries = new Dictionary<string, InputModelStateEntry>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="InputModelStateDictionary" /> class.
        /// </summary>
        public InputModelStateDictionary()
        {
        }

        /// <summary>
        /// Adds the specified entry to the dictionary.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public void Add(InputModelStateEntry entry)
        {
            _entries.Add(entry.Name, entry);
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _entries.Count;

        /// <summary>
        /// Determines whether the read-only dictionary contains an element that has the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>true if the read-only dictionary contains an element that has the specified key; otherwise, false.</returns>
        public bool ContainsKey(string key)
        {
            return _entries.ContainsKey(key);
        }

        /// <summary>
        /// Gets the value that is associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns>true if the object that implements the <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2"></see> interface contains an element that has the specified key; otherwise, false.</returns>
        public bool TryGetValue(string key, out InputModelStateEntry value)
        {
            return _entries.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets the <see cref="InputModelStateEntry"/> using its declared, case senseitive name in the C# method.
        /// </summary>
        /// <param name="key">The key of the model item.</param>
        /// <returns>GraphModelStateEntry.</returns>
        public InputModelStateEntry this[string key] => _entries[key];

        /// <summary>
        /// Gets an enumerable collection that contains the keys in the read-only dictionary.
        /// </summary>
        /// <value>The keys.</value>
        public IEnumerable<string> Keys => _entries.Keys;

        /// <summary>
        /// Gets an enumerable collection that contains the values in the read-only dictionary.
        /// </summary>
        /// <value>The values.</value>
        public IEnumerable<InputModelStateEntry> Values => _entries.Values;

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<string, InputModelStateEntry>> GetEnumerator()
        {
            return _entries.GetEnumerator();
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
        /// Gets a value indicating whether the collective model state entries are valid according to the validation rules executed for each item.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid => !_entries.Any() || _entries.Values.All(x => x.ValidationState != InputModelValidationState.Invalid);
    }
}