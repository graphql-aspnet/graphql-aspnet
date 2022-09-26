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
    public interface ISubscriptionServerClientFactory
    {
        /// <summary>
        /// Creates a new subscription client proxy for the given connection and protocol.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema this factory creates clients for.</typeparam>
        /// <param name="connection">The underlying connection the client will
        /// monitor.</param>
        /// <returns>Task&lt;ISubscriptionClientProxy&gt;.</returns>
        Task<ISubscriptionClientProxy<TSchema>> CreateSubscriptionClient<TSchema>(IClientConnection connection)
        where TSchema : class, ISchema;
    }
}