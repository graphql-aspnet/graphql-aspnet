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

    [GraphRoute("bottles")]
    public class SodaBottleController : GraphController
    {
        [Query]
        public Task<SodaCan> RetrieveSodaBottles(BottleSearchData bottleData)
        {
            return null;
        }
    }
}