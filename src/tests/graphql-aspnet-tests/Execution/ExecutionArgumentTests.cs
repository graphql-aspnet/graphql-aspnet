// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution
{
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ExecutionArgumentTests
    {
        private ExecutionArgumentCollection CreateArgumentCollection(string key, object value)
        {
            var argSet = new ExecutionArgumentCollection();

            var mockFieldArg = new Mock<IGraphFieldArgument>();
            mockFieldArg.Setup(x => x.ParameterName).Returns(key);

            argSet.Add(new ExecutionArgument(mockFieldArg.Object, value));
            return argSet;
        }

        [Test]
        public void TryGetArgument_WhenArgExists_AndIsCastable_Succeeds()
        {
            var col = CreateArgumentCollection("test1", "string1");

            var success = col.TryGetArgument<string>("test1", out var value);
            Assert.IsTrue(success);
            Assert.AreEqual("string1", value);
        }

        [Test]
        public void TryGetArgument_WhenArgDoesntExists_FailsWithResponse()
        {
            var col = CreateArgumentCollection("test1", "string1");

            var success = col.TryGetArgument<string>("test2", out var value);
            Assert.IsFalse(success);
            Assert.AreEqual(null, value);
        }

        [Test]
        public void TryGetArgument_WhenArgDoesntCast_FailsWithResponse()
        {
            var col = CreateArgumentCollection("test1", "string1");

            var success = col.TryGetArgument<int>("test1", out var value);
            Assert.IsFalse(success);
            Assert.AreEqual(0, value); // default of int
        }
    }
}