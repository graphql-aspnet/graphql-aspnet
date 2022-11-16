// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Defaults
{
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Metrics;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A factory to generate new metrics instances for a given schema.
    /// </summary>
    /// <typeparam name="TSchema">The type of the t schema.</typeparam>
    public class DefaultGraphQueryExecutionMetricsFactory<TSchema> : IGraphQueryExecutionMetricsFactory<TSchema>
        where TSchema : ISchema
    {
        private readonly TSchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGraphQueryExecutionMetricsFactory{TSchema}"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public DefaultGraphQueryExecutionMetricsFactory(TSchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <summary>
        /// Creates the metrics package.
        /// </summary>
        /// <returns>IQueryExecutionMetrics.</returns>
        public IGraphQueryExecutionMetrics CreateMetricsPackage()
        {
            return new ApolloTracingMetricsV1(_schema);
        }
    }
}