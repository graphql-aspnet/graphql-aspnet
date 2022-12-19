// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.Exceptions
{
    using System;

    /// <summary>
    /// An exception thrown when the allowed number of concurrent subscription client
    /// connections is reached.
    /// </summary>
    public class MaxConcurrentClientConnectionsReachedException : Exception
    {
    }
}