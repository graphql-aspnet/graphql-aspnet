// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Web;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// <para>An HttpContext parser that will seperate the request according to the multipart-request-spec
    /// and generate a structured payload that can be executed against the graphql engine.
    /// </para>
    /// <para>
    /// If a non-multi-part-form request is passed to this object, it should automatically perform the
    /// expected fallback behavior and parse the request as a standard graphql request.</para>
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this parser is configured for.</typeparam>
    /// <remarks>Spec: <see href="https://github.com/jaydenseric/graphql-multipart-request-spec" />.</remarks>
    public interface IMultiPartHttpFormPayloadParser<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Parses the contained http context and attempts to build out an appropriate payload
        /// that can be submitted to the graphql engine.
        /// </summary>
        /// <param name="context">The http context to be parsed.</param>
        /// <returns>&lt;MultiPartRequestGraphQLPayload&gt;</returns>
        Task<MultiPartRequestGraphQLPayload> ParseAsync(HttpContext context);
    }
}