// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating
{
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.AspNet.Tests.Internal.Templating.ControllerTestData;
    using GraphQL.AspNet.Tests.Internal.Templating.DirectiveTestData;
    using NUnit.Framework;

    [TestFixture]
    public class ControllerTemplateTests
    {
        [Test]
        public void Parse_GraphRootController_UsesEmptyRoutePath()
        {
            var template = new GraphControllerTemplate(typeof(DeclaredGraphRootController));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(SchemaItemPath.Empty, template.Route);
        }

        [Test]
        public void Parse_MismatchedRouteFragmentConfiguration_ThrowsException()
        {
            var template = new GraphControllerTemplate(typeof(InvalidRouteController));
            template.Parse();

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void Parse_OverloadedMethodsOnDifferentRoots_ParsesCorrectly()
        {
            var template = new GraphControllerTemplate(typeof(TwoMethodsDifferentRootsController));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(3, template.FieldTemplates.Count());
            Assert.AreEqual(2, template.Actions.Count());
            Assert.AreEqual(1, template.Extensions.Count());
            Assert.IsTrue(template.FieldTemplates.Any(x => x.Route.Path == $"[mutation]/TwoMethodsDifferentRoots/{nameof(TwoMethodsDifferentRootsController.ActionMethodNoAttributes)}"));
            Assert.IsTrue(template.FieldTemplates.Any(x => x.Route.Path == $"[query]/TwoMethodsDifferentRoots/{nameof(TwoMethodsDifferentRootsController.ActionMethodNoAttributes)}"));
            Assert.IsTrue(template.FieldTemplates.Any(x => x.Route.Path == $"[type]/TwoPropertyObject/Property3"));
        }

        [Test]
        public void Parse_ReturnArrayOnAction_ParsesCorrectly()
        {
            var expectedTypeExpression = new GraphTypeExpression(
                "String",
                MetaGraphTypes.IsList);

            var template = new GraphControllerTemplate(typeof(ArrayReturnController));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(1, template.Actions.Count());

            var action = template.Actions.ElementAt(0);
            Assert.AreEqual(typeof(string[]), action.DeclaredReturnType);
            Assert.AreEqual(expectedTypeExpression, action.TypeExpression);
        }

        [Test]
        public void Parse_ArrayOnInputParameter_ThrowsException()
        {
            var expectedTypeExpression = new GraphTypeExpression(
                "Input_" + typeof(TwoPropertyObject).FriendlyName(),
                MetaGraphTypes.IsList);

            var template = new GraphControllerTemplate(typeof(ArrayInputParamController));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(1, template.Actions.Count());

            var action = template.Actions.ElementAt(0);
            Assert.AreEqual(typeof(string), action.DeclaredReturnType);
            Assert.AreEqual(1, action.Arguments.Count);
            Assert.AreEqual(expectedTypeExpression, action.Arguments[0].TypeExpression);
        }

        [Test]
        public void Parse_AssignedDirective_IsTemplatized()
        {
            var template = new GraphControllerTemplate(typeof(ControllerWithDirective));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(1, template.AppliedDirectives.Count());

            var appliedDirective = template.AppliedDirectives.First();
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            Assert.AreEqual(new object[] { 101, "controller arg" }, appliedDirective.Arguments);
        }

        [Test]
        public void Parse_InheritedAction_IsIncludedInTheTemplate()
        {
            var template = new GraphControllerTemplate(typeof(ControllerWithInheritedAction));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(2, template.Actions.Count());

            Assert.IsNotNull(template.Actions.Single(x => x.Name.EndsWith(nameof(BaseControllerWithAction.BaseControllerAction))));
            Assert.IsNotNull(template.Actions.Single(x => x.Name.EndsWith(nameof(ControllerWithInheritedAction.ChildControllerAction))));
        }

        [Test]
        public void Parse_BatchExtensionWithCustomNamedObject_HasAppropriateTypeExpression()
        {
            var template = new GraphControllerTemplate(typeof(ControllerWithActionAsTypeExtensionForCustomNamedObject));
            template.Parse();
            template.ValidateOrThrow();

            var field = template.FieldTemplates[0];

            // type expression should be the custom name on the type
            // not the default name
            Assert.AreEqual("[Custom_Named_Object]", field.TypeExpression.ToString());
        }
    }
}