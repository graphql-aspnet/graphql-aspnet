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
    using System.Collections.Generic;

    /// <summary>
    /// A collection of directives assigned to a part of a query document.
    /// </summary>
    public interface IDirectiveCollectionDocumentPart : IReadOnlyList<IDirectiveDocumentPart>
    {
        /// <summary>
        /// Gets the document part that owns this collection of directives.
        /// </summary>
        /// <value>The owner.</value>
        IDocumentPart Owner { get; }
    }
}