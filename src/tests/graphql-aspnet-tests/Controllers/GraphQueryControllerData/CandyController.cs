// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Controllers.GraphQueryControllerData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    [GraphRoute("candy")]
    public class CandyController : GraphController
    {
        [Query("count")]
        public int TotalCandles(string name = null)
        {
            return 5;
        }
    }
}