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
    /// A factory responsible for creating protocol-specific client proxies
    /// overtop of a given persistant connection.
    /// </summary>
    /// <remarks>
    /// This is a library managed interface and should not be implemented
    /// by developers. Perhaps you need <see cref="ISubscriptionClientProxyFactory"/>.
    /// </remarks>
    public interface ISubscriptionServerClientFactory
    {
        /// <summary>
        /// Constructs a new client proxy for the given connection such that the server
        /// can communicate through the proxy to deliver information to the client.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema this factory creates clients for.</typeparam>
        /// <param name="connection">The instance represented the connected client.</param>
        /// <returns>Task&lt;ISubscriptionClientProxy&gt;.</returns>
        Task<ISubscriptionClientProxy<TSchema>> CreateSubscriptionClient<TSchema>(IClientConnection connection)
        where TSchema : class, ISchema;
    }
}