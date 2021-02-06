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

    public class ListController : GraphIdController
    {
        [QueryRoot]
        public IEnumerable<int> CreateIntList()
        {
            return new List<int>() { 5, 10, 15, 20, 25, 30 };
        }

        [QueryRoot]
        public IEnumerable<ListControllerTestEnum> CreateEnumList()
        {
            return new List<ListControllerTestEnum>()
            {
                ListControllerTestEnum.TestValue1,
                ListControllerTestEnum.TestValue2,
            };
        }

        [QueryRoot]
        public IEnumerable<ListControllerTestEnum> CreateEnumListWithInValidValue()
        {
            return new List<ListControllerTestEnum>()
            {
                ListControllerTestEnum.TestValue1,
                (ListControllerTestEnum)(-3),
            };
        }
    }
}