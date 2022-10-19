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
    /// A factory used to create client proxy instances that support a specific
    /// messaging protcol.
    /// </summary>
    /// <remarks>
    /// Implementors of this interface should do so expecting
    /// the resultant factor to be used as a singleton.
    /// </remarks>
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
        /// Gets the key value that represents the protocol that client proxies created
        /// from this factory support. Must be a single value. The server
        /// will compare this value to those provided by connected clients to determine
        /// if a proxy instance should be generated.
        /// </summary>
        /// <value>The protocol string.</value>
        string Protocol { get; }
    }
}