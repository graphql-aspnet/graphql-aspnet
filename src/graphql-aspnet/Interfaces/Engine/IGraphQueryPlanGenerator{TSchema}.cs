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
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;

    /// <summary>
    /// A plan generator capable of converting a fully constructed query document into an actionable
    /// query plan executable by a the query pipeline.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this plan generator is registered for.</typeparam>
    public interface IGraphQueryPlanGenerator<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Creates a new query plan from the parsed syntax tree.
        /// </summary>
        /// <param name="queryDocument">The parsed, and validated query document to
        /// convert into a query plan.</param>
        /// <returns>Task&lt;IGraphQueryPlan&gt;.</returns>
        Task<IGraphQueryPlan> CreatePlan(IGraphQueryDocument queryDocument);
    }
}