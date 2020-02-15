// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Subscriptions
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A set of event args representing an action that occured with the attached <see cref="ISubscription{TSchema}"/>.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema the subscription is related to.</typeparam>
    public class ClientSubscriptionEventArgs<TSchema> : EventArgs
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSubscriptionEventArgs{TSchema}"/> class.
        /// </summary>
        /// <param name="clientSubscription">The client subscription.</param>
        public ClientSubscriptionEventArgs(ISubscription<TSchema> clientSubscription)
        {
            this.ClientSubscription = Validation.ThrowIfNullOrReturn(clientSubscription, nameof(clientSubscription));
        }

        /// <summary>
        /// Gets the client subscription being acted on.
        /// </summary>
        /// <value>The client subscription.</value>
        public ISubscription<TSchema> ClientSubscription { get; }
    }
}