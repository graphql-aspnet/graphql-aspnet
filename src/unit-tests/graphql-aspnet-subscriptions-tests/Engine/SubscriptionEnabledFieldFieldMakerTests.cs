// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine
{
    using System.Linq;
    using GraphQL.AspNet.Engine.TypeMakers;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Engine.TestData;
    using Moq;
    using NUnit.Framework;
    using GraphQL.AspNet.Schemas.Generation;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Engine;

    [TestFixture]
    public class SubscriptionEnabledFieldFieldMakerTests
    {
        [Test]
        public void SubscriptionActionField_TransfersDirectives()
        {
            var mockController = new Mock<IGraphControllerTemplate>();
            mockController.Setup(x => x.InternalFullName).Returns(typeof(SubscriptionTestController).FullName);
            mockController.Setup(x => x.InternalName).Returns(typeof(SubscriptionTestController).Name);
            mockController.Setup(x => x.Route).Returns(new SchemaItemPath("path0"));
            mockController.Setup(x => x.Name).Returns("path0");
            mockController.Setup(x => x.ObjectType).Returns(typeof(SubscriptionTestController));

            var methodInfo = typeof(SubscriptionTestController).GetMethod(nameof(SubscriptionTestController.DoSub));
            var actionTemplate = new SubscriptionControllerActionGraphFieldTemplate(mockController.Object, methodInfo);
            actionTemplate.Parse();
            actionTemplate.ValidateOrThrow();

            var schema = new TestServerBuilder().Build().Schema;

            var factory = new SubscriptionEnabledGraphQLTypeMakerFactory<GraphSchema>();
            factory.Initialize(schema);

            var maker = new SubscriptionEnabledGraphFieldMaker(schema,  factory);

            var field = maker.CreateField(actionTemplate).Field;

            Assert.AreEqual(field, field.AppliedDirectives.Parent);
            Assert.AreEqual(1, field.AppliedDirectives.Count);

            var appliedDirective = Enumerable.FirstOrDefault(field.AppliedDirectives);
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            CollectionAssert.AreEqual(new object[] { 99, "sub action arg" }, appliedDirective.ArgumentValues);
        }
    }
}