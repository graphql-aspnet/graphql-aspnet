// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.DocumentParts
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;

    using ExecutableFieldList = System.Collections.Generic.List<(GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts.IFieldDocumentPart DocPart, System.Collections.Generic.List<GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts.IIncludeableDocumentPart> InclusionGoverners)>;

    /// <summary>
    /// An execution set that manages the actual fields to be resolved within
    /// a given field selection set. This includes fields directly included as well as those
    /// that would be incorporated via a fragment spread or inline fragment.
    /// </summary>
    internal class ExecutableFieldSelectionSet : IExecutableFieldSelectionSet
    {
        private int _sequence;
        private int _lastBuiltSequence;

        // for any field that should be included
        // track a list of fields that will have a hand in determining its includability
        // for instance a field defined in a fragment is only included if:
        // 1) the field itself is included
        // 2) the fragment spread is included
        // 3) the field containing the spread is included
        // 4) etc...
        //
        // The include status of each of those governing parts
        // will change as directives are executed so keep a reference to the parts
        // so includability can be deteremined quickly on the fly when needed
        private ExecutableFieldList _cachedExecutableFields;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutableFieldSelectionSet"/> class.
        /// </summary>
        /// <param name="owner">The master field selection set on which this instance
        /// is based.</param>
        public ExecutableFieldSelectionSet(IFieldSelectionSetDocumentPart owner)
        {
            this.Owner = Validation.ThrowIfNullOrReturn(owner, nameof(owner));
            _cachedExecutableFields = null;
            _lastBuiltSequence = -1;
            _sequence = 0;
        }

        /// <summary>
        /// Instructs this execution set to update its snapshot, clearing any
        /// cached fields and rebuilding from its owner.
        /// </summary>
        internal void ResetFieldSelectionSet()
        {
            // don't immediately rebuild the selection set
            // its likely to change more as a document is built
            // instead just mark the current snapshot (if there is one)
            // as being invalid and only rebuild on the next time the fields are needed
            _sequence++;
        }

        private void EnsureCurrentSnapshot()
        {
            if (_lastBuiltSequence == _sequence && _cachedExecutableFields != null)
                return;

            var setBuilder = new ExecutableFieldSelectionSetBuilder(this.Owner);
            _cachedExecutableFields = setBuilder.CreateFieldList();
            _lastBuiltSequence = _sequence;
        }

        /// <inheritdoc />
        public IEnumerator<IFieldDocumentPart> GetEnumerator()
        {
            this.EnsureCurrentSnapshot();
            return _cachedExecutableFields.Select(x => x.DocPart).GetEnumerator();
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
                this.EnsureCurrentSnapshot();
                return _cachedExecutableFields[index].DocPart;
            }
        }

        /// <inheritdoc />
        public IEnumerable<IFieldDocumentPart> IncludedOnly
        {
            get
            {
                this.EnsureCurrentSnapshot();
                for (var i = 0; i < _cachedExecutableFields.Count; i++)
                {
                    bool shouldInclude = true;
                    for (var j = 0; j < _cachedExecutableFields[i].InclusionGoverners.Count; j++)
                    {
                        if (!_cachedExecutableFields[i].InclusionGoverners[j].IsIncluded)
                        {
                            shouldInclude = false;
                            break;
                        }
                    }

                    if (shouldInclude)
                        yield return _cachedExecutableFields[i].DocPart;
                }
            }
        }

        /// <inheritdoc />
        public IFieldSelectionSetDocumentPart Owner { get; }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                this.EnsureCurrentSnapshot();
                return _cachedExecutableFields.Count;
            }
        }
    }
}