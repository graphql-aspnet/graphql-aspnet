// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Messaging.Handlers
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Messaging;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Messaging.Messages;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A handler for processing client operation stop requests.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema under which this handler is operating.</typeparam>
    [DebuggerDisplay("Client Operation Started Handler")]
    internal class ApolloSubscriptionStartHandler<TSchema> : ApolloMessageHandler
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloSubscriptionStartHandler{TSchema}"/> class.
        /// </summary>
        public ApolloSubscriptionStartHandler()
        {
        }

        /// <summary>
        /// Gets the type of the message this handler can process.
        /// </summary>
        /// <value>The type of the message.</value>
        public override ApolloMessageType MessageType => ApolloMessageType.START;

        /// <summary>
        /// Handles the message, executing the logic of this handler against it.
        /// </summary>
        /// <param name="clientProxy">The client proxy.</param>
        /// <param name="message">The message to be handled.</param>
        /// <returns>A newly set of messages (if any) to be sent back to the client.</returns>
        public override async Task<IEnumerable<IApolloMessage>> HandleMessage(
            ISubscriptionClientProxy clientProxy,
            IApolloMessage message)
        {
            Validation.ThrowIfNull(clientProxy, nameof(clientProxy));

            var startmessage = message as ApolloSubscriptionStartMessage;
            Validation.ThrowIfNull(startmessage, nameof(message));

            var resultMessages = new List<IApolloMessage>();

            var graphQLRuntime = clientProxy.ServiceProvider.GetRequiredService(typeof(IGraphQLRuntime<TSchema>))
                as IGraphQLRuntime;

            var request = graphQLRuntime.CreateRequest(startmessage.Payload);

            var response = await graphQLRuntime.ExecuteRequest(
                clientProxy.ServiceProvider,
                clientProxy.User,
                request,
                null);

            return resultMessages;
        }
    }
}