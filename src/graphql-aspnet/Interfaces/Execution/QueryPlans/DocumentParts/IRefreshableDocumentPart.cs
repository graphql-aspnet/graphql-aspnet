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
    /// A document part that implements a refresh mechanism to update, clear or rebuild some
    /// internal mechanisms.
    /// </summary>
    public interface IRefreshableDocumentPart : IDocumentPart
    {
        /// <summary>
        /// Forces this document part to do an internal refresh in a manner suitable to its function.
        /// </summary>
        void Refresh();
    }
}