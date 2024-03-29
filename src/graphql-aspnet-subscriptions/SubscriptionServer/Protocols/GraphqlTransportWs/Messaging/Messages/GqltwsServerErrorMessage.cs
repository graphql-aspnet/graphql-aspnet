﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messaging.Messages
{
    using System;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messaging.Common;

    /// <summary>
    /// An graphql-ws specific message containing one error that occured outside of any
    /// given graph query execution. This message terminates the connection.
    /// </summary>
    internal class GqltwsServerErrorMessage : GqltwsMessage<IGraphMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsServerErrorMessage" /> class.
        /// </summary>
        /// <param name="message">The message to put into the generated error.</param>
        /// <param name="lastMessage">The last message received by the server, generally the message
        /// that resulted in this error being generated.</param>
        /// <param name="clientProvidedId">The client provided identifier of the failed operation.</param>
        public GqltwsServerErrorMessage(
            IGraphMessage message,
            GqltwsMessage lastMessage = null,
            string clientProvidedId = null)
            : this(
                  message?.Message,
                  message?.Code,
                  message?.Severity ?? GraphMessageSeverity.Critical,
                  lastMessage?.Id,
                  lastMessage?.Type,
                  message?.Exception,
                  clientProvidedId)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsServerErrorMessage" /> class.
        /// </summary>
        /// <param name="message">The message to put into the generated error.</param>
        /// <param name="code">The custom code to apply to the error message.</param>
        /// <param name="severity">The severity of the error message being generated.</param>
        /// <param name="lastMessage">The last message received by the server, generally the message
        /// that resulted in this error being generated.</param>
        /// <param name="exception">An internal exception that was thrown that should be carried with this message.</param>
        /// <param name="clientProvidedId">The client provided identifier of the failed operation.</param>
        public GqltwsServerErrorMessage(
            string message,
            string code = Constants.ErrorCodes.DEFAULT,
            GraphMessageSeverity severity = GraphMessageSeverity.Critical,
            GqltwsMessage lastMessage = null,
            Exception exception = null,
            string clientProvidedId = null)
            : this(
                  message,
                  code,
                  severity,
                  lastMessage?.Id,
                  lastMessage?.Type,
                  exception,
                  clientProvidedId)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsServerErrorMessage" /> class.
        /// </summary>
        /// <param name="message">The message to put into the generated error.</param>
        /// <param name="code">The custom code to apply to the error message.</param>
        /// <param name="severity">The severity of the error message being generated.</param>
        /// <param name="lastMessageId">The id of the  message received which caused this error. The id will be returned
        /// as a meta data item for reference to the client.</param>
        /// <param name="lastMessageType">The type of the  message received which caused this error. The type will be returned
        /// as a meta data item for reference to the client.</param>
        /// <param name="exception">An internal exception that was thrown that should be carried with this message.</param>
        /// <param name="clientProvidedId">The client provided identifier of the failed operation.</param>
        public GqltwsServerErrorMessage(
            string message,
            string code = Constants.ErrorCodes.DEFAULT,
            GraphMessageSeverity severity = GraphMessageSeverity.Critical,
            string lastMessageId = null,
            GqltwsMessageType? lastMessageType = null,
            Exception exception = null,
            string clientProvidedId = null)
            : base(GqltwsMessageType.ERROR)
        {
            this.Id = clientProvidedId;
            this.Payload = new GraphExecutionMessage(severity, message, code, SourceOrigin.None, exception);

            if (!string.IsNullOrWhiteSpace(lastMessageId))
                this.Payload.MetaData.Add(GqltwsConstants.Messaging.LAST_RECEIVED_MESSAGE_ID, lastMessageId);

            if (lastMessageType.HasValue)
            {
                this.Payload.MetaData.Add(
                    GqltwsConstants.Messaging.LAST_RECEIVED_MESSAGE_TYPE,
                    GqltwsMessageTypeExtensions.Serialize(lastMessageType.Value));
            }
        }
    }
}