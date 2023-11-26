// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework.PipelineContextBuilders
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using NSubstitute;

    /// <summary>
    /// A subclassed <see cref="QueryExecutionContext"/> allowing for inline mocked replacements
    /// of various contained data to setup a test scenario.
    /// </summary>
    public class QueryContextBuilder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IQueryExecutionRequest _mockRequest;
        private readonly List<KeyValuePair<ItemPath, object>> _sourceData;

        private IUserSecurityContext _userSecurityContext;
        private IQueryExecutionMetrics _metrics;
        private IGraphEventLogger _eventLogger;
        private MetaDataCollection _items;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryContextBuilder" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="userSecurityContext">The user security context.</param>
        public QueryContextBuilder(
            IServiceProvider serviceProvider,
            IUserSecurityContext userSecurityContext)
        {
            _serviceProvider = Validation.ThrowIfNullOrReturn(serviceProvider, nameof(serviceProvider));
            _userSecurityContext = userSecurityContext;

            _items = new MetaDataCollection();
            _mockRequest = Substitute.For<IQueryExecutionRequest>();
            _mockRequest.Items.Returns(_items);
            _sourceData = new List<KeyValuePair<ItemPath, object>>();
        }

        /// <summary>
        /// Adds a set of variables to use in the request.
        /// </summary>
        /// <param name="jsonDocument">The json document containing the variable data.</param>
        /// <returns>QueryContextBuilder.</returns>
        public QueryContextBuilder AddVariableData(string jsonDocument)
        {
            var variableData = InputVariableCollection.FromJsonDocument(jsonDocument);
            _mockRequest.VariableData.Returns(variableData);
            return this;
        }

        /// <summary>
        /// Adds the name of the operation to be executed to this instance.
        /// </summary>
        /// <param name="operationName">Name of the operation.</param>
        /// <returns>QueryContextBuilder.</returns>
        public QueryContextBuilder AddOperationName(string operationName)
        {
            _mockRequest.OperationName.Returns(operationName);
            return this;
        }

        /// <summary>
        /// Adds the metrics package to the query context that is being generated by this builder.
        /// </summary>
        /// <param name="metricsPackage">The metrics package.</param>
        /// <returns>QueryContextBuilder.</returns>
        public QueryContextBuilder AddMetrics(IQueryExecutionMetrics metricsPackage)
        {
            _metrics = metricsPackage;
            return this;
        }

        /// <summary>
        /// Adds the speficied query text as the query to execute.
        /// </summary>
        /// <param name="queryText">The query text.</param>
        /// <returns>QueryContextBuilder.</returns>
        public QueryContextBuilder AddQueryText(string queryText)
        {
            _mockRequest.QueryText.Returns(queryText);
            return this;
        }

        /// <summary>
        /// Adds the given logger instance to the contexts that this builder creates.
        /// </summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <returns>QueryContextBuilder.</returns>
        public QueryContextBuilder AddLogger(IGraphEventLogger eventLogger)
        {
            _eventLogger = eventLogger;
            return this;
        }

        /// <summary>
        /// Adds or updates the security context for this builder to be the supplied value, even
        /// if null.
        /// </summary>
        /// <param name="securityContext">The security context.</param>
        /// <returns>QueryContextBuilder.</returns>
        public QueryContextBuilder AddUserSecurityContext(IUserSecurityContext securityContext)
        {
            _userSecurityContext = securityContext;
            return this;
        }

        /// <summary>
        /// Adds a new default value that will be included in the built context.
        /// </summary>
        /// <param name="path">The field path representing the action to accept the parameter.</param>
        /// <param name="sourceData">The source data.</param>
        /// <returns>QueryContextBuilder.</returns>
        public QueryContextBuilder AddDefaultValue(ItemPath path, object sourceData)
        {
            _sourceData.Add(new KeyValuePair<ItemPath, object>(path, sourceData));
            return this;
        }

        /// <summary>
        /// Creates this query context instance that can be executed against the test server.
        /// </summary>
        /// <returns>GraphQueryContext.</returns>
        public virtual QueryExecutionContext Build()
        {
            var startDate = DateTimeOffset.UtcNow;
            _mockRequest.StartTimeUTC.Returns(startDate);

            // updateable items about the request
            var context = new QueryExecutionContext(
                this.QueryRequest,
                _serviceProvider,
                new QuerySession(),
                securityContext: _userSecurityContext,
                items: this.QueryRequest.Items,
                metrics: _metrics,
                logger: _eventLogger);

            foreach (var kvp in _sourceData)
            {
                var mockField = Substitute.For<IGraphField>();
                mockField.FieldSource.Returns(GraphFieldSource.Action);
                mockField.ItemPath.Returns(kvp.Key);
                context.DefaultFieldSources.AddSource(mockField, kvp.Value);
            }

            return context;
        }

        /// <summary>
        /// Gets the mocked request as its currently defined by this builder.
        /// </summary>
        /// <value>The request.</value>
        public IQueryExecutionRequest QueryRequest => _mockRequest;
    }
}