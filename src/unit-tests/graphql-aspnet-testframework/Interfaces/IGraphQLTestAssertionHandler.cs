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
        /// Called when a test has outright failed.
        /// <remarks>
        /// The test being executed should halt in a failure state upon completion of this method.
        /// Allowing this method to return without halting will produce an indeterminate state.
        /// </remarks>
        /// </summary>
        /// <param name="message">The message indicating what failed.</param>
        void AssertFailure(string message);

        /// <summary>
        /// Asserts the equality of two objects.
        /// </summary>
        /// <remarks>
        /// The test being executed should halt in a failure state if the two objects
        /// are not concidered equal. Allowing this method to return without halting will
        /// produce an indeterminate state.
        /// </remarks>
        /// <param name="expectedObject">The object to compare against.</param>
        /// <param name="actualObject">The actual object to test.</param>
        /// <param name="failureMessage">An optional message to supply to the
        /// target unit testing framework in the event the object is not null.</param>
        void AssertEquality(object expectedObject, object actualObject, string failureMessage = null);

        /// <summary>
        /// Asserts that the target object is null.
        /// </summary>
        /// <remarks>
        /// The test being executed should halt in a failure state
        /// if the provided object is not null. Allowing this method to return without
        /// halting will produce an indeterminate state.
        /// </remarks>
        /// <param name="actualObject">The actual object.</param>
        /// <param name="failureMessage">An optional message to supply to the
        /// target unit testing framework in the event the object is not null.</param>
        void AssertNull(object actualObject, string failureMessage = null);
    }
}