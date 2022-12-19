﻿// *************************************************************
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
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// The default implementation of <see cref="IGraphFieldRequest"/>. Provides a container
    /// for various data fields needed to resolve a field.
    /// </summary>
    [DebuggerDisplay("{InvocationContext.Field.Name}, (Leaf = {InvocationContext.Field.IsLeaf})")]
    [DebuggerStepThrough]
    internal class GraphFieldRequest : IGraphFieldRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldRequest" /> class.
        /// </summary>
        /// <param name="parentOperationRequest">The original operation request from which ths field
        /// request was generated.</param>
        /// <param name="invocationContext">The invocation context that defines how hte field
        /// should be processed according to the query plan.</param>
        /// <param name="dataSource">The data source containing the source input data to the field as well as
        /// the graph items referenced by said input data.</param>
        /// <param name="origin">The place in a source document where this request appeared.</param>
        public GraphFieldRequest(
            IGraphOperationRequest parentOperationRequest,
            IGraphFieldInvocationContext invocationContext,
            GraphDataContainer dataSource,
            SourceOrigin origin = default)
        {
            this.OperationRequest = Validation.ThrowIfNullOrReturn(parentOperationRequest, nameof(parentOperationRequest));
            this.Id = Guid.NewGuid();
            this.InvocationContext = Validation.ThrowIfNullOrReturn(invocationContext, nameof(invocationContext));
            this.Data = dataSource;
            this.Items = parentOperationRequest.Items;

            // this may be different than the source indicated by teh parent operation request
            // do to child item resolution on arrays
            this.Origin = origin;
        }

        /// <inheritdoc />
        public IGraphOperationRequest OperationRequest { get; }

        /// <inheritdoc />
        public Guid Id { get; }

        /// <inheritdoc />
        public MetaDataCollection Items { get; }

        /// <inheritdoc />
        public SourceOrigin Origin { get; }

        /// <inheritdoc />
        public GraphDataContainer Data { get; private set; }

        /// <inheritdoc />
        public IGraphField Field => this.InvocationContext.Field;

        /// <inheritdoc />
        public IGraphFieldInvocationContext InvocationContext { get; }
    }
}