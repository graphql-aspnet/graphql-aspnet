// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.Document.Parts.SuppliedValues
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;

    /// <inheritdoc cref="IDocumentPartsCollection" />
    [DebuggerDisplay("Count = {Count}, Owner: {Owner.GetType().Name}")]
    internal class DocumentPartsCollection : IDocumentPartsCollection
    {
        // all the parts added to this collection in the order they
        // were added
        private List<IDocumentPart> _allParts;

        // all the parts, in the order they were added but seperated by
        // part type for easy searching and processing.
        private Dictionary<DocumentPartType, List<IDocumentPart>> _partsByType;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentPartsCollection"/> class.
        /// </summary>
        /// <param name="owner">The doc part that owns this collection.</param>
        public DocumentPartsCollection(IDocumentPart owner)
        {
            this.Owner = Validation.ThrowIfNullOrReturn(owner, nameof(owner));
            _partsByType = new Dictionary<DocumentPartType, List<IDocumentPart>>();
            _allParts = new List<IDocumentPart>();
        }

        public void Add(IDocumentPart documentPart)
        {
            var part = Validation.ThrowIfNullOrReturn(documentPart, nameof(documentPart));

            if (part.Parent != this.Owner)
            {
                throw new GraphExecutionException(
                    $"Document construction halted. An attempt was made to add a document " +
                    $"part to a collection owned by a different parent.");
            }

            _allParts.Add(part);

            if (!_partsByType.ContainsKey(part.PartType))
                _partsByType.Add(part.PartType, new List<IDocumentPart>());

            _partsByType[part.PartType].Add(part);

            var parent = part.Parent;
            var relativeDepth = 1;
            while (parent != null)
            {
                if (parent is IDecdendentDocumentPartSubscriber sub)
                    sub.OnDecendentPartAdded(part, relativeDepth);

                relativeDepth++;
                parent = parent.Parent;
            }
        }

        /// <inheritdoc />
        public IDocumentPart Owner { get; }

        /// <summary>
        /// Gets the <see cref="IDocumentPart"/> at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>IDocumentPart.</returns>
        public IDocumentPart this[int index] => _allParts[index];

        /// <inheritdoc />
        public IReadOnlyList<IDocumentPart> this[DocumentPartType partType]
        {
            get
            {
                if (_partsByType.ContainsKey(partType))
                    return _partsByType[partType];

                return new List<IDocumentPart>();
            }
        }

        /// <inheritdoc />
        public int Count => _allParts.Count;

        /// <inheritdoc />
        public IEnumerator<IDocumentPart> GetEnumerator()
        {
            return _allParts.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}