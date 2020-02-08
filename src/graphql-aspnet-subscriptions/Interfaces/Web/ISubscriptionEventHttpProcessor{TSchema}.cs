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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A processor that can respond to an incoming request to publish an event to connected clients.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor works for.</typeparam>
    public interface ISubscriptionEventHttpProcessor<TSchema> : ISubscriptionEventHttpProcessor
        where TSchema : class, ISchema
    {
    }
}