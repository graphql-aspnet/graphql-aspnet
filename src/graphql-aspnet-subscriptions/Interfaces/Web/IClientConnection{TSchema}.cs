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
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A <see cref="IClientConnection"/> targeting a specific schema.
    /// </summary>
    /// <typeparam name="TSchema">The type of the t schema.</typeparam>
    public interface IClientConnection<TSchema> : IClientConnection
        where TSchema : class, ISchema
    {
    }
}