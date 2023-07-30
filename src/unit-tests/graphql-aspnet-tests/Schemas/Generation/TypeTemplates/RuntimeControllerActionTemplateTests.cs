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
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Common.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class RuntimeControllerActionTemplateTests
    {
        [Test]
        public void MapQuery_PossibleTypesCheck()
        {
            var options = new SchemaOptions<GraphSchema>(new ServiceCollection());
            var field = options.MapQuery("fieldName", (int a) => null as ISinglePropertyObject)
                .AddPossibleTypes(typeof(TwoPropertyObjectV3), typeof(TwoPropertyObject));

            var template = new RuntimeGraphControllerTemplate(field);
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(1, template.Actions.Count());

            var fieldTemplate = template.Actions.First();
            Assert.AreEqual("fieldName", fieldTemplate.Name);
            Assert.AreEqual(0, fieldTemplate.AppliedDirectives.Count());

            var requiredTypes = fieldTemplate.RetrieveRequiredTypes().ToList();
            Assert.AreEqual(4, requiredTypes.Count);
            Assert.IsNotNull(requiredTypes.SingleOrDefault(x => x.Type == typeof(int)));
            Assert.IsNotNull(requiredTypes.SingleOrDefault(x => x.Type == typeof(ISinglePropertyObject)));
            Assert.IsNotNull(requiredTypes.SingleOrDefault(x => x.Type == typeof(TwoPropertyObjectV3)));
            Assert.IsNotNull(requiredTypes.SingleOrDefault(x => x.Type == typeof(TwoPropertyObject)));
        }

        [Test]
        public void MapQuery_InternalNameCheck()
        {
            var options = new SchemaOptions<GraphSchema>(new ServiceCollection());
            var field = options.MapQuery("fieldName", (int a) => 0)
                .WithInternalName("internalFieldName");

            var template = new RuntimeGraphControllerTemplate(field);
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(1, template.Actions.Count());
            Assert.AreEqual("internalFieldName", template.Actions.First().InternalName);
        }
    }
}