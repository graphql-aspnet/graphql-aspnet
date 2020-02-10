// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Internal.Templating
{
    using System.Linq;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.Subscriptions.Tests.Internal.Templating.ControllerTestData;
    using NUnit.Framework;

    [TestFixture]
    public class GraphControllerTemplateTests
    {
        public IGraphTypeTemplateProvider SubscriptionTemplateProvider => new SubscriptionEnabledTemplateProvider();

        [Test]
        public void Parse_SingleSubscriptionRoute_CreatesCorrectActionTemplate()
        {
            var template = this.SubscriptionTemplateProvider.ParseType<SimpleSubscriptionController>() as GraphControllerTemplate;
            Assert.IsNotNull(template);

            Assert.AreEqual(1, template.FieldTemplates.Count());
            Assert.AreEqual(1, template.Actions.Count());
            Assert.IsTrue(template.FieldTemplates.ContainsKey($"[subscription]/SimpleSubscription/WidgetWatcher"));
        }
    }
}