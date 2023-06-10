// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.ServerExtensions.MutlipartRequests.TestData
{
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Engine;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces;

    public class CompatiableMultipartProcessor<TSchema> : MultipartRequestGraphQLHttpProcessor<TSchema>
        where TSchema : class, ISchema
    {
        public CompatiableMultipartProcessor(
            TSchema schema,
            IGraphQLRuntime<TSchema> runtime,
            IQueryResponseWriter<TSchema> writer,
            IMultiPartHttpFormPayloadParser<TSchema> parser,
            IGraphEventLogger logger = null)
            : base(schema, runtime, writer, parser, logger)
        {
        }
    }
}