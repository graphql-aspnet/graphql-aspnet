// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using System.Collections;
    using System.Collections.Generic;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A collection of objects supplied to a pipeline that can act as an input object for
    /// a <see cref="GraphFieldDataSource"/>.
    /// </summary>
    public class DefaultFieldSourceCollection : IEnumerable<KeyValuePair<GraphFieldPath, object>>
    {
        private readonly Dictionary<GraphFieldPath, object> _actionSources;
        private readonly GraphFieldTemplateSource _sourceTemplateTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultFieldSourceCollection"/> class.
        /// </summary>
        /// <param name="sourcableTemplateTypes">The set of flags indicating which types of <see cref="IGraphField"/>
        /// are allowed to define objects in this collection.</param>
        public DefaultFieldSourceCollection(GraphFieldTemplateSource sourcableTemplateTypes = GraphFieldTemplateSource.Action)
        {
            _actionSources = new Dictionary<GraphFieldPath, object>();
            _sourceTemplateTypes = sourcableTemplateTypes;
        }

        /// <summary>
        /// Attempts to retrieve a default source object for the given field.
        /// </summary>
        /// <param name="field">The expected data type of the field .</param>
        /// <param name="result">The result object populated with the result.</param>
        /// <returns><c>true</c> a source entry is found, <c>false</c> otherwise.</returns>
        public bool TryRetrieveSource(IGraphField field, out object result)
        {
            result = null;
            if (field == null || !_actionSources.ContainsKey(field.Route))
                return false;

            result = _actionSources[field.Route];
            return true;
        }

        /// <summary>
        /// Adds a default source for a controller action that can be used as an input when no other source is available.
        /// </summary>
        /// <param name="field">The field representing the controller action.</param>
        /// <param name="sourceData">The source data to store with this context.</param>
        public void AddSource(IGraphField field, object sourceData)
        {
            if (field != null && _sourceTemplateTypes.HasFlag(field.FieldSource))
            {
                lock (_actionSources)
                {
                    if (_actionSources.ContainsKey(field.Route))
                        _actionSources[field.Route] = sourceData;
                    else
                        _actionSources.Add(field.Route, sourceData);
                }
            }
        }

        /// <summary>
        /// Determines whether the specified field has an item defined in this collection.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns><c>true</c> if the specified field has a defined value; otherwise, <c>false</c>.</returns>
        public bool ContainsKey(IGraphField field)
        {
            return field?.Route != null && _actionSources.ContainsKey(field.Route);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<GraphFieldPath, object>> GetEnumerator()
        {
            return _actionSources.GetEnumerator();
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
        /// Gets the number of fields defined in the collection.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _actionSources.Count;
    }
}