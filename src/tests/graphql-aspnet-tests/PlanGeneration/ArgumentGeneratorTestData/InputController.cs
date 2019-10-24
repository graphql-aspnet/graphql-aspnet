// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.PlanGeneration.ArgumentGeneratorTestData
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.CommonHelpers;

    public class InputController : GraphController
    {
        [Query]
        public string FetchString(int arg1, int? arg2 = 15)
        {
            return (arg1 + arg2).ToString();
        }

        [Query]
        public string FetchArrayTotal(IEnumerable<int> arg3)
        {
            return arg3.Sum().ToString();
        }

        [Query]
        public string FetchComplexValue(TwoPropertyObject arg4)
        {
            return string.Empty;
        }
    }
}