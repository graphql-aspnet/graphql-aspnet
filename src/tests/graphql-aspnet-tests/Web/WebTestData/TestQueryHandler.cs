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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Web;
    using Microsoft.AspNetCore.Http;

    public class TestQueryHandler : GraphQueryHandler<GraphSchema>
    {
        public Task TestInvoke(HttpContext context)
        {
            return this.Invoke(context);
        }
    }
}