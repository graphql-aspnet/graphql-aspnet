﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Subscriptions
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// The subscription server component is a protocol-agonistic, internal abstraction between the known
    /// subscription-related operations (such as publishing events) and the method through which they are
    /// invoked. Commonly used to swap out in-process vs. out-of-process subscription management. Typically there is
    /// one subscription server instance (a singleton) per schema per kestral server.
    /// </summary>
    public interface ISubscriptionServer : ISubscriptionEventReceiver
    {
        /// <summary>
        /// Gets an Id that uniquely identifies this server instance.
        /// </summary>
        /// <value>The identifier.</value>
        string Id { get; }
    }
}