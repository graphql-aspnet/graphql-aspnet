// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;

    /// <summary>
    /// An execution set that manages the actual fields to be resolved within
    /// a given field selection set. This includes fields directly included as well as those
    /// that would be incorporated via a fragment spread or inline fragment.
    /// </summary>
    internal class ExecutionFieldSet : IExecutableFieldSelectionSet
    {
        private const int INITIAL_CAPACITY = 10;

        // A collection of know fields per alias. In a valid document each item is
        // a list of one. Used primarily for rule validation and failure reporting.
        private Dictionary<string, List<IFieldDocumentPart>> _fieldsByAlias;

        // tracks the order of the direct children of the owner of this
        // instance.  Used to keep relative order of the asymetrical growing list of fields
        // added to inline frags and frag spreads as the document is parsed.
        private List<IDocumentPart> _firstOrderParts;

        // for all spreads and inline fragments that may exist in _orderedParts, this dictionary
        // tracks the fields within that are part of the execution set.
        private Dictionary<IDocumentPart, List<IFieldDocumentPart>> _fieldsByPart;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionFieldSet"/> class.
        /// </summary>
        /// <param name="owner">The master field selection set on which this instance
        /// is based.</param>
        public ExecutionFieldSet(IFieldSelectionSetDocumentPart owner)
        {
            this.Owner = Validation.ThrowIfNullOrReturn(owner, nameof(owner));
            this.Owner.Children.ChildPartAdded += (o, e) =>
            {
                if (e.RelativeDepth == 1 && e.TargetDocumentPart != null)
                    this.OnFirstOrderChildPartAdded(e.TargetDocumentPart);
            };

            _firstOrderParts = new List<IDocumentPart>(INITIAL_CAPACITY);
            _fieldsByAlias = new Dictionary<string, List<IFieldDocumentPart>>();
            _fieldsByPart = new Dictionary<IDocumentPart, List<IFieldDocumentPart>>();
        }

        /// <summary>
        /// A method called when a level one child is added to the <see cref="Owner"/>
        /// selection set.
        /// </summary>
        private void OnFirstOrderChildPartAdded(IDocumentPart docPart)
        {
            switch (docPart)
            {
                case IFieldDocumentPart fdp:
                    _firstOrderParts.Add(fdp);
                    break;

                case IFragmentSpreadDocumentPart fsdp:
                    _firstOrderParts.Add(fsdp);
                    this.WatchFragmentSpread(fsdp);
                    break;

                case IInlineFragmentDocumentPart iif:
                    _firstOrderParts.Add(iif);
                    this.WatchInlineFragment(iif);
                    break;
            }
        }

        /// <summary>
        /// A method called when a child
        /// </summary>
        /// <param name="docPart">The document part.</param>
        private void OnSecondOrderChildPartAdded(IDocumentPart docPart)
        {

        }

        private void WatchInlineFragment(IInlineFragmentDocumentPart iif)
        {
        }

        private void WatchFragmentSpread(IFragmentSpreadDocumentPart fsdp)
        {
        }

        /// <inheritdoc />
        public IReadOnlyList<IFieldDocumentPart> FilterByAlias(ReadOnlyMemory<char> alias)
        {
            var aliasText = alias.ToString();
            if (_fieldsByAlias.ContainsKey(aliasText))
                return _fieldsByAlias[aliasText];

            return new List<IFieldDocumentPart>(0);
        }

        /// <inheritdoc />
        public IEnumerator<IFieldDocumentPart> GetEnumerator()
        {
            return null;
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public IFieldDocumentPart this[int index]
        {
            get
            {
                return null;
            }
        }

        /// <inheritdoc />
        public IEnumerable<IFieldDocumentPart> IncludedOnly
        {
            get
            {
                foreach (var child in _fieldsByPart.Values)
                {
                    if (child is IFieldDocumentPart fd)
                    {
                        if (fd.IsIncluded)
                            yield return fd;
                    }
                    else if (child is IFragmentSpreadDocumentPart fsdp && fsdp.IsIncluded)
                    {
                        if (_fieldsByPart.ContainsKey(fsdp))
                        {
                            foreach (var spreadChild in _fieldsByPart[fsdp].Where(x => x.IsIncluded))
                                yield return spreadChild;
                        }
                    }
                    else if (child is IInlineFragmentDocumentPart iif && iif.IsIncluded)
                    {
                        if (_fieldsByPart.ContainsKey(iif))
                        {
                            foreach (var spreadChild in _fieldsByPart[iif].Where(x => x.IsIncluded))
                                yield return spreadChild;
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public int Count => _orderedParts.Count
                + _fieldsByPart.Sum(x => x.Value.Count)
                - _fieldsByPart.Count; // subtract out those items from _ordreedParts that are not fields

        /// <inheritdoc />
        public IFieldSelectionSetDocumentPart Owner { get; }
    }
}