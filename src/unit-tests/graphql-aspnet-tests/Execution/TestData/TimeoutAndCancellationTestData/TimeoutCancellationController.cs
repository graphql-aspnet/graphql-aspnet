// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.TimeoutAndCancellationTestData
{
    using System;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class TimeoutCancellationController : GraphController
    {
        public bool MethodInvoked { get; private set; } = false;

        public Action MethodToInvokeDuringControllerAction { get; set; }

        [QueryRoot]
        public async Task<string> TimedOutMethod(int ms)
        {
            this.MethodInvoked = true;
            this.MethodToInvokeDuringControllerAction?.Invoke();

            await Task.Delay(ms);
            return "Task complete";
        }
    }
}