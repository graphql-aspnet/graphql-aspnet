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
        private int _currentSequence;
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
        private List<IFieldDocumentPart> _allFields;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutableFieldSelectionSet"/> class.
        /// </summary>
        /// <param name="owner">The master field selection set on which this instance
        /// is based.</param>
        public ExecutableFieldSelectionSet(IFieldSelectionSetDocumentPart owner)
        {
            this.Owner = Validation.ThrowIfNullOrReturn(owner, nameof(owner));
            _allFields = null;
            _lastBuiltSequence = -1;
            _currentSequence = 0;
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
            _currentSequence++;
        }

        private void EnsureCurrentSnapshot()
        {
            if (_lastBuiltSequence == _currentSequence && _allFields != null)
                return;

            _allFields = ExecutableFieldSelectionSetBuilder.FlattenFieldList(this.Owner, false);
            _lastBuiltSequence = _currentSequence;
        }

        /// <inheritdoc />
        public IEnumerator<IFieldDocumentPart> GetEnumerator()
        {
            this.EnsureCurrentSnapshot();
            return _allFields.GetEnumerator();
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
                return _allFields[index];
            }
        }

        /// <inheritdoc />
        public IEnumerable<IFieldDocumentPart> IncludedOnly
        {
            get
            {
                // those fields that are included must always be deteremined in real time
                // directives executed against various parts of a document can change the included fields
                // either directly or via a spread or inline fragment. Its currently impossible to know if a set of
                // included fields should be recomuted because there is no relationships between a named fragment
                // and where its spread
                return ExecutableFieldSelectionSetBuilder.FlattenFieldList(this.Owner, true);
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
                return _allFields.Count;
            }
        }
    }
}