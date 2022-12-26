// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution
{
    using System;

    /// <summary>
    /// A plan telling the runtime how to fulfill a <see cref="IQueryExecutionRequest"/>
    /// and produce a result.
    /// </summary>
    public interface IQueryExecutionPlan
    {
        /// <summary>
        /// Gets or sets the executable operation that can be carried out by this query plan.
        /// </summary>
        /// <value>The query operation to execute.</value>
        IExecutableOperation Operation { get; set; }

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
        /// Gets or sets a value representing a total estimated complexity of the fields requested, their expected return values and nested dependencies there in. This value
        /// is used as a measure for determining executability of a query and to disallow unreasonable queries from overwhelming the server.
        /// </summary>
        /// <value>The total estimated complexity of this query plan.</value>
        float EstimatedComplexity { get; set; }

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
        Guid Id { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this query plan can be cached. In general,
        /// executable operations that contain directives are not cachable.
        /// </summary>
        /// <value><c>true</c> if this instance is cacheable; otherwise, <c>false</c>.</value>
        bool IsCacheable { get; set; }
    }
}