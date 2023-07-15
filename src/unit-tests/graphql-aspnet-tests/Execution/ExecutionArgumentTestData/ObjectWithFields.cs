// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.ExecutionArgumentTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class ObjectWithFields
    {
        [GraphField]
        public int FieldWithInjectedArgument(IServiceForExecutionArgumentTest arg1)
        {
            return arg1.ServiceValue;
        }

        [GraphField]
        public int FieldWithInjectedArgumentWithDefaultValue(IServiceForExecutionArgumentTest arg1 = null)
        {
            return 33;
        }

        [GraphField]
        public int FieldWithNullableSchemaArgument(TwoPropertyObject obj)
        {
            return 33;
        }

        [GraphField]
        public int FieldWithNonNullableSchemaArgumentThatHasDefaultValue(int arg1 = 3)
        {
            return 33;
        }

        [GraphField]
        public int FieldWithNotNullableQueryArgument(int arg1)
        {
            return 33;
        }
    }
}