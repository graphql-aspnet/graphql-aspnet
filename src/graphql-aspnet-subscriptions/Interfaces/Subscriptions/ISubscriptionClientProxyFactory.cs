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
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A factory used to create client proxy instances that act as the intermediary
    /// between the subscription service instance and some messaging protocol
    /// supported by the connected client.
    /// </summary>
    public interface ISubscriptionClientProxyFactory
    {
        /// <summary>
        /// Create a new client proxy of the type supported by this factory instance.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema to create the proxy
        /// instance for.</typeparam>
        /// <param name="connection">An underlying connection that the proxy instance
        /// should communicate with.</param>
        /// <returns>Task&lt;ISubscriptionClientProxy&lt;TSchema&gt;&gt;.</returns>
        Task<ISubscriptionClientProxy<TSchema>> CreateClient<TSchema>(IClientConnection connection)
            where TSchema : class, ISchema;

        /// <summary>
        /// Gets the key that represents the protocol that client proxies created
        /// from this factory support. Must be a single value.
        /// </summary>
        /// <value>The protocol string.</value>
        string Protocol { get; }
    }
}