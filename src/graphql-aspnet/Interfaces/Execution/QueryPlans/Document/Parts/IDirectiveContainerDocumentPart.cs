// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts
{
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;

    /// <summary>
    /// An interface declaring the implementer as a document part that can contain directives.
    /// </summary>
    public interface IDirectiveContainerDocumentPart : IDocumentPart
    {
        /// <summary>
        /// Gets the set of directives defined as direct children on this document part.
        /// </summary>
        /// <value>The directives.</value>
        IDirectiveCollectionDocumentPart Directives { get; }
    }
}