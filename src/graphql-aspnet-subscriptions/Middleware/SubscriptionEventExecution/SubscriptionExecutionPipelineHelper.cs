// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.SubscriptionEventExecution
{
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A decorator for the subscription execution pipeline builder to configure default components.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this helper will add components for.</typeparam>
    public class SubscriptionExecutionPipelineHelper<TSchema>
        where TSchema : class, ISchema
    {
        private readonly ISchemaPipelineBuilder<TSchema, ISubscriptionExecutionMiddleware, GraphSubscriptionExecutionContext> _pipelineBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionExecutionPipelineHelper{TSchema}" /> class.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        public SubscriptionExecutionPipelineHelper(ISchemaPipelineBuilder<TSchema, ISubscriptionExecutionMiddleware, GraphSubscriptionExecutionContext> pipelineBuilder)
        {
            _pipelineBuilder = Validation.ThrowIfNullOrReturn(pipelineBuilder, nameof(pipelineBuilder));
        }
    }
}