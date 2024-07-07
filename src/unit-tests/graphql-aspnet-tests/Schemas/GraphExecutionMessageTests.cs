// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Tests.Schemas
{
    using GraphQL.AspNet.Execution;
    using NUnit.Framework;

    [TestFixture]
    public class GraphExecutionMessageTests
    {
        [TestCase("BOB", "BOB", "JOE", "JOE")]
        [TestCase("", "", "", "")]
        [TestCase("     ", "", "   ", "")]
        [TestCase(null, Constants.ErrorCodes.DEFAULT, null, Constants.ErrorCodes.DEFAULT)]
        [TestCase(null, Constants.ErrorCodes.DEFAULT, "custom code   ", "custom code")]
        public void CodeTests(string initialValue, string expectedValue, string updatedValue, string expectedUpdatedValue)
        {
            var message = new GraphExecutionMessage(GraphMessageSeverity.Critical, "message", code: initialValue);
            Assert.AreEqual(expectedValue, message.Code);

            message.Code = updatedValue;
            Assert.AreEqual(expectedUpdatedValue, message.Code);
        }

        [TestCase("BOB", "BOB", "JOIE", "JOIE")]
        [TestCase("", "", "", "")]
        [TestCase("     ", "", "   ", "")]
        [TestCase(null, null, "jane  ", "jane")]
        public void MessageTests(string initialValue, string expectedValue, string updatedValue, string expectedUpdatedValue)
        {
            var message = new GraphExecutionMessage(GraphMessageSeverity.Critical, message: initialValue);
            Assert.AreEqual(expectedValue, message.Message);

            message.Message = updatedValue;
            Assert.AreEqual(expectedUpdatedValue, message.Message);
        }

        [Test]
        public void ExceptionTest()
        {
            var ex = new System.Exception("test");
            var message = new GraphExecutionMessage(
                GraphMessageSeverity.Critical,
                message: "message",
                exception: ex);

            Assert.AreEqual(ex, message.Exception);
        }

        [Test]
        public void ExceptionTest_WithRemoval()
        {
            var ex = new System.Exception("test");
            var message = new GraphExecutionMessage(
                GraphMessageSeverity.Critical,
                message: "message",
                exception: ex);

            Assert.AreEqual(ex, message.Exception);

            message.Exception = null;
            Assert.IsNull(message.Exception);
        }

        [Test]
        public void ExceptionTest_WithReplacement()
        {
            var ex = new System.Exception("test");
            var message = new GraphExecutionMessage(
                GraphMessageSeverity.Critical,
                message: "message",
                exception: ex);

            Assert.AreEqual(ex, message.Exception);

            var ex2 = new System.Exception("test again");
            message.Exception = ex2;
            Assert.AreEqual(ex2, message.Exception);
        }
    }
}
