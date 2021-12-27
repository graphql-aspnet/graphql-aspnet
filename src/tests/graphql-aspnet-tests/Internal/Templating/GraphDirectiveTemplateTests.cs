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
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Internal.Templating.DirectiveTestData;
    using NUnit.Framework;

    [TestFixture]
    public class GraphDirectiveTemplateTests
    {
        [Test]
        public void Simpletemplate_AllDefaults_GeneralPropertyCheck()
        {
            var template = new GraphDirectiveTemplate(typeof(SimpleExecutableDirective));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual("SimpleExecutable", template.Name);
            Assert.AreEqual(nameof(SimpleExecutableDirective), template.InternalName);
            Assert.AreEqual(typeof(SimpleExecutableDirective).FriendlyName(true), template.InternalFullName);
            Assert.AreEqual("Simple Description", template.Description);
            Assert.AreEqual(1, template.Methods.Count);
            Assert.IsTrue(template.Locations.HasFlag(DirectiveLocation.FIELD));
            Assert.AreEqual(typeof(SimpleExecutableDirective), template.ObjectType);
            Assert.AreEqual("[directive]/SimpleExecutable", template.Route.Path);
            Assert.AreEqual(DirectiveLocation.FIELD, template.Locations);
            Assert.IsNotNull(template.Methods.FindMethod(DirectiveLifeCycleEvent.BeforeResolution));
        }

        [Test]
        public void InvalidDirective_MethodWithInvalidSignature_ThrowsException()
        {
            var template = new GraphDirectiveTemplate(typeof(InvalidDirective));
            template.Parse();
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void InvalidDirective_NoLocations_ThrowsException()
        {
            var template = new GraphDirectiveTemplate(typeof(NoLocationsDirective));
            template.Parse();
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void InvalidDirective_NoLocationAttribute_ThrowsException()
        {
            var template = new GraphDirectiveTemplate(typeof(NoLocationAttributeDirective));
            template.Parse();
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void InvalidDirective_MismatchedSignatures_ThrowsException()
        {
            var template = new GraphDirectiveTemplate(typeof(MismatchedSignaturesDirective));
            template.Parse();
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void OverlappingDirectiveLocations_AreCountedCorrectlyAndNotDropped()
        {
            var expectedLocations = DirectiveLocation.FIELD | DirectiveLocation.MUTATION |
                DirectiveLocation.ENUM | DirectiveLocation.OBJECT | DirectiveLocation.UNION |
                DirectiveLocation.INPUT_OBJECT | DirectiveLocation.INPUT_FIELD_DEFINITION;

            var template = new GraphDirectiveTemplate(typeof(OverlappingLocationsDirective));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(expectedLocations, template.Locations);
        }

        [Test]
        public void RequiredTypeSystemLocationWithNoMethodDefined_ThrowsException()
        {
            var template = new GraphDirectiveTemplate(typeof(TypeSystemDirectiveWithoutMethod));
            template.Parse();

            try
            {
                template.ValidateOrThrow();
            }
            catch (GraphTypeDeclarationException ex)
            {
                Assert.IsTrue(ex.Message.Contains(Constants.ReservedNames.DIRECTIVE_ALTER_TYPE_SYSTEM_METHOD_NAME));
            }
            catch
            {
                Assert.Fail($"Invalid Exception Thrown. Expected {nameof(GraphTypeDeclarationException)}");
            }
        }

        [Test]
        public void RequiredExecutionLocationWithNoMethodDefined_ThrowsException()
        {
            var template = new GraphDirectiveTemplate(typeof(ExecutionDirectiveWithoutMethod));
            template.Parse();

            try
            {
                template.ValidateOrThrow();
            }
            catch (GraphTypeDeclarationException ex)
            {
                var containsRightMethodName = ex.Message.Contains(Constants.ReservedNames.DIRECTIVE_BEFORE_RESOLUTION_METHOD_NAME)
                    || ex.Message.Contains(Constants.ReservedNames.DIRECTIVE_AFTER_RESOLUTION_METHOD_NAME);

                Assert.True(containsRightMethodName, "Expected execution method names.");
            }
            catch
            {
                Assert.Fail($"Invalid Exception Thrown. Expected {nameof(GraphTypeDeclarationException)}");
            }
        }

        [Test]
        public void RequiredTypeSystemLocationWithMethodDefined_PropertyCheck()
        {
            var template = new GraphDirectiveTemplate(typeof(TypeSystemDirectiveWithMethod));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(1, template.Methods.Count);
            Assert.AreEqual(DirectiveLifeCycleEvent.AlterTypeSystem, template.Methods.First().LifeCycleEvent.Event);

            var method = template.Methods.First();
            Assert.AreEqual(DirectiveLifeCycleEvent.AlterTypeSystem, method.LifeCycleEvent);
            Assert.AreEqual(1, method.Arguments.Count);

            var arg = method.Arguments[0];
            Assert.AreEqual(typeof(ISchemaItem), arg.ObjectType);
        }
    }
}