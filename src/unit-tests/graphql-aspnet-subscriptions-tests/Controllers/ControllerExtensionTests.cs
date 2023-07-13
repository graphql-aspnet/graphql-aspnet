// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Controllers.ActionResults;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.SubscriptionServer;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Controllers.ControllerTestData;
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
            var result = await controller.InvokeActionAsync(fieldContextBuilder.ResolverMetaData.Object, resolutionContext);

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
        public void PublishSubEvent_NoDataThrowsException()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddController<InvokableController>()
                .Build();

            var fieldContextBuilder = server.CreateGraphTypeFieldContextBuilder<InvokableController>(
                nameof(InvokableController.MutationRaisesSubEventNoData));

            var arg1Value = "random string";
            fieldContextBuilder.AddInputArgument("arg1", arg1Value);

            var resolutionContext = fieldContextBuilder.CreateResolutionContext();

            var controller = new InvokableController();

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                var result = await controller.InvokeActionAsync(fieldContextBuilder.ResolverMetaData.Object, resolutionContext);
            });
        }

        [Test]
        public async Task PublishSubEvent_ExistingEventCollectionisAppendedTo()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddController<InvokableController>()
                .Build();

            var fieldContextBuilder = server.CreateGraphTypeFieldContextBuilder<InvokableController>(
                nameof(InvokableController.MutationRaisesSubEvent));

            var arg1Value = "random string";
            fieldContextBuilder.AddInputArgument("arg1", arg1Value);

            var resolutionContext = fieldContextBuilder.CreateResolutionContext();
            var eventCollection = new List<SubscriptionEventProxy>();
            resolutionContext.Session.Items.TryAdd(SubscriptionConstants.ContextDataKeys.RAISED_EVENTS_COLLECTION, eventCollection);

            var controller = new InvokableController();
            var result = await controller.InvokeActionAsync(fieldContextBuilder.ResolverMetaData.Object, resolutionContext);

            // ensure the method executed completely
            Assert.IsNotNull(result);
            Assert.IsTrue(result is ObjectReturnedGraphActionResult);

            Assert.AreEqual(1, eventCollection.Count);
        }

        [Test]
        public void PublishSubEvent_UnusableListForSubscriptionEvents_ThrowsException()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddController<InvokableController>()
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
                var result = await controller.InvokeActionAsync(fieldContextBuilder.ResolverMetaData.Object, resolutionContext);
            });
        }

        [Test]
        public void PublishSubEvent_NoEventNameFailsTheResolver_BubblesExceptionUp()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddController<InvokableController>()
                .Build();

            var fieldContextBuilder = server.CreateGraphTypeFieldContextBuilder<InvokableController>(
                nameof(InvokableController.MutationRaiseSubEventWithNoEventName));

            var arg1Value = "random string";
            fieldContextBuilder.AddInputArgument("arg1", arg1Value);

            var resolutionContext = fieldContextBuilder.CreateResolutionContext();
            var controller = new InvokableController();

            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                var result = await controller.InvokeActionAsync(fieldContextBuilder.ResolverMetaData.Object, resolutionContext);
            });
        }
    }
}