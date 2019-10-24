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
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;

    /// <summary>
    /// Helper method for displaying the ordered dictionary in the debugger.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class OrderedDictionaryDebugBrowser
    {
        private readonly List<KeyValuePair<object, object>> _data;
        private readonly List<object> _keys;
        private readonly List<object> _values;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionaryDebugBrowser"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        public OrderedDictionaryDebugBrowser(IOrderedDictionary dictionary)
        {
            _data = new List<KeyValuePair<object, object>>();
            _keys = new List<object>();
            _values = new List<object>();
            foreach (var key in dictionary.Keys)
            {
                _data.Add(new KeyValuePair<object, object>(key, dictionary[key]));
                _keys.Add(key);
                _values.Add(dictionary[key]);
            }
        }

        /// <summary>
        /// Gets the key value pairs in this dictionary.
        /// </summary>
        /// <value>The items.</value>
        public IReadOnlyList<KeyValuePair<object, object>> Items => _data;

        /// <summary>
        /// Gets the ordered keys in this dictionary.
        /// </summary>
        /// <value>The keys.</value>
        public IReadOnlyList<object> Keys => _keys;

        /// <summary>
        /// Gets the ordered values in this dictionary.
        /// </summary>
        /// <value>The values.</value>
        public IReadOnlyList<object> Values => _values;

        /// <summary>
        /// Gets the count of items in this dictionary.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _data.Count;
    }
}