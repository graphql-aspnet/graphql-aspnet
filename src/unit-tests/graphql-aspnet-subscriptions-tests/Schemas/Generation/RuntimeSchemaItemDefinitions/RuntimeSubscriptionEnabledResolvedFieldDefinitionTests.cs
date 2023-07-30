// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.RuntimeSchemaItemDefinitions
{
    using System.IO;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Generation.RuntimeSchemaItemDefinitions;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class RuntimeSubscriptionEnabledResolvedFieldDefinitionTests
    {
        [TestCase("field1", "field1")]
        [TestCase("field1/field2", "field1_field2")]
        [TestCase("/field1/field2", "field1_field2")]
        [TestCase("/field1/field2/", "field1_field2")]
        [TestCase("/field1/field2/field3/field4/field5", "field1_field2_field3_field4_field5")]
        [TestCase("field1/field_2_/field_3", "field1_field_2__field_3")]
        public void NoEventNameSupplied_EventNameIsSetToFieldName(string path, string expectedEventName)
        {
            var collection = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(collection);

            var field = new RuntimeSubscriptionEnabledFieldGroupTemplate(options, path);
            var resolvedField = RuntimeSubscriptionEnabledResolvedFieldDefinition.FromFieldTemplate(field);

            Assert.AreEqual(expectedEventName, resolvedField.EventName);
        }

        [Test]
        public void EventNameSupplied_IsSetToProvidedName()
        {
            var collection = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(collection);

            var field = new RuntimeSubscriptionEnabledFieldGroupTemplate(options, "field1");
            var resolvedField = RuntimeSubscriptionEnabledResolvedFieldDefinition.FromFieldTemplate(field);
            resolvedField.EventName = "theEventName";

            Assert.AreEqual("theEventName", resolvedField.EventName);
        }
    }
}