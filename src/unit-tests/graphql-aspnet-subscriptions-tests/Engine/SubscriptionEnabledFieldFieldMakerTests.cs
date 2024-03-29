﻿// *************************************************************
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
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionEnabledFieldFieldMakerTests
    {
        [Test]
        public void SubscriptionActionField_TransfersDirectives()
        {
            var mockController = Substitute.For<IGraphControllerTemplate>();
            mockController.InternalFullName.Returns(typeof(SubscriptionTestController).Name);
            mockController.Route.Returns(new SchemaItemPath("path0"));
            mockController.Name.Returns("path0");
            mockController.ObjectType.Returns(typeof(SubscriptionTestController));

            var methodInfo = typeof(SubscriptionTestController).GetMethod(nameof(SubscriptionTestController.DoSub));
            var actionTemplate = new SubscriptionControllerActionGraphFieldTemplate(mockController, methodInfo);
            actionTemplate.Parse();
            actionTemplate.ValidateOrThrow();

            var schema = new TestServerBuilder().Build().Schema;

            var maker = new SubscriptionEnabledGraphFieldMaker(schema);

            var field = maker.CreateField(actionTemplate).Field;

            Assert.AreEqual(field, field.AppliedDirectives.Parent);
            Assert.AreEqual(1, field.AppliedDirectives.Count);

            var appliedDirective = Enumerable.FirstOrDefault(field.AppliedDirectives);
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            CollectionAssert.AreEqual(new object[] { 99, "sub action arg" }, appliedDirective.ArgumentValues);
        }
    }
}