// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Internal.Templating
{
    using System.Linq;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.Subscriptions.Tests.Internal.Templating.ActionTestData;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GraphActionTemplateTests
    {
        private ControllerSubscriptionActionGraphFieldTemplate CreateActionTemplate<TControllerType>(string actionName)
            where TControllerType : GraphController
        {
            var mockController = new Mock<IGraphControllerTemplate>();
            mockController.Setup(x => x.InternalFullName).Returns(typeof(TControllerType).Name);
            mockController.Setup(x => x.Route).Returns(new GraphFieldPath("path0"));
            mockController.Setup(x => x.Name).Returns("path0");
            mockController.Setup(x => x.ObjectType).Returns(typeof(TControllerType));

            var methodInfo = typeof(TControllerType).GetMethod(actionName);
            var action = new ControllerSubscriptionActionGraphFieldTemplate(mockController.Object, methodInfo);
            action.Parse();
            action.ValidateOrThrow();

            return action;
        }

        [Test]
        public void ActionTemplate_Parse_BasicPropertySets()
        {
            var methodInfo = typeof(OneMethodSubscriptionController).GetMethod(nameof(OneMethodSubscriptionController.SingleMethod));
            var action = this.CreateActionTemplate<OneMethodSubscriptionController>(nameof(OneMethodSubscriptionController.SingleMethod));

            Assert.AreEqual("SubDescription", action.Description);
            Assert.AreEqual(typeof(TwoPropertyObject), action.SourceObjectType);
            Assert.AreEqual(typeof(OneMethodSubscriptionController), action.Parent.ObjectType);
            Assert.AreEqual(GraphCollection.Subscription, action.Route.RootCollection);
            Assert.AreEqual("[subscription]/path0/path1", action.Route.Path);
            Assert.AreEqual($"{nameof(OneMethodSubscriptionController)}.{nameof(OneMethodSubscriptionController.SingleMethod)}", action.InternalFullName);
            Assert.AreEqual(methodInfo.ReflectedType, ((IGraphMethod)action).Parent.ObjectType);
            Assert.AreEqual("path0", action.Parent.Name);
            Assert.AreEqual(methodInfo, action.Method);
            Assert.AreEqual(1, action.Arguments.Count);
            Assert.IsFalse(action.Route.IsTopLevelField);
            Assert.IsFalse(action.IsAsyncField);
            Assert.IsFalse(action.IsDeprecated);

            Assert.IsNull(action.EventName);
        }

        [Test]
        public void ActionTemplate_Parse_EnsureCustomEventNameIsAssigned()
        {
            var action = this.CreateActionTemplate<SubscriptionMethodController>(nameof(SubscriptionMethodController.SingleMethod));
            Assert.AreEqual("WatchForThings", action.EventName);
        }

        [Test]
        public void ActionTemplate_Parse_ParameterSameAsReturnTypeIsMarkedSource()
        {
            var action = this.CreateActionTemplate<SubscriptionMethodController>(nameof(SubscriptionMethodController.SingleMethod));

            Assert.AreEqual(1, action.Arguments.Count);
            Assert.IsTrue(action.Arguments[0].ArgumentModifiers.HasFlag(GraphArgumentModifiers.Internal));
            Assert.IsTrue(action.Arguments[0].ArgumentModifiers.HasFlag(GraphArgumentModifiers.ParentFieldResult));
        }

        [Test]
        public void ActionTemplate_Parse_ExplicitlyDeclaredSourceIsAttributedCorrectly()
        {
            var action = this.CreateActionTemplate<SubscriptionMethodController>(nameof(SubscriptionMethodController.ExplicitSourceReference));

            Assert.AreEqual(1, action.Arguments.Count);
            Assert.IsTrue(action.Arguments[0].ArgumentModifiers.HasFlag(GraphArgumentModifiers.Internal));
            Assert.IsTrue(action.Arguments[0].ArgumentModifiers.HasFlag(GraphArgumentModifiers.ParentFieldResult));
        }

        [Test]
        public void ActionTemplate_Parse_ExplicitlyDeclaredSourceIsAttributedCorrectly_WhenOtherParamsMatchReturnType()
        {
            var action = this.CreateActionTemplate<SubscriptionMethodController>(nameof(SubscriptionMethodController.DeclaredSourceFirst));

            Assert.AreEqual(2, action.Arguments.Count);

            var sourceDataParam = action.Arguments.SingleOrDefault(x => x.ArgumentModifiers.HasFlag(GraphArgumentModifiers.ParentFieldResult));
            Assert.IsNotNull(sourceDataParam);
            Assert.AreEqual(typeof(TwoPropertyObjectV2), sourceDataParam.DeclaredArgumentType);
        }

        [Test]
        public void ActionTemplate_Parse_ExplicitlyDeclaredSourceIsAttributedCorrectly_WhenOtherParamsMatchReturnTypeAndDeclaredSecond()
        {
            var action = this.CreateActionTemplate<SubscriptionMethodController>(nameof(SubscriptionMethodController.DeclaredSourceSecond));

            Assert.AreEqual(2, action.Arguments.Count);

            var sourceDataParam = action.Arguments.SingleOrDefault(x => x.ArgumentModifiers.HasFlag(GraphArgumentModifiers.ParentFieldResult));
            Assert.IsNotNull(sourceDataParam);
            Assert.AreEqual(typeof(TwoPropertyObjectV2), sourceDataParam.DeclaredArgumentType);
        }

        [Test]
        public void ActionTemplate_Parse_InvalidEventName_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var action = this.CreateActionTemplate<SubscriptionMethodController>(nameof(SubscriptionMethodController.InvalidEventNameMethod));
            });
        }

        [Test]
        public void ActionTemplate_Parse_WhenMethodDoesNotDeclareASourceInputArgument_AndOneCannotBeInferred_ThrowException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var action = this.CreateActionTemplate<SubscriptionMethodController>(nameof(SubscriptionMethodController.MissingInputReferenceObject));
            });
        }

        [Test]
        public void ActionTemplate_Parse_MultipleSourceParams_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var action = this.CreateActionTemplate<SubscriptionMethodController>(nameof(SubscriptionMethodController.MultipleDeclaredSourceParams));
            });
        }
    }
}