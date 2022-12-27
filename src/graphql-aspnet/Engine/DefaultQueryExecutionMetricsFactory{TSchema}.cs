// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Metrics;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A factory to generate new metrics instances for a given schema.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this factory creates metrics for.</typeparam>
    public class DefaultQueryExecutionMetricsFactory<TSchema> : IQueryExecutionMetricsFactory<TSchema>
        where TSchema : ISchema
    {
        private readonly TSchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultQueryExecutionMetricsFactory{TSchema}"/> class.
        /// </summary>
        /// <param name="schema">The schema instance the metrics package should reference.</param>
        public DefaultQueryExecutionMetricsFactory(TSchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <inheritdoc />
        public IQueryExecutionMetrics CreateMetricsPackage()
        {
            return new ApolloTracingMetricsV1(_schema);
        }
    }
}