// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.InputVariableExecutionTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class NullableVariableObjectController : GraphController
    {
        public enum TestEnum
        {
            Value1,
            Value2,
        }

        [MutationRoot]
        public TwoPropertyObject CreateWithModelGuidNullId(ModelWithGuid param)
        {
            return new TwoPropertyObject()
            {
                Property1 = param == null ? "no param" : param.Id?.ToString(),
                Property2 = 5,
            };
        }

        [MutationRoot]
        public TwoPropertyObject CreateWithModelWithIntWithDefaultValue(ModelWithIntWithDefaultValue param)
        {
            return new TwoPropertyObject()
            {
                Property1 = param.Id.ToString(),
                Property2 = 5,
            };
        }

        [MutationRoot]
        public TwoPropertyObject CreateWithModelGuid(ModelWithGuid param)
        {
            return new TwoPropertyObject()
            {
                Property1 = param?.Id?.ToString(),
                Property2 = 5,
            };
        }

        [MutationRoot]
        public TwoPropertyObject CreateWithModelInt(ModelWithInt param)
        {
            return new TwoPropertyObject()
            {
                Property1 = "some value",
                Property2 = param.Id ?? -1,
            };
        }

        [MutationRoot]
        public TwoPropertyObject CreateWithNullableId(GraphId? param)
        {
            return new TwoPropertyObject()
            {
                Property1 = param?.Value,
                Property2 = 5,
            };
        }

        [MutationRoot]
        public TwoPropertyObject CreateWithId(GraphId param)
        {
            return new TwoPropertyObject()
            {
                Property1 = param.Value,
                Property2 = 5,
            };
        }

        [MutationRoot]
        public TwoPropertyObject CreateWithEnum(TestEnum? param)
        {
            return new TwoPropertyObject()
            {
                Property1 = param?.ToString(),
                Property2 = 5,
            };
        }

        [MutationRoot]
        public TwoPropertyObject CreateWithNullableInt(int? param)
        {
            return new TwoPropertyObject()
            {
                Property1 = param?.ToString(),
                Property2 = 5,
            };
        }

        [MutationRoot]
        public TwoPropertyObject CreateWithInt(int param)
        {
            return new TwoPropertyObject()
            {
                Property1 = param.ToString(),
                Property2 = 5,
            };
        }


        [MutationRoot]
        public TwoPropertyObject CreateWithIntWithDefaultValue(int param = 33)
        {
            return new TwoPropertyObject()
            {
                Property1 = param.ToString(),
                Property2 = 5,
            };
        }
    }
}