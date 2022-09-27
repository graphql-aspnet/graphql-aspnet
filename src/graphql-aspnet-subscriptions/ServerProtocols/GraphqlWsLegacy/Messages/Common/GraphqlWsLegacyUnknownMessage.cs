// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.GraphqlWsLegacy.Messages.Common
{
    /// <summary>
    /// A message representing an unknown message type.
    /// </summary>
    public class GraphqlWsLegacyUnknownMessage : GraphqlWsLegacyMessage<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyUnknownMessage" /> class.
        /// </summary>
        /// <param name="decodedTexxt">The decoded text, if any, that was recieved from the client that
        /// was not convertable to an GraphqlWsLegacy message.</param>
        public GraphqlWsLegacyUnknownMessage(string decodedTexxt = null)
            : base(GraphqlWsLegacyMessageType.UNKNOWN)
        {
            this.Payload = decodedTexxt;
        }
    }
}