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
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.ControllerTestData;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.DirectiveTestData;
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

            Assert.AreEqual(3, Enumerable.Count<KeyValuePair<string, IGraphFieldTemplate>>(template.FieldTemplates));
            Assert.AreEqual(2, Enumerable.Count<IGraphFieldTemplate>(template.Actions));
            Assert.AreEqual(1, Enumerable.Count<IGraphFieldTemplate>(template.Extensions));
            Assert.IsTrue((bool)template.FieldTemplates.ContainsKey($"[mutation]/TwoMethodsDifferentRoots/{nameof(TwoMethodsDifferentRootsController.ActionMethodNoAttributes)}"));
            Assert.IsTrue((bool)template.FieldTemplates.ContainsKey($"[query]/TwoMethodsDifferentRoots/{nameof(TwoMethodsDifferentRootsController.ActionMethodNoAttributes)}"));
            Assert.IsTrue((bool)template.FieldTemplates.ContainsKey($"[type]/TwoPropertyObject/Property3"));
        }

        [Test]
        public void Parse_ReturnArrayOnAction_ParsesCorrectly()
        {
            var expectedTypeExpression = new GraphTypeExpression(
                Constants.Other.DEFAULT_TYPE_EXPRESSION_TYPE_NAME,
                MetaGraphTypes.IsList);

            var template = new GraphControllerTemplate(typeof(ArrayReturnController));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(1, Enumerable.Count<IGraphFieldTemplate>(template.Actions));

            var action = Enumerable.ElementAt<IGraphFieldTemplate>(template.Actions, 0);
            Assert.AreEqual(typeof(string[]), action.DeclaredReturnType);
            Assert.AreEqual(expectedTypeExpression, action.TypeExpression);
        }

        [Test]
        public void Parse_ArrayOnInputParameter_ParsesCorrectly()
        {
            var expectedTypeExpression = new GraphTypeExpression(
                Constants.Other.DEFAULT_TYPE_EXPRESSION_TYPE_NAME,
                MetaGraphTypes.IsList);

            var template = new GraphControllerTemplate(typeof(ArrayInputParamController));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(1, Enumerable.Count<IGraphFieldTemplate>(template.Actions));

            var action = Enumerable.ElementAt<IGraphFieldTemplate>(template.Actions, 0);
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

            Assert.AreEqual(1, Enumerable.Count<IAppliedDirectiveTemplate>(template.AppliedDirectives));

            var appliedDirective = Enumerable.First<IAppliedDirectiveTemplate>(template.AppliedDirectives);
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            Assert.AreEqual(new object[] { 101, "controller arg" }, appliedDirective.Arguments);
        }

        [Test]
        public void Parse_StaticMethodsWithProperGraphFieldDeclarations_AreSkipped()
        {
            var template = new GraphControllerTemplate(typeof(ControllerWithStaticMethod));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(1, Enumerable.Count<IGraphFieldTemplate>(template.Actions));

            var action = Enumerable.First<IGraphFieldTemplate>(template.Actions);
            Assert.AreEqual(nameof(ControllerWithStaticMethod.InstanceMethod), action.Name);
        }
    }
}