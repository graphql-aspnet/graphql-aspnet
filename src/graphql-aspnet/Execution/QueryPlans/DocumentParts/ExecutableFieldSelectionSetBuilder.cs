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
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using ExecutableFieldList = System.Collections.Generic.List<(GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts.IFieldDocumentPart DocPart, System.Collections.Generic.List<GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts.IIncludeableDocumentPart> InclusionGoverners)>;

    /// <summary>
    /// An builder that walks a field selection set looking for the set of fields
    /// that should be included. These fields can be a directly attached field,
    /// or one that would be included via an inline fragment or named fragment spread.
    /// </summary>
    internal class ExecutableFieldSelectionSetBuilder
    {
        private readonly IFieldSelectionSetDocumentPart _primarySelectionSet;
        private HashSet<INamedFragmentDocumentPart> _traversedFragments;
        private ExecutableFieldList _fieldList;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutableFieldSelectionSetBuilder" /> class.
        /// </summary>
        /// <param name="selectionSet">The selection set to generate a field list for.</param>
        public ExecutableFieldSelectionSetBuilder(IFieldSelectionSetDocumentPart selectionSet)
        {
            _primarySelectionSet = Validation.ThrowIfNullOrReturn(selectionSet, nameof(selectionSet));
            _traversedFragments = new HashSet<INamedFragmentDocumentPart>();
        }

        /// <summary>
        /// Creates the field list for the selection set. Once generated a cached list is returned, the list
        /// is never regenerated.
        /// </summary>
        /// <returns>ExecutableFieldList.</returns>
        public ExecutableFieldList CreateFieldList()
        {
            if (_fieldList != null)
                return _fieldList;

            _fieldList = new ExecutableFieldList(_primarySelectionSet.Children.Count);
            var governingParts = new List<IIncludeableDocumentPart>(8);

            this.FillFieldList(_primarySelectionSet, governingParts);

            return _fieldList;
        }

        private void FillFieldList(IFieldSelectionSetDocumentPart selectionSet, List<IIncludeableDocumentPart> governingParts)
        {
            if (selectionSet == null || selectionSet.Children.Count == 0)
                return;

            for (var i = 0; i < selectionSet.Children.Count; i++)
            {
                var childPart = selectionSet.Children[i];
                if (childPart is IFieldDocumentPart fd)
                {
                    governingParts.Add(fd);
                    _fieldList.Add((fd, new List<IIncludeableDocumentPart>(governingParts)));
                    governingParts.RemoveAt(governingParts.Count - 1);
                    continue;
                }

                if (childPart is IInlineFragmentDocumentPart iif)
                {
                    governingParts.Add(iif);
                    this.FillFieldList(iif.FieldSelectionSet, new List<IIncludeableDocumentPart>(governingParts));

                    governingParts.RemoveAt(governingParts.Count - 1);
                    continue;
                }

                if (childPart is IFragmentSpreadDocumentPart fs)
                {
                    // a named fragment can't be spread more than once
                    // nor are fragment spread loops allowed
                    // if this happens its an error that will be caught in validation
                    // but we need to prevent an infinite loop here
                    if (fs.Fragment != null && !_traversedFragments.Contains(fs.Fragment))
                    {
                        _traversedFragments.Add(fs.Fragment);

                        governingParts.Add(fs);
                        this.FillFieldList(fs.Fragment.FieldSelectionSet, new List<IIncludeableDocumentPart>(governingParts));

                        governingParts.RemoveAt(governingParts.Count - 1);
                    }

                    continue;
                }
            }
        }
    }
}