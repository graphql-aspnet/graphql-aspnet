// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Web.WebTestData
{
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class ItemInjectorHttpProcessor : DefaultGraphQLHttpProcessor<GraphSchema>
    {
        public ItemInjectorHttpProcessor(GraphSchema schema, IGraphQLRuntime<GraphSchema> runtime, IQueryResponseWriter<GraphSchema> writer, IGraphEventLogger logger = null)
            : base(schema, runtime, writer, logger)
        {
        }

        protected override async Task<IQueryExecutionRequest> CreateQueryRequestAsync(GraphQueryData queryData, CancellationToken cancelToken = default)
        {
            var request = await base.CreateQueryRequestAsync(queryData, cancelToken);
            request.Items.Add("test-key", new TwoPropertyObject()
            {
                Property2 = 3,
            });

            return request;
        }
    }
}