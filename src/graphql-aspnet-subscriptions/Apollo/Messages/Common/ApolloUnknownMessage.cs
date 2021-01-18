// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Apollo.Messages.Common
{
    /// <summary>
    /// A message representing an unknown message type.
    /// </summary>
    public class ApolloUnknownMessage : ApolloMessage<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloUnknownMessage" /> class.
        /// </summary>
        /// <param name="decodedTexxt">The decoded text, if any, that was recieved from the client that
        /// was not convertable to an apollo message.</param>
        public ApolloUnknownMessage(string decodedTexxt = null)
            : base(ApolloMessageType.UNKNOWN)
        {
            this.Payload = decodedTexxt;
        }
    }
}