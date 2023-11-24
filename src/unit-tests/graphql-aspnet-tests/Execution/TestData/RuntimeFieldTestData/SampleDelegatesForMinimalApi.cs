// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.RuntimeFieldTestData
{
    public class SampleDelegatesForMinimalApi
    {
        public static int StaticMethod(int a, int b)
        {
            return a + b;
        }

        public int InstanceMethod(int a, int b)
        {
            this.InstanceMethodCalls += 1;
            return a + b;
        }

        public int InstanceMethodCalls { get; set; }
    }
}