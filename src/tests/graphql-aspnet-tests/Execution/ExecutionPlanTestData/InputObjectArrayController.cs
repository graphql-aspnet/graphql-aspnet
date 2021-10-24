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
    using System.Linq;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class InputObjectArrayController : GraphController
    {
        [QueryRoot]
        public bool parseArray(TwoPropertyObject[] items)
        {
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i].Property1 != $"key{i + 1}" || items[i].Property2 != i + 1)
                    return false;
            }

            return true;
        }
    }
}