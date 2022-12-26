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
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts.Common;

    /// <summary>
    /// An interface representing a parsed query document generated from a graphql request.
    /// </summary>
    public interface IQueryDocument : IDocumentPart
    {
        /// <summary>
        /// Gets any messages generated during the generation of this document.
        /// </summary>
        /// <value>The messages.</value>
        IGraphMessageCollection Messages { get; }

        /// <summary>
        /// Gets the operations defined on this query document.
        /// </summary>
        /// <value>The operations.</value>
        IOperationCollectionDocumentPart Operations { get; }

        /// <summary>
        /// Gets the set of named fragments defined on this query document.
        /// </summary>
        /// <value>The named fragments.</value>
        INamedFragmentCollectionDocumentPart NamedFragments { get; }
    }
}