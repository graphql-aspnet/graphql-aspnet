// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Web
{
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A <see cref="IClientConnection"/> targeting a specific schema.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this client is connected to.</typeparam>
    public interface IClientConnection<TSchema> : IClientConnection
        where TSchema : class, ISchema
    {
    }
}