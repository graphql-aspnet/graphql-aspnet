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
        /// Initializes a new instance of the <see cref="GraphOperationRequest"/> class.
        /// </summary>
        /// <param name="queryData">The query data.</param>
        public GraphOperationRequest(GraphQueryData queryData)
        {
            this.Id = Guid.NewGuid().ToString("N");
            this.OperationName = queryData.OperationName?.Trim();
            this.QueryText = queryData.Query;
            this.VariableData = queryData.Variables ?? new InputVariableCollection();

            this.StartTimeUTC = DateTimeOffset.UtcNow;
            this.Items = new MetaDataCollection();
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

            this.StartTimeUTC = request.StartTimeUTC;
            this.Items = request.Items.Clone();
        }

        /// <inheritdoc />
        public GraphQueryData ToDataPackage()
        {
            return new GraphQueryData()
            {
                Query = this.QueryText,
                Variables = new InputVariableCollection(this.VariableData),
                OperationName = this.OperationName,
            };
        }

        /// <inheritdoc />
        public string OperationName { get; set; }

        /// <inheritdoc />
        public string QueryText { get; }

        /// <inheritdoc />
        public IInputVariableCollection VariableData { get; set; }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>
        /// Gets the length of the <see cref="QueryText"/>.
        /// </summary>
        /// <value>The length of the query.</value>
        protected int QueryLength => QueryText?.Length ?? 0;

        /// <inheritdoc />
        public DateTimeOffset StartTimeUTC { get; }

        /// <inheritdoc />
        public MetaDataCollection Items { get; }
    }
}