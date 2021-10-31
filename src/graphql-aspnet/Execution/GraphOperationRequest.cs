// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Variables;
    using GraphQL.AspNet.Variables;

    /// <summary>
    /// A context object representing a single request, by a single requestor, to use through the query execution process.
    /// </summary>
    [DebuggerDisplay("Query Length = {QueryLength} (Operation = {OperationName})")]
    public class GraphOperationRequest : IGraphOperationRequest
    {
        /// <summary>
        /// Creates a new operation result from a collection of generated messages and optional raw data
        /// provided by a requestor.
        /// </summary>
        /// <param name="errorMessages">The collection of messages. Must be not null and contain at least one message.</param>
        /// <param name="queryData">The original query data.</param>
        /// <returns>GraphOperationResult.</returns>
        public static GraphOperationResult FromMessages(IGraphMessageCollection errorMessages, GraphQueryData queryData = null)
        {
            Validation.ThrowIfNull(errorMessages, nameof(errorMessages));
            if (errorMessages.Count < 1)
                errorMessages.Critical("An unknown error occured.");

            return new GraphOperationResult(
                new GraphOperationRequest(queryData),
                errorMessages);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphOperationRequest"/> class.
        /// </summary>
        /// <param name="queryData">The query data.</param>
        public GraphOperationRequest(GraphQueryData queryData)
        {
            this.Id = Guid.NewGuid().ToString("N");
            this.OperationName = queryData.OperationName?.Trim();
            this.QueryText = queryData.Query;
            this.VariableData = queryData.Variables ?? new InputVariableCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphOperationRequest"/> class.
        /// </summary>
        /// <param name="request">The request to injest into this request.</param>
        protected GraphOperationRequest(IGraphOperationRequest request)
        {
            Validation.ThrowIfNull(request, nameof(request));
            this.Id = request.Id;
            this.OperationName = request.OperationName;
            this.QueryText = request.QueryText;
            this.VariableData = request.VariableData;
        }

        /// <summary>
        /// Extracts a raw data package from this request .
        /// </summary>
        /// <returns>GraphQueryData.</returns>
        public GraphQueryData ToDataPackage()
        {
            return new GraphQueryData()
            {
                Query = this.QueryText,
                Variables = new InputVariableCollection(this.VariableData),
                OperationName = this.OperationName,
            };
        }

        /// <summary>
        /// Gets or sets the name of the operation, from the supplied query document, to execute.
        /// </summary>
        /// <value>The name of the operation.</value>
        public string OperationName { get; set; }

        /// <summary>
        /// Gets the query text that was supplied by the end user to be parsed and processed.
        /// </summary>
        /// <value>The query text.</value>
        public string QueryText { get; }

        /// <summary>
        /// Gets or sets the variables, if any, supplied by the end user.
        /// </summary>
        /// <value>The variables.</value>
        public IInputVariableCollection VariableData { get; set; }

        /// <summary>
        /// Gets a globally unique identifier assigned to this request when it was created.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; }

        /// <summary>
        /// Gets the length of the <see cref="QueryText"/>.
        /// </summary>
        /// <value>The length of the query.</value>
        protected int QueryLength => QueryText?.Length ?? 0;
    }
}