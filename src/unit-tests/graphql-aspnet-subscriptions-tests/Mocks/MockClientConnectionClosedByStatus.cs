﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Mocks
{
    /// <summary>
    /// A status indicating how a <see cref="MockClientConnection"/> was cloded.
    /// </summary>
    public enum MockClientConnectionClosedByStatus
    {
        NotClosed,

        ClosedByClient,

        ClosedByServer,
    }
}