// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.IntrospectionTestData
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    [GraphRoute("sodas")]
    public class SodaCanController : GraphController
    {
        [Query]
        public Task<SodaCan> RetrieveSoda(int id)
        {
            return Task.FromResult(new SodaCan()
            {
                Id = id,
                Brand = "Super Fun Soda!",
            });
        }

        [Query]
        public Task<int> CanCount(int id)
        {
            return Task.FromResult(id);
        }
    }
}