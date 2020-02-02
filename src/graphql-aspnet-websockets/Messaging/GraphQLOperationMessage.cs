// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Messaging
{
    using GraphQL.AspNet.Interfaces.Messaging;

    /// <summary>
    /// An implementation of the required operation message interface.
    /// </summary>
    public class GraphQLOperationMessage : IGraphQLOperationMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLOperationMessage"/> class.
        /// </summary>
        public GraphQLOperationMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLOperationMessage"/> class.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        public GraphQLOperationMessage(OperationMessageType messageType)
        {
            this.Type = messageType;
            this.Payload = null;
            this.Id = null;
        }

        /// <summary>
        /// Gets or sets the payload of the message as a stringified json object.
        /// </summary>
        /// <value>The payload.</value>
        public string Payload { get; set; }

        /// <summary>
        /// Gets or sets the identifier for the scoped operation started by a client.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the message, indicating expected payload types.
        /// </summary>
        /// <value>The type.</value>
        public OperationMessageType Type { get; set; }
    }
}