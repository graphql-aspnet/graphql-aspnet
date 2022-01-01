﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Controllers
{
    using System.Reflection;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Controllers.ActionResults;
    using GraphQL.AspNet.Execution.InputModel;
    using GraphQL.AspNet.Tests.Controllers.ControllerTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class GraphControllerTests
    {
        [Test]
        public async Task MethodInvocation_EnsureInternalPropertiesAreSet()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphController<InvokableController>()
                .Build();

            var fieldContextBuilder = server.CreateGraphTypeFieldContextBuilder<InvokableController>(
                nameof(InvokableController.AsyncActionMethod));
            fieldContextBuilder.AddInputArgument("arg1", "random string");

            var resolutionContext = fieldContextBuilder.CreateResolutionContext();

            var controller = new InvokableController();
            var result = await controller.InvokeActionAsync(fieldContextBuilder.GraphMethod.Object, resolutionContext);

            Assert.IsNotNull(result);
            Assert.IsTrue(result is ObjectReturnedGraphActionResult);

            Assert.AreEqual(3, controller.CapturedItems.Count);

            Assert.AreEqual(server.ServiceProvider, controller.CapturedItems["RequestServices"]);
            Assert.AreEqual(resolutionContext.Request, controller.CapturedItems["Request"]);
            var modelState = controller.CapturedItems["ModelState"] as InputModelStateDictionary;

            Assert.IsTrue(modelState.IsValid);
            Assert.IsTrue(modelState.ContainsKey("arg1"));
        }

        [Test]
        public async Task MethodInvocation_SyncMethodReturnsObjectNotTask()
        {
            var tester = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphController<InvokableController>()
                .Build();

            var fieldContextBuilder = tester.CreateGraphTypeFieldContextBuilder<InvokableController>(
                nameof(InvokableController.SyncronousActionMethod));
            fieldContextBuilder.AddInputArgument("arg1", "random string");

            var controller = new InvokableController();
            var resolutionContext = fieldContextBuilder.CreateResolutionContext();
            var result = await controller.InvokeActionAsync(fieldContextBuilder.GraphMethod.Object, resolutionContext);

            Assert.IsNotNull(result);
            Assert.IsTrue(result is ObjectReturnedGraphActionResult);
        }

        [Test]
        public async Task MethodInvocation_UnawaitableAsyncMethodFlag_ResultsInInternalError()
        {
            var tester = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphController<InvokableController>()
                .Build();
            var fieldContextBuilder = tester.CreateGraphTypeFieldContextBuilder<InvokableController>(
                nameof(InvokableController.SyncronousActionMethod));
            fieldContextBuilder.AddInputArgument("arg1", "random string");

            fieldContextBuilder.GraphMethod.Setup(x => x.IsAsyncField).Returns(true);

            var controller = new InvokableController();
            var resolutionContext = fieldContextBuilder.CreateResolutionContext();
            var result = await controller.InvokeActionAsync(fieldContextBuilder.GraphMethod.Object, resolutionContext);

            // ensure a server error reslt is generated
            Assert.IsNotNull(result);
            Assert.IsTrue(result is InternalServerErrorGraphActionResult);
        }

        [Test]
        public async Task MethodInvocation_MissingMethodInfo_ReturnsInternalServerError()
        {
            var tester = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphController<InvokableController>()
                .Build();
            var fieldContextBuilder = tester.CreateGraphTypeFieldContextBuilder<InvokableController>(
                nameof(InvokableController.SyncronousActionMethod));
            fieldContextBuilder.AddInputArgument("arg1", "random string");
            fieldContextBuilder.GraphMethod.Setup(x => x.Method).Returns<MethodInfo>(null);

            var controller = new InvokableController();
            var resolutionContext = fieldContextBuilder.CreateResolutionContext();
            var result = await controller.InvokeActionAsync(fieldContextBuilder.GraphMethod.Object, resolutionContext);

            // ensure a server error reslt is generated
            Assert.IsNotNull(result);
            Assert.IsTrue(result is InternalServerErrorGraphActionResult);
        }

        [Test]
        public void MethodInvocation_UserCodeExceptionIsAllowedToThrow()
        {
            var tester = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphController<InvokableController>()
                .Build();
            var fieldContextBuilder = tester.CreateGraphTypeFieldContextBuilder<InvokableController>(
                nameof(InvokableController.AsyncActionMethodToCauseException));

            fieldContextBuilder.AddInputArgument("arg1", "random string");

            var controller = new InvokableController();
            var resolutionContext = fieldContextBuilder.CreateResolutionContext();
            Assert.ThrowsAsync<UserThrownException>(async () => await controller.InvokeActionAsync(fieldContextBuilder.GraphMethod.Object, resolutionContext));
        }

        [Test]
        public async Task NotFoundResult_ViaCustomErrorMessage()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphController<InvokableController>()
                .Build();
            var fieldContextBuilder = server.CreateGraphTypeFieldContextBuilder<InvokableController>(
                nameof(InvokableController.CreateNotFoundResult));

            var controller = new InvokableController();
            var resolutionContext = fieldContextBuilder.CreateResolutionContext();
            var result = await controller.InvokeActionAsync(
                fieldContextBuilder.GraphMethod.Object,
                resolutionContext) as RouteNotFoundGraphActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("it was not found", result.Message);
        }

        [Test]
        public async Task ErrorResult()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<InvokableController>()
                .Build();

            var fieldContextBuilder = server.CreateGraphTypeFieldContextBuilder<InvokableController>(
                nameof(InvokableController.ErrorResult));

            var controller = new InvokableController();
            var resolutionContext = fieldContextBuilder.CreateResolutionContext();
            var result = await controller.InvokeActionAsync(
                fieldContextBuilder.GraphMethod.Object,
                resolutionContext) as GraphFieldErrorActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("an error happened", result.Message);
            Assert.AreEqual("12345", result.Code);
            Assert.IsNotNull(result.Exception);
            Assert.AreEqual("exception text", result.Exception.Message);
        }
    }
}