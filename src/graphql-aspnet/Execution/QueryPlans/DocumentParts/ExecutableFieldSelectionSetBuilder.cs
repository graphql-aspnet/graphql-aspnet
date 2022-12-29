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
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;

    /// <summary>
    /// An builder that walks a field selection set looking for the set of fields
    /// that should be included. These fields can be a directly attached field,
    /// or one that would be included via an inline fragment or named fragment spread.
    /// </summary>
    internal static class ExecutableFieldSelectionSetBuilder
    {
        /// <summary>
        /// Creates a field list for the selection set by walking the fields, inline fragments and
        /// named fragments to determine every includable field.
        /// </summary>
        /// <param name="selectionSet">The selection set to flatten.</param>
        /// <param name="onlyIncludedFields">if set to <c>true</c> only fields marked as
        /// includable will be gathered.</param>
        /// <returns>List&lt;IFieldDocumentPart&gt;.</returns>
        public static List<IFieldDocumentPart> FlattenFieldList(
            IFieldSelectionSetDocumentPart selectionSet,
            bool onlyIncludedFields)
        {
            var fieldList = new List<IFieldDocumentPart>(selectionSet?.Children?.Count ?? 0);
            FillFieldList(selectionSet, fieldList, !onlyIncludedFields);

            return fieldList;
        }

        private static void FillFieldList(
            IFieldSelectionSetDocumentPart selectionSet,
            List<IFieldDocumentPart> foundFields,
            bool includeAll,
            HashSet<INamedFragmentDocumentPart> traversedFragments = null)
        {
            if (selectionSet?.Children == null || selectionSet.Children.Count == 0)
                return;

            for (var i = 0; i < selectionSet.Children.Count; i++)
            {
                var childPart = selectionSet.Children[i];
                if (childPart is IFieldDocumentPart fd && (fd.IsIncluded || includeAll))
                {
                    foundFields.Add(fd);
                    continue;
                }

                if (childPart is IInlineFragmentDocumentPart iif && (iif.IsIncluded || includeAll))
                {
                    FillFieldList(
                        iif.FieldSelectionSet,
                        foundFields,
                        includeAll,
                        traversedFragments);
                    continue;
                }

                if (childPart is IFragmentSpreadDocumentPart fs && (fs.IsIncluded || includeAll))
                {
                    // a named fragment can't be spread more than once
                    // nor are fragment spread loops allowed
                    // if this happens its an error that will be caught in validation
                    // but we need to prevent an infinite loop here
                    if (fs.Fragment != null)
                    {
                        traversedFragments = traversedFragments ?? new HashSet<INamedFragmentDocumentPart>();
                        if (!traversedFragments.Contains(fs.Fragment))
                        {
                            traversedFragments.Add(fs.Fragment);
                            FillFieldList(
                                fs.Fragment.FieldSelectionSet,
                                foundFields,
                                includeAll,
                                traversedFragments);
                        }
                    }

                    continue;
                }
            }
        }
    }
}