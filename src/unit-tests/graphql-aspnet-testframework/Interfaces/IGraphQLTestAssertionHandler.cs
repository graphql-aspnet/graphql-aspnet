// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework.Interfaces
{
    /// <summary>
    /// A handler that can execute custom logic for various test assertions
    /// targeting a specific framework, such as NUnit or XUnit.
    /// </summary>
    public interface IGraphQLTestAssertionHandler
    {
        /// <summary>
        /// Called when a test has outright failed. Testing should halt upon completion of this
        /// method.
        /// </summary>
        /// <param name="message">The message indicating what failed.</param>
        void AssertFailure(string message);

        /// <summary>
        /// Asserts the equality of two objects. The testing should halt in a failure state
        /// if the two objects are not concidered equal.
        /// </summary>
        /// <param name="expectedObject">The object to compare against.</param>
        /// <param name="actualObject">The actual object to test.</param>
        /// <param name="failureMessage">An optional message to supply to the
        /// target unit testing framework in the event the object is not null.</param>
        void AssertEquality(object expectedObject, object actualObject, string failureMessage = null);

        /// <summary>
        /// Asserts that the target object is null. Testing should halt in a failure
        /// state of the provided object is not null.
        /// </summary>
        /// <param name="actualObject">The actual object.</param>
        /// <param name="failureMessage">An optional message to supply to the
        /// target unit testing framework in the event the object is not null.</param>
        void AssertNull(object actualObject, string failureMessage = null);
    }
}