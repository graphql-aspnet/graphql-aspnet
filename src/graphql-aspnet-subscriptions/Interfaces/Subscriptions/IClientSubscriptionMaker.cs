// *************************************************************
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
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An interface for a maker object that can create a client subscription
    /// from a recieved data package.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this maker works with.</typeparam>
    public interface IClientSubscriptionMaker<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Creates a new encapsulated subscription from a graphql data package.
        /// </summary>
        /// <param name="clientProxy">The proxy representing the client requesting the subscription.</param>
        /// <param name="data">The data package recieved from the client.</param>
        /// <param name="clientProvidedId">The provided identifier, from the client, that should be
        /// sent when ever this subscription is provided data.</param>
        /// <returns>Task&lt;ClientSubscription&lt;TSchema&gt;&gt;.</returns>
        Task<ISubscription<TSchema>> Create(ISubscriptionClientProxy clientProxy, GraphQueryData data, string clientProvidedId);
    }
}