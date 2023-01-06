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
    public class ModelWithIntWithDefaultValue
    {
        public ModelWithIntWithDefaultValue()
        {
            this.Id = 98;
        }

        public int Id { get; set; }
    }
}