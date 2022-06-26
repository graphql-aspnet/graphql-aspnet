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
    /// A common fragment definition shared by all fragment types within the query
    /// document (Inline and Named fragments).
    /// </summary>
    public interface IFragmentDocumentPart : IDocumentPart
    {
        /// <summary>
        /// Marks this fragment as being referenced and used in at least one operation in the document.
        /// </summary>
        internal void MarkAsReferenced();

        /// <summary>
        /// Gets a value indicating whether this instance is referenced by an operation in the query document.
        /// </summary>
        /// <value><c>true</c> if this instance is referenced; otherwise, <c>false</c>.</value>
        bool IsReferenced { get; }

        /// <summary>
        /// Gets the name of the target graph type if any.
        /// </summary>
        /// <value>The name of the target graph type.</value>
        string TargetGraphTypeName { get; }

        /// <summary>
        /// Gets the set of fields queried on this fragment.
        /// </summary>
        /// <value>The field selection set.</value>
        IFieldSelectionSetDocumentPart FieldSelectionSet { get; }
    }
}