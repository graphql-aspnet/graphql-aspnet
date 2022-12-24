// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Controllers.ActionResults
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Controllers.ActionResults;
    using GraphQL.AspNet.Controllers.InputModel;
    using GraphQL.AspNet.Engine.TypeMakers;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Tests.Controllers.ActionResults.ActuionResultTestData;
    using GraphQL.AspNet.Tests.Framework;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ActionResultTests
    {
        private FieldResolutionContext CreateResolutionContext()
        {
            var server = new TestServerBuilder()
                .AddType<ActionableController>()
                .Build();

            var builder = server.CreateGraphTypeFieldContextBuilder<ActionableController>(
                nameof(ActionableController.DoStuff));
            return builder.CreateResolutionContext();
        }

        [Test]
        public async Task BadRequest_WithModelDictionary_RendersMessageOnResponse()
        {
            var server = new TestServerBuilder().Build();
            var methodTemplate = TemplateHelper.CreateFieldTemplate<ActionableController>(nameof(ActionableController.DoStuff));
            var argTemplate = methodTemplate.Arguments[0];

            var mockedOwner = new Mock<ISchemaItem>();
            var fieldArg = new GraphArgumentMaker(server.Schema).CreateArgument(mockedOwner.Object, argTemplate).Argument;

            // name is marked required, will fail
            var dataItem = new ActionableModelItem()
            {
                Name = null,
                Id = "5",
            };

            var arg = new ExecutionArgument(fieldArg, dataItem);
            var col = new ExecutionArgumentCollection();
            col.Add(arg);

            var generator = new ModelStateGenerator();
            var modelDictionary = generator.CreateStateDictionary(col);

            var actionResult = new BadRequestGraphActionResult(modelDictionary);

            var context = this.CreateResolutionContext();
            await actionResult.CompleteAsync(context);

            Assert.IsNull(context.Result);
            Assert.IsTrue(context.Messages.Any(x => x.Code == Constants.ErrorCodes.BAD_REQUEST));
            Assert.IsTrue(context.Messages.Any(x => x.Code == Constants.ErrorCodes.MODEL_VALIDATION_ERROR));
        }

        [Test]
        public async Task BadRequest_WithMessage_RendersMessageOnResponse()
        {
            var actionResult = new BadRequestGraphActionResult("Test message");

            var context = this.CreateResolutionContext();
            await actionResult.CompleteAsync(context);

            Assert.IsNull(context.Result);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.AreEqual("Test message", context.Messages[0].Message);
            Assert.AreEqual(Constants.ErrorCodes.BAD_REQUEST, context.Messages[0].Code);
        }

        [Test]
        public async Task GraphFieldError_WithException_ReturnsMessageWithException()
        {
            var exception = new Exception("Failure");
            var actionResult = new GraphFieldErrorActionResult("Test Message", "Test Code", exception);

            var context = this.CreateResolutionContext();
            await actionResult.CompleteAsync(context);

            Assert.IsTrue(context.IsCancelled);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.AreEqual("Test Code", context.Messages[0].Code);
            Assert.AreEqual("Test Message", context.Messages[0].Message);
            Assert.AreEqual(exception, context.Messages[0].Exception);
        }

        [Test]
        public async Task GraphFieldError_WithMessageAndCode_ReturnsMessageWithException()
        {
            var actionResult = new GraphFieldErrorActionResult("Test Message", "Test Code");

            var context = this.CreateResolutionContext();
            await actionResult.CompleteAsync(context);

            Assert.IsTrue(context.IsCancelled);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.AreEqual("Test Code", context.Messages[0].Code);
            Assert.AreEqual("Test Message", context.Messages[0].Message);
        }

        [Test]
        public async Task InternalServerError_WithAction_AndException_FriendlyErrorMessage()
        {
            var action = TemplateHelper.CreateFieldTemplate<ActionableController>(nameof(ActionableController.DoStuff)) as IGraphFieldResolverMethod;

            var exception = new Exception("Fail");
            var actionResult = new InternalServerErrorGraphActionResult(action, exception);

            var context = this.CreateResolutionContext();
            await actionResult.CompleteAsync(context);

            Assert.IsTrue(context.IsCancelled);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.UNHANDLED_EXCEPTION, context.Messages[0].Code);
            Assert.IsTrue(context.Messages[0].Message.Contains("An unhandled exception was thrown during"));
            Assert.AreEqual(exception, context.Messages[0].Exception);
        }

        [Test]
        public async Task InternalServerError_WithMessage_AndException_FriendlyErrorMessage()
        {
            var exception = new Exception("Fail");
            var actionResult = new InternalServerErrorGraphActionResult("big fail", exception);

            var context = this.CreateResolutionContext();
            await actionResult.CompleteAsync(context);

            Assert.IsTrue(context.IsCancelled);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.UNHANDLED_EXCEPTION, context.Messages[0].Code);
            Assert.AreEqual("big fail", context.Messages[0].Message);
            Assert.AreEqual(exception, context.Messages[0].Exception);
        }

        [Test]
        public async Task ObjectRetrunedGraph_ReturnsSameItemGivenToIt()
        {
            var testObject = new object();

            var actionResult = new ObjectReturnedGraphActionResult(testObject);

            var context = this.CreateResolutionContext();
            await actionResult.CompleteAsync(context);

            Assert.AreEqual(testObject, context.Result);
            Assert.AreEqual(0, context.Messages.Count);
        }

        [Test]
        public async Task RouteNotFound_ViaGraphAction_YieldsNegativeResult()
        {
            var action = TemplateHelper.CreateFieldTemplate<ActionableController>(nameof(ActionableController.DoStuff)) as IGraphFieldResolverMethod;

            var exception = new Exception("fail");
            var actionResult = new RouteNotFoundGraphActionResult(action, exception);

            var context = this.CreateResolutionContext();
            await actionResult.CompleteAsync(context);

            Assert.IsTrue(context.IsCancelled);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_ROUTE, context.Messages[0].Code);
            Assert.IsTrue(context.Messages[0].Message.Contains(action.Name));
        }

        [Test]
        public async Task RouteNotFound_ViaMessage_YieldsMessageInResult()
        {
            var actionResult = new RouteNotFoundGraphActionResult("The route was not found");

            var context = this.CreateResolutionContext();
            await actionResult.CompleteAsync(context);

            Assert.IsTrue(context.IsCancelled);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_ROUTE, context.Messages[0].Code);
            Assert.AreEqual("The route was not found", context.Messages[0].Message);
        }

        [Test]
        public async Task RouteNotFound_ViaNoParameters_YieldsDefaultResult()
        {
            var actionResult = new RouteNotFoundGraphActionResult();

            var context = this.CreateResolutionContext();
            await actionResult.CompleteAsync(context);

            Assert.IsTrue(context.IsCancelled);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_ROUTE, context.Messages[0].Code);
        }

        [Test]
        public async Task UnauthorizedAccess_YieldsErrorMessageWithResults()
        {
            var actionResult = new UnauthorizedGraphActionResult("Error message for access");

            var context = this.CreateResolutionContext();
            await actionResult.CompleteAsync(context);

            Assert.IsTrue(context.IsCancelled);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.ACCESS_DENIED, context.Messages[0].Code);
            Assert.AreEqual("Error message for access", context.Messages[0].Message);
        }
    }
}