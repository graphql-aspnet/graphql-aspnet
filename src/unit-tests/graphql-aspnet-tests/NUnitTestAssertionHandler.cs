// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests
{
    using GraphQL.AspNet.Tests.Framework.Interfaces;
    using NUnit.Framework;

    public class NUnitTestAssertionHandler : IGraphQLTestAssertionHandler
    {
        /// <inheritdoc />
        public void AssertFailure(string message)
            => Assert.Fail(message);

        /// <inheritdoc />
        public void AssertEquality(object expectedObject, object actualObject, string failureMessage = null)
            => Assert.AreEqual(expectedObject, actualObject, failureMessage);

        /// <inheritdoc />
        public void AssertNull(object actualObject, string failureMessage = null)
            => Assert.IsNull(actualObject, failureMessage);
    }
}