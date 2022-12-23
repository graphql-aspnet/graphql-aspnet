// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.SubscriptionServer.Protocols
{
    using GraphQL.AspNet;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.SubscriptionServer.Protocols.Common;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionDataExecutionResultTests
    {
        [Test]
        public void DuplicateId_PropCheck()
        {
            var result = SubscriptionDataExecutionResult<GraphSchema>.DuplicateId("abc");

            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, result.Messages[0].Severity);
            Assert.AreEqual(SubscriptionOperationResultType.IdInUse, result.Status);
            Assert.AreEqual(SubscriptionConstants.ErrorCodes.DUPLICATE_MESSAGE_ID, result.Messages[0].Code);
        }

        [Test]
        public void SubscriptionRegsitered_PropCheck()
        {
            var sub = new Mock<IGraphMessageCollection>();

            var result = SubscriptionDataExecutionResult<GraphSchema>.OperationFailure("abc", sub.Object);

            Assert.AreEqual("abc", result.SubscriptionId);
            Assert.IsNull(result.Subscription);
            Assert.AreEqual(SubscriptionOperationResultType.OperationFailure, result.Status);
        }

        [Test]
        public void OperationFailure_PropCheck()
        {
            var result = SubscriptionDataExecutionResult<GraphSchema>.DuplicateId("abc");

            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, result.Messages[0].Severity);
            Assert.AreEqual(SubscriptionOperationResultType.IdInUse, result.Status);
            Assert.AreEqual(SubscriptionConstants.ErrorCodes.DUPLICATE_MESSAGE_ID, result.Messages[0].Code);
        }

        [Test]
        public void SingleOperationCompleted_PropCheck()
        {
            var operationResult = new Mock<IQueryOperationResult>();

            var result = SubscriptionDataExecutionResult<GraphSchema>.SingleOperationCompleted("abc", operationResult.Object);

            Assert.AreEqual(operationResult.Object, result.OperationResult);
            Assert.AreEqual(SubscriptionOperationResultType.SingleQueryCompleted, result.Status);
        }
    }
}