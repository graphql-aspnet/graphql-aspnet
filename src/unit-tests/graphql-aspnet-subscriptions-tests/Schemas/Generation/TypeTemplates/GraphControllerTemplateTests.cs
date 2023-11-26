// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates
{
    using System.Linq;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.ControllerTestData;
    using NUnit.Framework;

    [TestFixture]
    public class GraphControllerTemplateTests
    {
        [Test]
        public void Parse_SingleSubscriptionRoute_CreatesCorrectActionTemplate()
        {
            var template = new SubscriptionGraphControllerTemplate(typeof(SimpleSubscriptionController)) as GraphControllerTemplate;
            template.Parse();
            template.ValidateOrThrow();

            Assert.IsNotNull(template);

            Assert.AreEqual(1, template.FieldTemplates.Count());
            Assert.AreEqual(1, template.Actions.Count());
            Assert.IsTrue(template.FieldTemplates.Any(x => x.ItemPath.ToString() == $"[subscription]/SimpleSubscription/WidgetWatcher"));
        }
    }
}