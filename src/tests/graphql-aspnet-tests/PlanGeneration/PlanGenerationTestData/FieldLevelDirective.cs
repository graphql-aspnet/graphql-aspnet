// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.PlanGeneration.PlanGenerationTestData
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;

    public class FieldLevelDirective : GraphDirective
    {
        public static int TotalCalls { get; set; }

        public Task<IGraphActionResult> BeforeFieldResolution(int arg)
        {
            TotalCalls += 1;

            return this.Ok().AsCompletedTask();
        }
    }
}