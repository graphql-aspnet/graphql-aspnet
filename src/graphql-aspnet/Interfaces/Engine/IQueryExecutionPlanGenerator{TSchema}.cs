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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A plan generator capable of converting a fully constructed query document into an actionable
    /// query plan executable by a the query pipeline.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this plan generator is registered for.</typeparam>
    public interface IQueryExecutionPlanGenerator<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Creates a new query plan from the parsed operation.
        /// </summary>
        /// <param name="operation">The operation to generate a query plan for.</param>
        /// <returns>Task&lt;IGraphQueryPlan&gt;.</returns>
        Task<IQueryExecutionPlan> CreatePlanAsync(IOperationDocumentPart operation);
    }
}