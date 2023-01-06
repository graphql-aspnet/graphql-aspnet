// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.Variables
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class InvokableVariableController : GraphController
    {
        public int TotalInvocations { get; private set; }

        [QueryRoot]
        public int InvokeMethod(int param = 0)
        {
            this.TotalInvocations += 1;
            return -1;
        }
    }
}