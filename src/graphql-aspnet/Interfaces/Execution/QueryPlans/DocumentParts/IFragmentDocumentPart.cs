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
    /// A common fragment definition shared by all fragment types within the query
    /// document (Inline and Named fragments).
    /// </summary>
    public interface IFragmentDocumentPart : IDirectiveContainerDocumentPart, IDocumentPart
    {
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