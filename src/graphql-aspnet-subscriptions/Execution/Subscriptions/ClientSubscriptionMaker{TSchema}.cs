// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Subscriptions
{
    using System;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;

    /// <summary>
    /// A helper object that will create a client subscription with the proper DI scope.
    /// </summary>
    /// <typeparam name="TSchema">The type of the t schema.</typeparam>
    public class ClientSubscriptionMaker<TSchema> : IClientSubscriptionMaker<TSchema>
        where TSchema : class, ISchema
    {
        private IGraphQLDocumentParser _parser;
        private IGraphQueryPlanGenerator<TSchema> _planGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSubscriptionMaker{TSchema}"/> class.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="planGenerator">The plan generator.</param>
        public ClientSubscriptionMaker(IGraphQLDocumentParser parser, IGraphQueryPlanGenerator<TSchema> planGenerator)
        {
            _parser = Validation.ThrowIfNullOrReturn(parser, nameof(parser));
            _planGenerator = Validation.ThrowIfNullOrReturn(planGenerator, nameof(planGenerator));
        }

        /// <summary>
        /// Creates a new encapsulated subscription from a graphql data package.
        /// </summary>
        /// <param name="data">The data package recieved from the client.</param>
        /// <returns>Task&lt;ClientSubscription&lt;TSchema&gt;&gt;.</returns>
        public async Task<ClientSubscription<TSchema>> Create(GraphQueryData data)
        {
            var syntaxTree = _parser.ParseQueryDocument(data.Query.AsMemory());
            var plan = await _planGenerator.CreatePlan(syntaxTree);

            return new ClientSubscription<TSchema>(plan, data.OperationName);
        }
    }
}