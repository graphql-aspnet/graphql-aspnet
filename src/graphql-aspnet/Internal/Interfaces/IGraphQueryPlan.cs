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
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A represention of a set of operations to be fulfilled to generate data for a
    /// <see cref="IGraphOperationRequest"/>.
    /// </summary>
    public interface IGraphQueryPlan
    {
        /// <summary>
        /// Adds a parsed executable operation to the plan's operation collection.
        /// </summary>
        /// <param name="operation">The completed and validated operation to add.</param>
        void AddOperation(IGraphFieldExecutableOperation operation);

        /// <summary>
        /// Retrieves the operation from those contained in this plan. If operationName is empty or null
        /// and this plan contains only one operation that singular operation will be returned. Otherwise, if the operation
        /// is not found null will be returned.
        /// </summary>
        /// <param name="operationName">Name of the operation.</param>
        /// <returns>IFieldExecutionContextCollection.</returns>
        IGraphFieldExecutableOperation RetrieveOperation(string operationName = null);

        /// <summary>
        /// Gets the collection of field contexts that need to be executed to fulfill an operation of a given name.
        /// </summary>
        /// <value>The operations.</value>
        IReadOnlyDictionary<string, IGraphFieldExecutableOperation> Operations { get; }

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
        /// Gets the unique identifier assigned to this instance when it was created.
        /// </summary>
        /// <value>The identifier.</value>
        string Id { get; }
    }
}