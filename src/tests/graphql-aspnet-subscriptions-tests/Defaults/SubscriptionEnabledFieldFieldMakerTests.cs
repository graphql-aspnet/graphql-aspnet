// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.Subscriptions.Tests.Defaults
{
    using System.Linq;
    using GraphQL.AspNet.Defaults.TypeMakers;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.Subscriptions.Tests.Defaults.TestData;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionEnabledFieldFieldMakerTests
    {
        [Test]
        public void SubscriptionActionField_TransfersDirectives()
        {
            var mockController = new Mock<IGraphControllerTemplate>();
            mockController.Setup(x => x.InternalFullName).Returns(typeof(SubscriptionTestController).Name);
            mockController.Setup(x => x.Route).Returns(new GraphFieldPath("path0"));
            mockController.Setup(x => x.Name).Returns("path0");
            mockController.Setup(x => x.ObjectType).Returns(typeof(SubscriptionTestController));

            var methodInfo = typeof(SubscriptionTestController).GetMethod(nameof(SubscriptionTestController.DoSub));
            var actionTemplate = new ControllerSubscriptionActionGraphFieldTemplate(mockController.Object, methodInfo);
            actionTemplate.Parse();
            actionTemplate.ValidateOrThrow();

            var schema = new TestServerBuilder().Build().Schema;

            var maker = new SubscriptionEnabledGraphFieldMaker(schema);

            var field = maker.CreateField(actionTemplate).Field;

            Assert.AreEqual(field, field.AppliedDirectives.Parent);
            Assert.AreEqual(1, field.AppliedDirectives.Count);

            var appliedDirective = field.AppliedDirectives.FirstOrDefault();
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            CollectionAssert.AreEqual(new object[] { 99, "sub action arg" }, appliedDirective.Arguments);
        }
    }
}