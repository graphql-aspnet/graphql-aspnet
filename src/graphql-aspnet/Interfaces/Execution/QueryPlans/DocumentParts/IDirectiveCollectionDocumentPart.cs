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
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts.Common;

    /// <summary>
    /// A collection of directives assigned to a part of a query document.
    /// </summary>
    public interface IDirectiveCollectionDocumentPart : IEnumerable<IDirectiveDocumentPart>
    {
        /// <summary>
        /// Gets the number of directives in this collection.
        /// </summary>
        /// <value>The number of directives.</value>
        int Count { get; }

        /// <summary>
        /// Gets the <see cref="IDirectiveDocumentPart"/> at the specified index.
        /// </summary>
        /// <param name="index">The index of the directive.</param>
        /// <returns>IDirectiveDocumentPart.</returns>
        IDirectiveDocumentPart this[int index] { get; }

        /// <summary>
        /// Gets the document part that owns this collection of directives.
        /// </summary>
        /// <value>The owner.</value>
        IDocumentPart Owner { get; }
    }
}