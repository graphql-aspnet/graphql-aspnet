// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.PlanGeneration.Document.Parts
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;

    /// <summary>
    /// A collection of input arguments defined in a user's query document for a single field or directive.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class QueryInputArgumentCollection : IQueryInputArgumentCollection
    {
        private readonly Dictionary<string, QueryInputArgument> _arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryInputArgumentCollection" /> class.
        /// </summary>
        public QueryInputArgumentCollection()
        {
            _arguments = new Dictionary<string, QueryInputArgument>();
        }

        /// <summary>
        /// Determines whether the specified input name exists on this collection.
        /// </summary>
        /// <param name="inputName">Name of the input argument.</param>
        /// <returns><c>true</c> if this instance contains the key; otherwise, <c>false</c>.</returns>
        public bool ContainsKey(ReadOnlyMemory<char> inputName)
        {
            return this.ContainsKey(inputName.ToString());
        }

        /// <summary>
        /// Adds the input argument to the collection.
        /// </summary>
        /// <param name="argument">The argument.</param>
        public void AddArgument(QueryInputArgument argument)
        {
            Validation.ThrowIfNull(argument, nameof(argument));
            _arguments.Add(argument.Name, argument);
        }

        /// <summary>
        /// Inspects the collection of arguments for an argument with the provided input name.
        /// Returns the argument if its found otherwise null.
        /// </summary>
        /// <param name="inputName">The name of the argument to look for.</param>
        /// <returns>The found argument or null.</returns>
        public QueryInputArgument FindArgumentByName(ReadOnlyMemory<char> inputName)
        {
            return this.FindArgumentByName(inputName.ToString());
        }

        /// <summary>
        /// Inspects the collection of arguments for an argument with the provided input name.
        /// Returns the argument if its found otherwise null.
        /// </summary>
        /// <param name="inputName">The name of the argument to look for.</param>
        /// <returns>The found argument or null.</returns>
        public QueryInputArgument FindArgumentByName(string inputName)
        {
            if (this.ContainsKey(inputName))
                return this[inputName];

            return null;
        }

        /// <summary>
        /// Determines whether the read-only dictionary contains an element that has the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>true if the read-only dictionary contains an element that has the specified key; otherwise, false.</returns>
        public bool ContainsKey(string key)
        {
            return _arguments.ContainsKey(key);
        }

        /// <summary>
        /// Gets the value that is associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns>true if the object that implements the <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2"></see> interface contains an element that has the specified key; otherwise, false.</returns>
        public bool TryGetValue(string key, out QueryInputArgument value)
        {
            return _arguments.TryGetValue(key, out value);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<string, QueryInputArgument>> GetEnumerator()
        {
            return _arguments.GetEnumerator();
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
        public int Count => _arguments.Count;

        /// <summary>
        /// Gets the <see cref="QueryInputArgument"/> with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>QueryInputArgument.</returns>
        public QueryInputArgument this[string key] => _arguments[key];

        /// <summary>
        /// Gets an enumerable collection that contains the keys in the read-only dictionary.
        /// </summary>
        /// <value>The keys.</value>
        public IEnumerable<string> Keys => _arguments.Keys;

        /// <summary>
        /// Gets an enumerable collection that contains the values in the read-only dictionary.
        /// </summary>
        /// <value>The values.</value>
        public IEnumerable<QueryInputArgument> Values => _arguments.Values;
    }
}