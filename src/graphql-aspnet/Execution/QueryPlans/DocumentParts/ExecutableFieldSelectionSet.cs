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

    /// <summary>
    /// An execution set that manages the actual fields to be resolved within
    /// a given field selection set. This includes fields directly included as well as those
    /// that would be incorporated via a fragment spread or inline fragment.
    /// </summary>
    internal class ExecutableFieldSelectionSet : IExecutableFieldSelectionSet
    {
        private int _sequence;
        private int _lastBuiltSequence;
        private List<IFieldDocumentPart> _cachedExecutableFields;

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
        internal void UpdateSnapshot()
        {
            _sequence++;
        }

        private void EnsureCurrentSnapshot()
        {
            if (_lastBuiltSequence == _sequence && _cachedExecutableFields != null)
                return;

            var newList = new List<IFieldDocumentPart>((_cachedExecutableFields?.Count ?? 8) * 2);

            var iterator = new ExecutableFieldSelectionSetEnumerator(this.Owner);
            while (iterator.MoveNext())
                newList.Add(iterator.Current);

            _lastBuiltSequence = _sequence;
            _cachedExecutableFields = newList;
        }

        /// <inheritdoc />
        public IEnumerator<IFieldDocumentPart> GetEnumerator()
        {
            this.EnsureCurrentSnapshot();
            return _cachedExecutableFields.GetEnumerator();
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
                return _cachedExecutableFields[index];
            }
        }

        /// <inheritdoc />
        public IEnumerable<IFieldDocumentPart> IncludedOnly
        {
            get
            {
                this.EnsureCurrentSnapshot();
                return _cachedExecutableFields.Where(x => x.IsIncluded);
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