// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Apollo.Messages
{
    /// <summary>
    /// <para>An enumeration of the different types of messages carried over a presistent connection.</para>
    /// <para>Reference: https://github.com/apollographql/subscriptions-transport-ws/blob/master/PROTOCOL.md .</para>
    /// </summary>
    public enum ApolloMessageType
    {
        UNKNOWN = 0,

        // Client -> Server Messages
        CONNECTION_INIT = 10,
        START = 20,
        STOP = 30,
        CONNECTION_TERMINATE = 40,

        // Server -> Client Messages
        CONNECTION_ERROR = 100,
        CONNECTION_ACK = 110,
        DATA = 120,
        ERROR = 130,
        COMPLETE = 140,
        CONNECTION_KEEP_ALIVE = 150,
    }
}