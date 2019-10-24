// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Engine
{
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// An factory that can generate a new collection of metrics that can be used for performance monitoring a request.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this factory is registered for.</typeparam>
    public interface IGraphQueryExecutionMetricsFactory<TSchema>
    {
        /// <summary>
        /// Creates a new instance of the metrics package this factory can create.
        /// </summary>
        /// <returns>IQueryExecutionMetrics.</returns>
        IGraphQueryExecutionMetrics CreateMetricsPackage();
    }
}