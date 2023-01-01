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
    /// A document part encapsulating a named fragment declared in a supplied
    /// query document.
    /// </summary>
    public interface INamedFragmentDocumentPart : IFragmentDocumentPart, IReferenceDocumentPart, ITopLevelDocumentPart, IDirectiveContainerDocumentPart
    {
        /// <summary>
        /// Informs this named fragment that it is being spread by the given part.
        /// </summary>
        /// <param name="spreadPart">The fragment spread document part where this
        /// named fragment was spread into a selection set.</param>
        void SpreadBy(IFragmentSpreadDocumentPart spreadPart);

        /// <summary>
        /// Gets a value indicating whether this instance is referenced by an operation in the query document.
        /// </summary>
        /// <value><c>true</c> if this instance is referenced; otherwise, <c>false</c>.</value>
        bool IsReferenced { get; }

        /// <summary>
        /// Gets the unique name of this reference in the collection.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }
    }
}