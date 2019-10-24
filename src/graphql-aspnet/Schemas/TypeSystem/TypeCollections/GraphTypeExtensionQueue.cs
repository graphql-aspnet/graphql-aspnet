// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem.TypeCollections
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A collection of extension <see cref="IGraphField"/>s that are queued and waiting to be
    /// registered if/when their required master type is added to the primary collection.
    /// </summary>
    internal class GraphTypeExtensionQueue
    {
        private readonly ConcurrentDictionary<Type, HashSet<UnregisteredGraphField>> _unregisteredFieldExtensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphTypeExtensionQueue"/> class.
        /// </summary>
        public GraphTypeExtensionQueue()
        {
            _unregisteredFieldExtensions = new ConcurrentDictionary<Type, HashSet<UnregisteredGraphField>>();
        }

        /// <summary>
        /// Adds the field extension to the queue such that if a graph type associated with the supplied master type is added to the schema,
        /// then the new graph field is automatically added to the new graph type.
        /// </summary>
        /// <param name="masterType">The master type that, if added to the schema, will trigger the addition of this field to its associated <see cref="IGraphType"/>.</param>
        /// <param name="field">The field.</param>
        public void EnQueueField(Type masterType, IGraphField field)
        {
            if (!_unregisteredFieldExtensions.TryGetValue(masterType, out var fieldSet))
            {
                fieldSet = new HashSet<UnregisteredGraphField>(UnregisteredGraphField.Comparer);
                _unregisteredFieldExtensions.TryAdd(masterType, fieldSet);
            }

            var unregisteredField = new UnregisteredGraphField(field, masterType);
            fieldSet.Add(unregisteredField);
            this.FieldCount += 1;
        }

        /// <summary>
        /// Dequeues and returns the <see cref="IGraphField" /> that were queued for the given master type. Will return
        /// an empty list if no items are found.
        /// </summary>
        /// <param name="masterType">The type for which the fields should be dequeued.</param>
        /// <returns>IEnumerable&lt;IGraphField&gt;.</returns>
        public IEnumerable<IGraphField> DequeueFields(Type masterType)
        {
            if (_unregisteredFieldExtensions.TryRemove(masterType, out var queuedFields))
            {
                this.FieldCount -= queuedFields.Count;
                return queuedFields.Select(x => x.Field);
            }

            return Enumerable.Empty<IGraphField>();
        }

        /// <summary>
        /// Gets the count of queued <see cref="IGraphField"/>.
        /// </summary>
        /// <value>The unregistered field count.</value>
        public int FieldCount { get; private set; }
    }
}