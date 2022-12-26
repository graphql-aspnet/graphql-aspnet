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
    /// A collection of fragment spreads owned by another document part. Used to quickly access
    /// and validate spreads during document validation.
    /// </summary>
    public interface IFragmentSpreadCollectionDocumentPart : IReadOnlyList<IFragmentSpreadDocumentPart>
    {
        /// <summary>
        /// Gets the owner of this collection.
        /// </summary>
        /// <value>The owner.</value>
        IDocumentPart Owner { get; }
    }
}