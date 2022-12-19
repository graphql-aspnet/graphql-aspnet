﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.ServerMessages
{
    using System;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.Common;

    /// <summary>
    /// An GraphqlWsLegacy specific message containing one error that occured outside of any
    /// given graph query execution.
    /// </summary>
    public class GraphqlWsLegacyServerErrorMessage : GraphqlWsLegacyMessage<IGraphMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyServerErrorMessage" /> class.
        /// </summary>
        /// <param name="message">The message to put into the generated error.</param>
        /// <param name="lastMessage">The last message received by the server, generally the message
        /// that resulted in this error being generated.</param>
        /// <param name="clientProvidedId">The client provided identifier of the failed operation.</param>
        public GraphqlWsLegacyServerErrorMessage(
            IGraphMessage message,
            GraphqlWsLegacyMessage lastMessage = null,
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
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyServerErrorMessage" /> class.
        /// </summary>
        /// <param name="message">The message to put into the generated error.</param>
        /// <param name="code">The custom code to apply to the error message.</param>
        /// <param name="severity">The severity of the error message being generated.</param>
        /// <param name="lastMessage">The last message received by the server, generally the message
        /// that resulted in this error being generated.</param>
        /// <param name="exception">An internal exception that was thrown that should be carried with this message.</param>
        /// <param name="clientProvidedId">The client provided identifier of the failed operation.</param>
        public GraphqlWsLegacyServerErrorMessage(
            string message,
            string code = Constants.ErrorCodes.DEFAULT,
            GraphMessageSeverity severity = GraphMessageSeverity.Critical,
            GraphqlWsLegacyMessage lastMessage = null,
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
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyServerErrorMessage" /> class.
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
        public GraphqlWsLegacyServerErrorMessage(
            string message,
            string code = Constants.ErrorCodes.DEFAULT,
            GraphMessageSeverity severity = GraphMessageSeverity.Critical,
            string lastMessageId = null,
            GraphqlWsLegacyMessageType? lastMessageType = null,
            Exception exception = null,
            string clientProvidedId = null)
            : base(GraphqlWsLegacyMessageType.ERROR)
        {
            this.Id = clientProvidedId;
            this.Payload = new GraphExecutionMessage(severity, message, code, SourceOrigin.None, exception);

            if (!string.IsNullOrWhiteSpace(lastMessageId))
                this.Payload.MetaData.Add(GraphqlWsLegacyConstants.Messaging.LAST_RECEIVED_MESSAGE_ID, lastMessageId);

            if (lastMessageType.HasValue)
            {
                this.Payload.MetaData.Add(
                    GraphqlWsLegacyConstants.Messaging.LAST_RECEIVED_MESSAGE_TYPE,
                    GraphqlWsLegacyMessageTypeExtensions.Serialize(lastMessageType.Value));
            }
        }
    }
}