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
            var server = new TestServerBuilder()
                .AddGraphController<InvokableController>()
                .Build();

            var fieldContextBuilder = server.CreateFieldContextBuilder<InvokableController>(
                nameof(InvokableController.MutationRaisesSubEvent),
                new object());

            var arg1Value = "random string";
            fieldContextBuilder.AddInputArgument("arg1", arg1Value);

            var resolutionContext = fieldContextBuilder.CreateResolutionContext();

            var controller = new InvokableController();
            var result = await controller.InvokeActionAsync(fieldContextBuilder.GraphMethod.Object, resolutionContext);

            // ensure the method executed completely
            Assert.IsNotNull(result);
            Assert.IsTrue(result is ObjectReturnedGraphActionResult);

            // ensure the event collection was created on the context
            Assert.IsTrue(resolutionContext.Request.Items.ContainsKey(SubscriptionConstants.RAISED_EVENTS_COLLECTION_KEY));
            var raisedEvents = resolutionContext.Request.Items[SubscriptionConstants.RAISED_EVENTS_COLLECTION_KEY]
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
            var server = new TestServerBuilder()
                .AddGraphController<InvokableController>()
                .Build();

            var fieldContextBuilder = server.CreateFieldContextBuilder<InvokableController>(
                nameof(InvokableController.MutationRaisesSubEventNoData),
                new object());

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
                .Request
                .Items
                .ContainsKey(SubscriptionConstants.RAISED_EVENTS_COLLECTION_KEY));
        }

        [Test]
        public async Task PublishSubEvent_ExistingEventCollectionisAppendedTo()
        {
            var server = new TestServerBuilder()
                .AddGraphController<InvokableController>()
                .Build();

            var fieldContextBuilder = server.CreateFieldContextBuilder<InvokableController>(
                nameof(InvokableController.MutationRaisesSubEvent),
                new object());

            var arg1Value = "random string";
            fieldContextBuilder.AddInputArgument("arg1", arg1Value);

            var resolutionContext = fieldContextBuilder.CreateResolutionContext();
            var eventCollection = new List<SubscriptionEventProxy>();
            resolutionContext.Request.Items.TryAdd(SubscriptionConstants.RAISED_EVENTS_COLLECTION_KEY, eventCollection);

            var controller = new InvokableController();
            var result = await controller.InvokeActionAsync(fieldContextBuilder.GraphMethod.Object, resolutionContext);

            // ensure the method executed completely
            Assert.IsNotNull(result);
            Assert.IsTrue(result is ObjectReturnedGraphActionResult);

            Assert.AreEqual(1, eventCollection.Count);
        }

        [Test]
        public async Task PublishSubEvent_UnusableListForSubscriptionEvents_ThrowsException()
        {
            var server = new TestServerBuilder()
                .AddGraphController<InvokableController>()
                .Build();

            var fieldContextBuilder = server.CreateFieldContextBuilder<InvokableController>(
                nameof(InvokableController.MutationRaisesSubEvent),
                new object());

            var arg1Value = "random string";
            fieldContextBuilder.AddInputArgument("arg1", arg1Value);

            var resolutionContext = fieldContextBuilder.CreateResolutionContext();

            // prepopulate with a collection thats not really a collection
            var eventCollection = new TwoPropertyObject();
            resolutionContext.Request.Items.TryAdd(SubscriptionConstants.RAISED_EVENTS_COLLECTION_KEY, eventCollection);

            var controller = new InvokableController();
            Assert.ThrowsAsync<GraphExecutionException>(async () =>
            {
                var result = await controller.InvokeActionAsync(fieldContextBuilder.GraphMethod.Object, resolutionContext);
            });

            await Task.CompletedTask;
        }

        [Test]
        public async Task PublishSubEvent_NoEventNameFailsTheResolver_BubblesExceptionUp()
        {
            var server = new TestServerBuilder()
                .AddGraphController<InvokableController>()
                .Build();

            var fieldContextBuilder = server.CreateFieldContextBuilder<InvokableController>(
                nameof(InvokableController.MutationRaiseSubEventWithNoEventName),
                new object());

            var arg1Value = "random string";
            fieldContextBuilder.AddInputArgument("arg1", arg1Value);

            var resolutionContext = fieldContextBuilder.CreateResolutionContext();
            var controller = new InvokableController();

            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                var result = await controller.InvokeActionAsync(fieldContextBuilder.GraphMethod.Object, resolutionContext);
            });

            await Task.CompletedTask;
        }
    }
}