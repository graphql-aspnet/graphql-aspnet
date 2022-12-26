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
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts.Common;

    /// <summary>
    /// An enumerator that walks a field selection set looking for the next
    /// executable field in its scope. This can be a directly attached field,
    /// or one that would be included via an inline fragment or named fragment spread.
    /// </summary>
    internal class ExecutableFieldSelectionSetEnumerator : IEnumerator<IFieldDocumentPart>
    {
        private readonly bool _includedOnly;
        private readonly List<IDocumentPart> _partsToIterator;

        private int _index;
        private HashSet<INamedFragmentDocumentPart> _traversedFragments;
        private INamedFragmentDocumentPart _activeNamedFragment;
        private ExecutableFieldSelectionSetEnumerator _childEnumerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutableFieldSelectionSetEnumerator"/> class.
        /// </summary>
        /// <param name="selectionSet">The selection set.</param>
        /// <param name="includedOnly">if set to <c>true</c> only included parts are iterated on.</param>
        /// <param name="traversedFragments">The traversed fragments.</param>
        public ExecutableFieldSelectionSetEnumerator(
            IFieldSelectionSetDocumentPart selectionSet,
            bool includedOnly = false,
            HashSet<INamedFragmentDocumentPart> traversedFragments = null)
        {
            // make static the children in this enumerator so that it doesnt accidently
            // change if the document is still being built as its being read
            if ((selectionSet?.Children?.Count ?? 0) > 0)
            {
                _partsToIterator = new List<IDocumentPart>(selectionSet.Children.Count);
                _partsToIterator.AddRange(selectionSet.Children);
            }

            _includedOnly = includedOnly;
            _traversedFragments = traversedFragments ?? new HashSet<INamedFragmentDocumentPart>();
            _index = -1;
            _childEnumerator = null;
            _activeNamedFragment = null;
        }

        private bool MoveIndexToNextField()
        {
            if (_partsToIterator == null)
                return false;

            // then advance the pointer on this selection set
            _index++;
            while (_index < _partsToIterator.Count)
            {
                if (_partsToIterator[_index] is IFieldDocumentPart fd)
                {
                    if (!_includedOnly || fd.IsIncluded)
                        return true;
                }
                else if (_partsToIterator[_index] is IInlineFragmentDocumentPart iif)
                {
                    if (!_includedOnly || iif.IsIncluded)
                    {
                        _childEnumerator = new ExecutableFieldSelectionSetEnumerator(
                            iif.FieldSelectionSet,
                            _includedOnly,
                            _traversedFragments);

                        if (_childEnumerator.MoveNext())
                            return true;

                        // no fields to traverse immediately release the child
                        // enumerator
                        _childEnumerator = null;
                    }
                }
                else if (_partsToIterator[_index] is IFragmentSpreadDocumentPart fs)
                {
                    if (fs.Fragment != null
                        && !_traversedFragments.Contains(fs.Fragment)
                        && (!_includedOnly || fs.IsIncluded))
                    {
                        _traversedFragments.Add(fs.Fragment);
                        _activeNamedFragment = fs.Fragment;
                        _childEnumerator = new ExecutableFieldSelectionSetEnumerator(
                            fs.Fragment.FieldSelectionSet,
                            _includedOnly,
                            _traversedFragments);

                        if (_childEnumerator.MoveNext())
                            return true;

                        // no fields to traverse immediately release the child
                        // enumerator
                        _childEnumerator = null;
                    }
                }

                _index++;
            }

            return false;
        }

        /// <inheritdoc />
        public IFieldDocumentPart Current
        {
            get
            {
                if (_childEnumerator != null)
                    return _childEnumerator.Current;

                return _partsToIterator[_index] as IFieldDocumentPart;
            }
        }

        /// <inheritdoc />
        object IEnumerator.Current => this.Current;

        /// <inheritdoc />
        public void Dispose()
        {
            _childEnumerator?.Dispose();
            _childEnumerator = null;
            _traversedFragments = null;
            _activeNamedFragment = null;
        }

        /// <inheritdoc />
        public bool MoveNext()
        {
            if (_childEnumerator != null && _childEnumerator.MoveNext())
                return true;

            // if the child enumerator is/was traversing a named fragment
            // then its finished iterating, remove it from the traversal collection
            if (_activeNamedFragment != null)
            {
                _traversedFragments.Remove(_activeNamedFragment);
                _activeNamedFragment = null;
            }

            _childEnumerator = null;
            return this.MoveIndexToNextField();
        }

        /// <inheritdoc />
        public void Reset()
        {
            _childEnumerator = null;
            _activeNamedFragment = null;
            _index = -1;
        }
    }
}