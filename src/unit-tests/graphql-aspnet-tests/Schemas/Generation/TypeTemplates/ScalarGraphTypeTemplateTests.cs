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
    using GraphQL.AspNet.Directives.Global;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.ScalarTestData;
    using NuGet.Frameworks;
    using NUnit.Framework;

    [TestFixture]
    public class ScalarGraphTypeTemplateTests
    {
        [Test]
        public void ValidScalar_PropertyCheck()
        {
            var template = new ScalarGraphTypeTemplate(typeof(MyTestScalar));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(TypeKind.SCALAR, template.Kind);
            Assert.AreEqual("myScalar", template.Name);
            Assert.AreEqual("[type]/myScalar", template.ItemPath.Path);
            Assert.AreEqual("myScalar Desc", template.Description);
            Assert.IsTrue(template.Publish);
            Assert.AreEqual("My.Test.Scalar", template.InternalName);
            Assert.AreEqual(typeof(MyTestScalar), template.ScalarType);
            Assert.AreEqual(typeof(MyTestObject), template.ObjectType);
            Assert.IsFalse(template.DeclarationRequirements.HasValue);

            Assert.AreEqual(2, template.AppliedDirectives.Count());
            Assert.AreEqual("myDirective", template.AppliedDirectives.First().DirectiveName);
            Assert.IsNull(template.AppliedDirectives.First().DirectiveType);
            Assert.AreEqual(1, template.AppliedDirectives.First().Arguments.Count());
            Assert.AreEqual("arg1", template.AppliedDirectives.First().Arguments.First().ToString());

            Assert.IsNull(template.AppliedDirectives.Skip(1).First().DirectiveName);
            Assert.AreEqual(typeof(SkipDirective), template.AppliedDirectives.Skip(1).First().DirectiveType);
            Assert.AreEqual(1, template.AppliedDirectives.Skip(1).First().Arguments.Count());
            Assert.AreEqual("argA", template.AppliedDirectives.Skip(1).First().Arguments.First().ToString());
        }

        [Test]
        public void InValidScalar_ThrowsExceptionOnValidate()
        {
            var template = new ScalarGraphTypeTemplate(typeof(TwoPropertyObject));
            template.Parse();

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }
    }
}