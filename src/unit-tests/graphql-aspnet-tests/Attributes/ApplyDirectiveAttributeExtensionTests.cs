// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Attributes
{
    using System.Linq;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Tests.Attributes.ApplyDirectiveAttributeTestData;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class ApplyDirectiveAttributeExtensionTests
    {
        [Test]
        public void AttributesThatInheritFromApplyDirective_AreReadAsDirectives()
        {
            var item = new ApplyDirectiveTestObject();
            var directives = item.GetType().ExtractAppliedDirectiveTemplates(item);

            Assert.AreEqual(1, directives.Count());
            Assert.AreEqual(typeof(TwoPropertyObject), directives.First().DirectiveType);
            Assert.AreEqual("arg1", directives.First().Arguments.First());
        }
    }
}