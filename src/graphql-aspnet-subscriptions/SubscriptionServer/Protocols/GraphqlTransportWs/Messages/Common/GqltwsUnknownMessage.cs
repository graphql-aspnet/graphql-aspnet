// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messages.Common
{
    /// <summary>
    /// A message representing an unknown message type.
    /// </summary>
    public class GqltwsUnknownMessage : GqltwsMessage<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsUnknownMessage" /> class.
        /// </summary>
        /// <param name="decodedText">The decoded text, if any, that was recieved from the client that
        /// was not convertable to an graphql-ws message.</param>
        public GqltwsUnknownMessage(string decodedText = null)
            : base(GqltwsMessageType.UNKNOWN)
        {
            this.Payload = decodedText;
        }
    }
}