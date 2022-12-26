﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messaging
{
    /// <summary>
    /// <para>An enumeration of the different types of messages carried over a presistent connection.</para>
    /// <para>Reference: <see href="https://github.com/enisdenjo/graphql-ws/blob/master/PROTOCOL.md" />.</para>
    /// </summary>
    internal enum GqltwsMessageType
    {
        UNKNOWN = 0,

        // Client -> Server Messages
        CONNECTION_INIT = 10,
        SUBSCRIBE = 20,

        // Server -> Client Messages
        CONNECTION_ACK = 110,
        NEXT = 120,
        ERROR = 130,

        // Client <-> Server Messages
        PING = 200,
        PONG = 210,
        COMPLETE = 220,
    }
}