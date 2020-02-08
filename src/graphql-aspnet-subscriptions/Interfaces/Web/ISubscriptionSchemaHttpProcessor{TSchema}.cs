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
    /// A processor that can respond to an incoming request for subscription related schema data.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor works for.</typeparam>
    public interface ISubscriptionSchemaHttpProcessor<TSchema> : ISubscriptionSchemaHttpProcessor
        where TSchema : class, ISchema
    {
    }
}