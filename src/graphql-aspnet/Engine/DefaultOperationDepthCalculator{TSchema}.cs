// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// Default implementation of the operation depth calculator.
    /// </summary>
    /// <typeparam name="TSchema">The schema this calculator operates for.</typeparam>
    public class DefaultOperationDepthCalculator<TSchema> : IQueryOperationDepthCalculator<TSchema>
        where TSchema : class, ISchema
    {
        /// <inheritdoc />
        public int Calculate(IOperationDocumentPart operation)
        {
            return this.ComputeMaxDepth(
                operation.FieldSelectionSet,
                new HashSet<INamedFragmentDocumentPart>());
        }

        private int ComputeMaxDepth(
            IFieldSelectionSetDocumentPart selectionSet,
            HashSet<INamedFragmentDocumentPart> walkedNamedFragments)
        {
            int maxDepth = 0;
            if (selectionSet != null)
            {
                for (var i = 0; i < selectionSet.Children.Count; i++)
                {
                    var child = selectionSet.Children[i];
                    if (child is IFieldDocumentPart fd)
                    {
                        var depth = 1;
                        if (fd.FieldSelectionSet != null)
                            depth += this.ComputeMaxDepth(fd.FieldSelectionSet, walkedNamedFragments);
                        if (depth > maxDepth)
                            maxDepth = depth;
                    }
                    else if (child is IInlineFragmentDocumentPart iif)
                    {
                        var depth = this.ComputeMaxDepth(iif.FieldSelectionSet, walkedNamedFragments);
                        if (depth > maxDepth)
                            maxDepth = depth;
                    }
                    else if (child is IFragmentSpreadDocumentPart spread && spread.Fragment != null)
                    {
                        if (!walkedNamedFragments.Contains(spread.Fragment))
                        {
                            walkedNamedFragments.Add(spread.Fragment);
                            var depth = this.ComputeMaxDepth(spread.Fragment.FieldSelectionSet, walkedNamedFragments);
                            walkedNamedFragments.Remove(spread.Fragment);

                            if (depth > maxDepth)
                                maxDepth = depth;
                        }
                    }
                }
            }

            return maxDepth;
        }
    }
}