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
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class NullableFieldArgumentTestController : GraphIdController
    {
        public enum TestEnum
        {
            Value1,
            Value2,
        }

        [QueryRoot]
        public string ReceiveNullableEnum(TestEnum? obj)
        {
            return obj.HasValue ? "object supplied" : "object null";
        }

        [QueryRoot]
        public string ReceiveNullableEnumWithDefaultValue(TestEnum? obj = null)
        {
            return obj.HasValue ? "object supplied" : "object null";
        }

        [QueryRoot]
        public string ReceiveNullableInt(int? obj)
        {
            return obj.HasValue ? "object supplied" : "object null";
        }

        [QueryRoot]
        public string ReceiveNullableIntWithDefaultValue(int? obj = null)
        {
            return obj.HasValue ? "object supplied" : "object null";
        }

        [QueryRoot]
        public string ReceiveObject(TwoPropertyObject obj)
        {
            return obj != null ? "object supplied" : "object null";
        }

        [QueryRoot]
        public string ReceiveObjectWithDefaultValue(TwoPropertyObject obj = null)
        {
            return obj != null ? "object supplied" : "object null";
        }

#nullable enable
        [QueryRoot]
        public string ReceiveNullableObject(TwoPropertyObject? obj)
        {
            return obj != null ? "object supplied" : "object null";
        }

        [QueryRoot]
        public string ReceiveNullableObjectWithDefaultValue(TwoPropertyObject? obj = null)
        {
            return obj != null ? "object supplied" : "object null";
        }
#nullable disable

        [QueryRoot]
        public string ReceiveNullableStruct(InputObjStruct? obj)
        {
            return obj.HasValue ? "object supplied" : "object null";
        }

        [QueryRoot]
        public string ReceiveNullableStructWithDefaultValue(InputObjStruct? obj = null)
        {
            return obj.HasValue ? "object supplied" : "object null";
        }
    }
}