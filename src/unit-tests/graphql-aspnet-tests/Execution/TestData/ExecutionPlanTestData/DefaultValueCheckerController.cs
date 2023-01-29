// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.ExecutionPlanTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class DefaultValueCheckerController : GraphController
    {
        public enum TestEnum
        {
            Value1,
            Value2,
        }

        [QueryRoot]
        public string ReceiveNullableInt(int? obj)
        {
            if (!obj.HasValue)
                return null;

            return obj.Value.ToString();
        }

        [QueryRoot]
        public string ReceiveNullableIntWithDefaultValue(int? obj = 5)
        {
            if (!obj.HasValue)
                return null;

            return obj.Value.ToString();
        }

        [QueryRoot]
        public string ReceiveNullableEnum(TestEnum? obj)
        {
            if (!obj.HasValue)
                return null;

            return obj.Value.ToString();
        }

        [QueryRoot]
        public string ReceiveNullableEnumWithDefaultValue(TestEnum? obj = TestEnum.Value2)
        {
            if (!obj.HasValue)
                return null;

            return obj.Value.ToString();
        }
    }
}