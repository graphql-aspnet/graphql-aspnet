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
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;

    /// <summary>
    /// A value supplied in a query document that repressents a list of other
    /// <see cref="ISuppliedValueDocumentPart"/>.
    /// </summary>
    public interface IListSuppliedValueDocumentPart : ISuppliedValueDocumentPart, IResolvableList
    {
        /// <summary>
        /// Gets the values supplied on this list in the source document.
        /// </summary>
        /// <value>The list items defined in the document.</value>
        public IReadOnlyList<ISuppliedValueDocumentPart> ListItems { get; }
    }
}