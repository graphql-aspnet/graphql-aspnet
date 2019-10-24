// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Interfaces
{
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;

    /// <summary>
    /// An interface representing a query document that can be
    /// executed by a graphql schema.
    /// </summary>
    public interface IGraphQueryDocument
    {
        /// <summary>
        /// Gets any messages generated during the generation of this document.
        /// </summary>
        /// <value>The messages.</value>
        IGraphMessageCollection Messages { get; }

        /// <summary>
        /// Gets the collected set of operations parsed from a user's query document.
        /// </summary>
        /// <value>The operations.</value>
        IQueryOperationCollection Operations { get; }

        /// <summary>
        /// Gets the field maximum depth of any given operation of this document.
        /// </summary>
        /// <value>The maximum depth.</value>
        int MaxDepth { get; }
    }
}