// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.PlanGeneration
{
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;

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

        /// <summary>
        /// Gets or sets the maximum depth of any field of this document.
        /// </summary>
        /// <value>The maximum depth achived by the document.</value>
        int MaxDepth { get; set; }
    }
}