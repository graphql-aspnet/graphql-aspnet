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
    public class NonNullInputField
    {
        public NonNullInputField()
        {
            this.Id = 33;
        }

        public int Id { get; set; }
    }
}