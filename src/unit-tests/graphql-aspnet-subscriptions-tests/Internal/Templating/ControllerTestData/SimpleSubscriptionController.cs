// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Internal.Templating.ControllerTestData
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    public class SimpleSubscriptionController : GraphController
    {
        [Subscription("WidgetWatcher", typeof(TwoPropertyObject), EventName = "WidgetWatcherEvent")]
        [Description("WidgetWatcher Secondary Description")]
        public Task<IGraphActionResult> WatchForWidgets(TwoPropertyObject data, string nameLike = "*")
        {
            if (data.Property1 == nameLike || nameLike == "*")
                return Task.FromResult(this.Ok(data));
            else
                return Task.FromResult(this.Ok(null));
        }
    }
}