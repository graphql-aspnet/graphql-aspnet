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
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    [DirectiveInvocation(DirectiveInvocationPhase.BeforeFieldResolution | DirectiveInvocationPhase.AfterFieldResolution)]
    public class MetaDataShareDirective : GraphDirective
    {
        public static bool FoundInAfterCompletion;

        private static object _objItem;

        [DirectiveLocations(DirectiveLocation.FIELD | DirectiveLocation.FRAGMENT_SPREAD | DirectiveLocation.INLINE_FRAGMENT)]
        public Task<IGraphActionResult> ExecuteFromField(int arg)
        {
            if (this.DirectivePhase == DirectiveInvocationPhase.BeforeFieldResolution)
            {
                _objItem = new object();
                this.Request.Items.TryAdd($"testKey_{arg}", _objItem);
                return this.Ok().AsCompletedTask();
            }

            if (this.DirectivePhase == DirectiveInvocationPhase.AfterFieldResolution)
            {
                if (this.Request.Items.ContainsKey($"testKey_{arg}"))
                {
                    FoundInAfterCompletion = this.Request.Items[$"testKey_{arg}"] == _objItem && _objItem != null;
                }
            }

            return this.Ok().AsCompletedTask();
        }
    }
}