// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Messaging
{
    using System;
    using System.Net.WebSockets;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An interface representing an established connection to a client that can process
    /// subscription data from the server.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor is built for.</typeparam>
    public interface ISubscriptionClientProxy<TSchema> : ISubscriptionClientProxy
        where TSchema : class, ISchema
    {
    }
}