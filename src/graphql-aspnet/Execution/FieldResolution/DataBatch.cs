// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.FieldResolution
{
    using System.Collections;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// A data batch capable of issueing data in a manner consistant with what a field expects for resolved data.
    /// </summary>
    [DebuggerDisplay("Count = {_dictionary.Count}")]
    internal class DataBatch
    {
        private readonly GraphTypeExpression _fieldTypeExpression;
        private readonly IDictionary _dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataBatch" /> class.
        /// </summary>
        /// <param name="fieldTypeExpression">The type expression provided by teh field attempting to resolve the data.</param>
        /// <param name="dictionary">The dictionary.</param>
        public DataBatch(GraphTypeExpression fieldTypeExpression, IDictionary dictionary)
        {
            _fieldTypeExpression = fieldTypeExpression;
            _dictionary = dictionary;
        }

        /// <summary>
        /// Extracts and generates a valid return object for the given source item.
        /// </summary>
        /// <param name="sourceItem">The source item.</param>
        /// <returns>System.Object.</returns>
        public object RetrieveAssociatedItems(object sourceItem)
        {
            if (_dictionary == null)
                return null;

            var result = _dictionary.Contains(sourceItem) ? _dictionary[sourceItem] : null;
            if (result == null)
                return null;

            if (_fieldTypeExpression.IsListOfItems && !(result is IEnumerable))
                result = result.AsEnumerable();

            return result;
        }
    }
}