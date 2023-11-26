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
    public class ServiceForExecutionArgumentTest : IServiceForExecutionArgumentTest
    {
        public ServiceForExecutionArgumentTest()
        {
            this.ServiceValue = 99;
        }

        public int ServiceValue { get; }
    }
}