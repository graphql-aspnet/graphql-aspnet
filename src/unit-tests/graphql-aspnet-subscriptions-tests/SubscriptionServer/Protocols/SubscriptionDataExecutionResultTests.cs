// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.SubscriptionServer.Protocols
{
    using GraphQL.AspNet;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.SubscriptionServer;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionDataExecutionResultTests
    {
        [Test]
        public void DuplicateId_PropCheck()
        {
            var result = SubscriptionQueryExecutionResult<GraphSchema>.DuplicateId("abc");

            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, result.Messages[0].Severity);
            Assert.AreEqual(SubscriptionQueryResultType.IdInUse, result.Status);
            Assert.AreEqual(SubscriptionConstants.ErrorCodes.DUPLICATE_MESSAGE_ID, result.Messages[0].Code);
        }

        [Test]
        public void SubscriptionRegsitered_PropCheck()
        {
            var sub = Substitute.For<IGraphMessageCollection>();

            var result = SubscriptionQueryExecutionResult<GraphSchema>.OperationFailure("abc", sub);

            Assert.AreEqual("abc", result.SubscriptionId);
            Assert.IsNull(result.Subscription);
            Assert.AreEqual(SubscriptionQueryResultType.OperationFailure, result.Status);
        }

        [Test]
        public void OperationFailure_PropCheck()
        {
            var result = SubscriptionQueryExecutionResult<GraphSchema>.DuplicateId("abc");

            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, result.Messages[0].Severity);
            Assert.AreEqual(SubscriptionQueryResultType.IdInUse, result.Status);
            Assert.AreEqual(SubscriptionConstants.ErrorCodes.DUPLICATE_MESSAGE_ID, result.Messages[0].Code);
        }

        [Test]
        public void SingleOperationCompleted_PropCheck()
        {
            var queryResult = Substitute.For<IQueryExecutionResult>();

            var result = SubscriptionQueryExecutionResult<GraphSchema>.SingleOperationCompleted("abc", queryResult);

            Assert.AreEqual(queryResult, result.QueryResult);
            Assert.AreEqual(SubscriptionQueryResultType.SingleQueryCompleted, result.Status);
        }
    }
}