// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.ExecutionPlanTestData
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;

    public class CallTestDirective : GraphDirective
    {
        public static int TotalCalls { get; set; }

        public Task<IGraphActionResult> BeforeFieldResolution(int arg)
        {
            TotalCalls += 1;

            return this.Ok().AsCompletedTask();
        }
    }
}