// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Controllers.ActionResults;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.Subscriptions.Tests.Controllers.ControllerTestData;
    using NUnit.Framework;

    [TestFixture]
    public class ControllerExtensionTests
    {
        [Test]
        public async Task PublishSubEvent_PublishesEventWithCorrectData()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphController<InvokableController>()
                .Build();

            var fieldContextBuilder = server.CreateGraphTypeFieldContextBuilder<InvokableController>(
                nameof(InvokableController.MutationRaisesSubEvent));

            var arg1Value = "random string";
            fieldContextBuilder.AddInputArgument("arg1", arg1Value);

            var resolutionContext = fieldContextBuilder.CreateResolutionContext();

            var controller = new InvokableController();
            var result = await controller.InvokeActionAsync(fieldContextBuilder.GraphMethod.Object, resolutionContext);

            // ensure the method executed completely
            Assert.IsNotNull(result);
            Assert.IsTrue(result is ObjectReturnedGraphActionResult);

            // ensure the event collection was created on the context
            Assert.IsTrue(resolutionContext.Session.Items.ContainsKey(SubscriptionConstants.ContextDataKeys.RAISED_EVENTS_COLLECTION));
            var raisedEvents = resolutionContext.Session.Items[SubscriptionConstants.ContextDataKeys.RAISED_EVENTS_COLLECTION]
                as List<SubscriptionEventProxy>;

            // ensure only one event was added
            Assert.AreEqual(1, raisedEvents.Count);
            var eventData = raisedEvents[0];

            // ensure event properties
            Assert.AreEqual("event1", eventData.EventName);
            var obj = eventData.DataObject as TwoPropertyObject;
            Assert.IsNotNull(obj);
            Assert.AreEqual(arg1Value, obj.Property1);
        }

        [Test]
        public async Task PublishSubEvent_NoDataYieldsNoEvent()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphController<InvokableController>()
                .Build();

            var fieldContextBuilder = server.CreateGraphTypeFieldContextBuilder<InvokableController>(
                nameof(InvokableController.MutationRaisesSubEventNoData));

            var arg1Value = "random string";
            fieldContextBuilder.AddInputArgument("arg1", arg1Value);

            var resolutionContext = fieldContextBuilder.CreateResolutionContext();

            var controller = new InvokableController();
            var result = await controller.InvokeActionAsync(fieldContextBuilder.GraphMethod.Object, resolutionContext);

            // ensure the method executed completely
            Assert.IsNotNull(result);
            Assert.IsTrue(result is ObjectReturnedGraphActionResult);

            // ensure the event collection was not created
            Assert.IsFalse(resolutionContext
                .Session
                .Items
                .ContainsKey(SubscriptionConstants.ContextDataKeys.RAISED_EVENTS_COLLECTION));
        }

        [Test]
        public async Task PublishSubEvent_ExistingEventCollectionisAppendedTo()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphController<InvokableController>()
                .Build();

            var fieldContextBuilder = server.CreateGraphTypeFieldContextBuilder<InvokableController>(
                nameof(InvokableController.MutationRaisesSubEvent));

            var arg1Value = "random string";
            fieldContextBuilder.AddInputArgument("arg1", arg1Value);

            var resolutionContext = fieldContextBuilder.CreateResolutionContext();
            var eventCollection = new List<SubscriptionEventProxy>();
            resolutionContext.Session.Items.TryAdd(SubscriptionConstants.ContextDataKeys.RAISED_EVENTS_COLLECTION, eventCollection);

            var controller = new InvokableController();
            var result = await controller.InvokeActionAsync(fieldContextBuilder.GraphMethod.Object, resolutionContext);

            // ensure the method executed completely
            Assert.IsNotNull(result);
            Assert.IsTrue(result is ObjectReturnedGraphActionResult);

            Assert.AreEqual(1, eventCollection.Count);
        }

        [Test]
        public void PublishSubEvent_UnusableListForSubscriptionEvents_ThrowsException()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphController<InvokableController>()
                .Build();

            var fieldContextBuilder = server.CreateGraphTypeFieldContextBuilder<InvokableController>(
                nameof(InvokableController.MutationRaisesSubEvent));

            var arg1Value = "random string";
            fieldContextBuilder.AddInputArgument("arg1", arg1Value);

            var resolutionContext = fieldContextBuilder.CreateResolutionContext();

            // prepopulate with a collection thats not really a collection
            var eventCollection = new TwoPropertyObject();
            resolutionContext.Session.Items.TryAdd(
                SubscriptionConstants.ContextDataKeys.RAISED_EVENTS_COLLECTION,
                eventCollection);

            var controller = new InvokableController();
            Assert.ThrowsAsync<GraphExecutionException>(async () =>
            {
                var result = await controller.InvokeActionAsync(fieldContextBuilder.GraphMethod.Object, resolutionContext);
            });
        }

        [Test]
        public void PublishSubEvent_NoEventNameFailsTheResolver_BubblesExceptionUp()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphController<InvokableController>()
                .Build();

            var fieldContextBuilder = server.CreateGraphTypeFieldContextBuilder<InvokableController>(
                nameof(InvokableController.MutationRaiseSubEventWithNoEventName));

            var arg1Value = "random string";
            fieldContextBuilder.AddInputArgument("arg1", arg1Value);

            var resolutionContext = fieldContextBuilder.CreateResolutionContext();
            var controller = new InvokableController();

            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                var result = await controller.InvokeActionAsync(fieldContextBuilder.GraphMethod.Object, resolutionContext);
            });
        }
    }
}