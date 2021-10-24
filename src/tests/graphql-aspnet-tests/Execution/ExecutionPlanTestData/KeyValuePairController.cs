// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.ExecutionPlanTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class KeyValuePairController : GraphController
    {
        [QueryRoot]
        public IEnumerable<KeyValuePair<string, int>> RetrieveData()
        {
            var list = new List<KeyValuePair<string, int>>();

            list.Add(new KeyValuePair<string, int>("key1", 1));
            list.Add(new KeyValuePair<string, int>("key2", 2));

            return list;
        }
    }
}