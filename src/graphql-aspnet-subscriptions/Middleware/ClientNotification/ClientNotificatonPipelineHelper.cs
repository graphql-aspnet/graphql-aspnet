// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.ClientNotification
{
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A decorator for the client notification pipeline pipeline builder to configure default components.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this helper will add components for.</typeparam>
    public class ClientNotificatonPipelineHelper<TSchema>
        where TSchema : class, ISchema
    {
        private readonly ISchemaPipelineBuilder<TSchema, ISubscriptionClientNotificationMiddleware, ClientNotificationContext> _pipelineBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientNotificatonPipelineHelper{TSchema}" /> class.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        public ClientNotificatonPipelineHelper(ISchemaPipelineBuilder<TSchema, ISubscriptionClientNotificationMiddleware, ClientNotificationContext> pipelineBuilder)
        {
            _pipelineBuilder = Validation.ThrowIfNullOrReturn(pipelineBuilder, nameof(pipelineBuilder));
        }
    }
}