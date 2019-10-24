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

    public class MetaDataShareDirective : GraphDirective
    {
        public static bool FoundInAfterCompletion;

        private static object _objItem;

        public Task<IGraphActionResult> BeforeFieldResolution(int arg)
        {
            _objItem = new object();
            this.Request.Items.TryAdd($"testKey_{arg}", _objItem);
            return this.Ok().AsCompletedTask();
        }

        public Task<IGraphActionResult> AfterFieldResolution(int arg)
        {
            if (this.Request.Items.ContainsKey($"testKey_{arg}"))
            {
                FoundInAfterCompletion = this.Request.Items[$"testKey_{arg}"] == _objItem && _objItem != null;
            }

            return this.Ok().AsCompletedTask();
        }
    }
}