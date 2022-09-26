// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Common
{
    /// <summary>
    /// A message representing an unknown message type.
    /// </summary>
    public class GQLWSUnknownMessage : GQLWSMessage<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GQLWSUnknownMessage" /> class.
        /// </summary>
        /// <param name="decodedText">The decoded text, if any, that was recieved from the client that
        /// was not convertable to an graphql-ws message.</param>
        public GQLWSUnknownMessage(string decodedText = null)
            : base(GQLWSMessageType.UNKNOWN)
        {
            this.Payload = decodedText;
        }
    }
}