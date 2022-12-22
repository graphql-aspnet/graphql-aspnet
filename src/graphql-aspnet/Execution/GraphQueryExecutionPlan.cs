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
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A query plan detailing the field data requested by an end user and the order in which
    /// the fields should be resolved. Also acts as a collection bin for any messages generated by
    /// server code during the execution.
    /// </summary>
    /// <typeparam name="TSchema">The type of the graphql schema to this plan exists for.</typeparam>
    [Serializable]
    [DebuggerDisplay("Operation {OperationName}")]
    internal sealed class GraphQueryExecutionPlan<TSchema> : IGraphQueryPlan
         where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQueryExecutionPlan{TSchema}" /> class.
        /// </summary>
        public GraphQueryExecutionPlan()
        {
            this.Id = Guid.NewGuid();
            this.Messages = new GraphMessageCollection();
        }

        /// <inheritdoc />
        public Guid Id { get; }

        /// <inheritdoc />
        public IGraphMessageCollection Messages { get; }

        /// <inheritdoc />
        public IExecutableOperation Operation { get; set; }

        /// <inheritdoc />
        public bool IsValid => !this.Messages.Severity.IsCritical();

        /// <inheritdoc />
        public float EstimatedComplexity { get; set; }

        /// <inheritdoc />
        public Type SchemaType => typeof(TSchema);

        /// <inheritdoc />
        public string OperationName => this.Operation?.OperationName;

        /// <inheritdoc />
        public bool IsCacheable { get; set; }
    }
}