// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts
{
    /// <summary>
    /// An interface employed by any document part that contains references to other parts
    /// of the document (i.e. fragment spreads and variable usages).
    /// </summary>
    public interface IReferenceDocumentPart : IDocumentPart
    {
        /// <summary>
        /// Gets the set named fragment spreads used in field selection sets within this document part.
        /// </summary>
        /// <value>All named fragment spreads within this operation.</value>
        IFragmentSpreadCollectionDocumentPart FragmentSpreads { get; }

        /// <summary>
        /// Gets the supplied value items which reference a variable, on fields or directives, within
        /// this document part.
        /// </summary>
        /// <value>All variable references declared within this operation.</value>
        IVariableUsageCollectionDocumentPart VariableUsages { get; }
    }
}