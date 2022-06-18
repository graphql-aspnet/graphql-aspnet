// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts
{
    /// <summary>
    /// A document part representing the declaration of a variable
    /// on an operation defined in a query document.
    /// </summary>
    public interface IQueryVariableDocumentPart : IAssignableValueDocumentPart
    {
        /// <summary>
        /// Marks this variable as being referenced within the operation.
        /// </summary>
        internal void MarkAsReferenced();

        /// <summary>
        /// Gets a value indicating whether this instance is referenced somewhere within the operation
        /// where its defined.
        /// </summary>
        /// <value><c>true</c> if this instance is referenced; otherwise, <c>false</c>.</value>
        bool IsReferenced { get; }
    }
}