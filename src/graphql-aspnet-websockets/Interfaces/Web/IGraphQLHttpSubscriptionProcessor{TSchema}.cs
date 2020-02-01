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
    /// A processor acting on a request to perform a subscription operation.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor is built for.</typeparam>
    public interface IGraphQLHttpSubscriptionProcessor<TSchema> : IGraphQLHttpSubscriptionProcessor
        where TSchema : class, ISchema
    {
    }
}