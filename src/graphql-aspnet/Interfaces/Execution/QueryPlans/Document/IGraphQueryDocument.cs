// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document
{
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;

    /// <summary>
    /// An interface representing a query document that can be
    /// executed by a graphql schema.
    /// </summary>
    public interface IGraphQueryDocument : IDocumentPart
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