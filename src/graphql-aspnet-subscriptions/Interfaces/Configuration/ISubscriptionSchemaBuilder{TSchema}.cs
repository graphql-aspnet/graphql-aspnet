// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Configuration
{
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.SubscriptionEventExecution;

    /// <summary>
    /// A <see cref="ISchemaBuilder{TSchema}"/> with added support for configuring the
    /// subscription execution pipeline.
    /// </summary>
    /// <typeparam name="TSchema">The type of the t schema.</typeparam>
    public interface ISubscriptionSchemaBuilder<TSchema> : ISchemaBuilder<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Gets a builder to construct the subscription execution pipeline. This pipeline is invoked per event raised per
        /// registered subscription to process event data and ultimately transmit said to a connected client.
        /// </summary>
        /// <value>The field execution pipeline.</value>
        ISchemaPipelineBuilder<TSchema, ISubscriptionExecutionMiddleware, GraphSubscriptionExecutionContext> SubscriptionExecutionPipeline
        {
            get;
        }
    }
}