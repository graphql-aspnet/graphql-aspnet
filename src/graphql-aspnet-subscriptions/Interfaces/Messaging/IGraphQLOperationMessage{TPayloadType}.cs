// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Messaging
{
    using System;

    /// <summary>
    /// A representation of a graphql message sent or recieved over a presistent connection.
    /// </summary>
    /// <typeparam name="TPayloadType">The type of payload expected for this message.</typeparam>
    public interface IGraphQLOperationMessage<TPayloadType> : IGraphQLOperationMessage
        where TPayloadType : class
    {
        /// <summary>
        /// Gets the type of the payload handled by this message.
        /// </summary>
        /// <value>The type of the payload.</value>
        public Type PayloadType { get; }

        /// <summary>
        /// Gets or sets the payload of the message as a stringified json object.
        /// </summary>
        /// <value>The payload.</value>
        TPayloadType Payload { get; set; }
    }
}