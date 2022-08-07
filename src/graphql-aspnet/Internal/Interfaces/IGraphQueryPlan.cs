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
    using System;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A represention of a set of operations to be fulfilled to generate data for a
    /// <see cref="IGraphOperationRequest"/>.
    /// </summary>
    public interface IGraphQueryPlan
    {
        /// <summary>
        /// Gets or sets the executable operation that can be carried out by this query plan.
        /// </summary>
        /// <value>The query operation to execute.</value>
        IGraphFieldExecutableOperation Operation { get; set; }

        /// <summary>
        /// Gets a value indicating whether this plan is in a valid and potentially executable state.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        bool IsValid { get; }

        /// <summary>
        /// Gets the messages generated, if any, during the generation of the plan.
        /// </summary>
        /// <value>The messages.</value>
        IGraphMessageCollection Messages { get; }

        /// <summary>
        /// Gets or sets a value representing a total estimate dcomplexity of the fields requested, their expected return values and nested dependencies there in. This value
        /// is used as a measure for determining executability of a query and to disallow unreasonable queries from overwhelming the server.
        /// </summary>
        /// <value>The total estimated complexity of this query plan.</value>
        float EstimatedComplexity { get; set; }

        /// <summary>
        /// Gets or sets the maximum depth of nested nodes the query text acheives in any operation or fragment.
        /// </summary>
        /// <value>The maximum depth.</value>
        int MaxDepth { get; set; }

        /// <summary>
        /// Gets the <see cref="Type"/> of the schema that this plan was created for.
        /// </summary>
        /// <value>The name of the schema.</value>
        Type SchemaType { get; }

        /// <summary>
        /// Gets the name of the operation contained within this query plan.
        /// </summary>
        /// <value>The name of the operation.</value>
        string OperationName { get; }

        /// <summary>
        /// Gets the unique identifier assigned to this instance when it was created.
        /// </summary>
        /// <value>The identifier.</value>
        string Id { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this query plan can be cached. In general,
        /// executable operations that contain directives are not cachable.
        /// </summary>
        /// <value><c>true</c> if this instance is cacheable; otherwise, <c>false</c>.</value>
        bool IsCacheable { get; set; }
    }
}