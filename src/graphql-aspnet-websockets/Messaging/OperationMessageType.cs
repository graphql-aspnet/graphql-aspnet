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
    /// <summary>
    /// <para>An enumeration of the different types of messages carried over a presistent connection.</para>
    /// <para>Reference: https://github.com/apollographql/subscriptions-transport-ws/blob/master/PROTOCOL.md .</para>
    /// </summary>
    public enum OperationMessageType
    {
        UNKNOWN = 0,

        // Client -> Server Messages
        GQL_CONNECTION_INIT = 10,
        GQL_START = 20,
        GQL_STOP = 30,
        GQL_CONNECTION_TERMINATE = 40,

        // Server -> Client Messages
        GQL_CONNECTION_ERROR = 100,
        GQL_CONNECTION_ACK = 110,
        GQL_DATA = 120,
        GQL_ERROR = 130,
        GQL_COMPLETE = 140,
        GQL_CONNECTION_KEEP_ALIVE = 150,
    }
}