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
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;

    /// <summary>
    /// An enumerator that walks a field selection set looking for the next
    /// executable field in its scope. This can be a directly attached field,
    /// or one that would be included via an inline fragment or named fragment spread.
    /// </summary>
    internal class ExecutableFieldSelectionSetEnumerator : IEnumerator<(IFieldDocumentPart DocPart, bool IsIncluded)>
    {
        // Rewrite to be an "include chain"
        // determine which includeable fields determine the includability of a field in a selection
        // set and carry a reference to each such that it can be evaluated in real time
        // when needed to determine if a field is or should be included


        private readonly bool _forceExclude;
        private readonly List<IDocumentPart> _partsToIterate;

        private int _index;
        private HashSet<INamedFragmentDocumentPart> _traversedFragments;
        private INamedFragmentDocumentPart _activeNamedFragment;
        private ExecutableFieldSelectionSetEnumerator _childEnumerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutableFieldSelectionSetEnumerator"/> class.
        /// </summary>
        /// <param name="selectionSet">The selection set to traverse.</param>
        public ExecutableFieldSelectionSetEnumerator(IFieldSelectionSetDocumentPart selectionSet)
            : this(selectionSet, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutableFieldSelectionSetEnumerator" /> class.
        /// </summary>
        /// <param name="selectionSet">The selection set.</param>
        /// <param name="forceExclusion">If set to true, all fields traversed will be automatically set to
        /// "not included" regardless of thier own inclusion status.</param>
        /// <param name="traversedFragments">The traversed fragments.</param>
        private ExecutableFieldSelectionSetEnumerator(
            IFieldSelectionSetDocumentPart selectionSet,
            bool forceExclusion = false,
            HashSet<INamedFragmentDocumentPart> traversedFragments = null)
        {
            // make static the children in this enumerator so that it doesnt accidently
            // change if the document is still being built as its being read
            if ((selectionSet?.Children?.Count ?? 0) > 0)
            {
                _partsToIterate = new List<IDocumentPart>(selectionSet.Children.Count);
                _partsToIterate.AddRange(selectionSet.Children);
            }

            _forceExclude = forceExclusion;
            _traversedFragments = traversedFragments ?? new HashSet<INamedFragmentDocumentPart>();
            _index = -1;
            _childEnumerator = null;
            _activeNamedFragment = null;
        }

        private bool MoveIndexToNextField()
        {
            if (_partsToIterate == null)
                return false;

            // then advance the pointer on this selection set
            _index++;
            while (_index < _partsToIterate.Count)
            {
                if (_partsToIterate[_index] is IFieldDocumentPart fd)
                {
                    // field document is valid for next item
                    return true;
                }
                else if (_partsToIterate[_index] is IInlineFragmentDocumentPart iif)
                {
                    _childEnumerator = new ExecutableFieldSelectionSetEnumerator(
                        iif.FieldSelectionSet,
                        _forceExclude || !iif.IsIncluded,
                        _traversedFragments);

                    if (_childEnumerator.MoveNext())
                        return true;

                    // no fields to traverse immediately release the child
                    // enumerator
                    _childEnumerator = null;
                }
                else if (_partsToIterate[_index] is IFragmentSpreadDocumentPart fs)
                {
                    if (fs.Fragment != null
                        && !_traversedFragments.Contains(fs.Fragment))
                    {
                        _traversedFragments.Add(fs.Fragment);
                        _activeNamedFragment = fs.Fragment;
                        _childEnumerator = new ExecutableFieldSelectionSetEnumerator(
                            fs.Fragment.FieldSelectionSet,
                            _forceExclude || !fs.IsIncluded,
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
        public (IFieldDocumentPart, bool) Current
        {
            get
            {
                if (_childEnumerator != null)
                    return _childEnumerator.Current;

                var fieldDocPart = _partsToIterate[_index] as IFieldDocumentPart;
                return (fieldDocPart, !_forceExclude && fieldDocPart.IsIncluded);
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