﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.TestServerExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.ApolloSubscriptionQueryExecution;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Variables;
    using Moq;

    /// <summary>
    /// A test builder to create a subscription query context that can be
    /// passed to an execution pipeline.
    /// </summary>
    public class SubscriptionContextBuilder
    {
        private readonly Mock<IGraphOperationRequest> _mockRequest;

        private readonly List<KeyValuePair<GraphFieldPath, object>> _sourceData;

        private IGraphQueryExecutionMetrics _metrics;
        private IGraphEventLogger _eventLogger;
        private ISubscriptionClientProxy _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionContextBuilder" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public SubscriptionContextBuilder(ISubscriptionClientProxy client)
        {
            _client = client;
            _mockRequest = new Mock<IGraphOperationRequest>();
            _sourceData = new List<KeyValuePair<GraphFieldPath, object>>();

            _mockRequest.Setup(x => x.ToDataPackage()).Returns(
                new AspNet.GraphQueryData()
                {
                    Query = _mockRequest.Object.QueryText,
                    Variables = new InputVariableCollection(_mockRequest.Object.VariableData),
                    OperationName = _mockRequest.Object.OperationName,
                });
        }

        /// <summary>
        /// Adds a set of variables to use in the request.
        /// </summary>
        /// <param name="jsonDocument">The json document containing the variable data.</param>
        /// <returns>MockOperationRequest.</returns>
        public SubscriptionContextBuilder AddVariableData(string jsonDocument)
        {
            var variableData = JsonSerializer.Deserialize<InputVariableCollection>(jsonDocument);
            _mockRequest.Setup(x => x.VariableData).Returns(variableData);
            return this;
        }

        /// <summary>
        /// Adds the name of the operation to be executed to this instance.
        /// </summary>
        /// <param name="operationName">Name of the operation.</param>
        /// <returns>MockOperationRequest.</returns>
        public SubscriptionContextBuilder AddOperationName(string operationName)
        {
            _mockRequest.Setup(x => x.OperationName).Returns(operationName);
            return this;
        }

        /// <summary>
        /// Adds the metrics package to the query context that is being generated by this builder.
        /// </summary>
        /// <param name="metricsPackage">The metrics package.</param>
        /// <returns>SubscriptionContextBuilder.</returns>
        public SubscriptionContextBuilder AddMetrics(IGraphQueryExecutionMetrics metricsPackage)
        {
            _metrics = metricsPackage;
            return this;
        }

        /// <summary>
        /// Adds the speficied query text as the query to execute.
        /// </summary>
        /// <param name="queryText">The query text.</param>
        /// <returns>MockOperationRequest.</returns>
        public SubscriptionContextBuilder AddQueryText(string queryText)
        {
            _mockRequest.Setup(x => x.QueryText).Returns(queryText);
            return this;
        }

        /// <summary>
        /// Adds the given logger instance to the contexts that this builder creates.
        /// </summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <returns>SubscriptionContextBuilder.</returns>
        public SubscriptionContextBuilder AddLogger(IGraphEventLogger eventLogger)
        {
            _eventLogger = eventLogger;
            return this;
        }

        /// <summary>
        /// Adds a new default value that will be included in the built context.
        /// </summary>
        /// <param name="path">The field path representing the action to accept the parameter.</param>
        /// <param name="sourceData">The source data.</param>
        /// <returns>SubscriptionContextBuilder.</returns>
        public SubscriptionContextBuilder AddDefaultValue(GraphFieldPath path, object sourceData)
        {
            _sourceData.Add(new KeyValuePair<GraphFieldPath, object>(path, sourceData));
            return this;
        }

        /// <summary>
        /// Creates this query context instance that can be executed against the test server.
        /// </summary>
        /// <param name="subscriptionId">The subscription identifier to assign to the created sub.</param>
        /// <returns>GraphQueryContext.</returns>
        public virtual SubcriptionExecutionContext Build(string subscriptionId = null)
        {
            subscriptionId = subscriptionId ?? Guid.NewGuid().ToString();
            var metaData = new MetaDataCollection();

            // unchangable items about the request
            var request = new Mock<IGraphOperationRequest>();

            // updateable items about the request
            var context = new SubcriptionExecutionContext(
                _client,
                this.OperationRequest,
                subscriptionId,
                _metrics,
                _eventLogger,
                metaData);

            foreach (var kvp in _sourceData)
            {
                var mockField = new Mock<IGraphField>();
                mockField.Setup(x => x.FieldSource).Returns(GraphQL.AspNet.Internal.TypeTemplates.GraphFieldTemplateSource.Action);
                mockField.Setup(x => x.Route).Returns(kvp.Key);
                context.DefaultFieldSources.AddSource(mockField.Object, kvp.Value);
            }

            return context;
        }

        /// <summary>
        /// Gets the mocked operation request as its currently defined by this builder.
        /// </summary>
        /// <value>The operation request.</value>
        public IGraphOperationRequest OperationRequest => _mockRequest.Object;
    }
}