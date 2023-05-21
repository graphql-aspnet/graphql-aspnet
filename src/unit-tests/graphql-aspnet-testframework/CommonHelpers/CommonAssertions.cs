// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework.CommonHelpers
{
    using System.Collections.Generic;
    using System.Text.Json;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers.JsonComparing;

    /// <summary>
    /// Test helper methods to assert various claims about items.
    /// </summary>
    public static class CommonAssertions
    {
        /// <summary>
        /// Converts the actual object into a json string and compares it to the expected string.
        /// Expected json string is normalized to eliminate variations due to whitespace and non-printable characters.
        /// </summary>
        /// <param name="expectedJson">The expected json string.</param>
        /// <param name="actualJson">The actual json string generated in the test.</param>
        /// <param name="failureMessage">The error messsage to show when asserting a failure.</param>
        public static void AreEqualJsonStrings(string expectedJson, string actualJson, string failureMessage = null)
        {
            var options = new JsonDocumentOptions()
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip,
            };

            var expectedJsonObject = JsonDocument.Parse(expectedJson, options).RootElement;
            var actualJsonObject = JsonDocument.Parse(actualJson, options).RootElement;

            var result = JsonComparer.AreEqualJsonElements(expectedJsonObject, actualJsonObject);
            if (result.ElementsAreEqual)
                return;

            GraphQLTestFrameworkProviders
                .Assertions
                .AssertFailure(failureMessage ?? result.ErrorMessage);
        }

        /// <summary>
        /// Asserts that the provided lists are the same. Performs a deep check
        /// to insure that "Lists of Lists" are also equal.
        /// </summary>
        /// <param name="expectedOutput">The expected list of data.</param>
        /// <param name="actualOutput">The actual output from a test.</param>
        /// <param name="failureMessage">The error messsage to show when asserting a failure.</param>
        public static void AreEqualNestedLists(IList<object> expectedOutput, IList<object> actualOutput, string failureMessage = null)
        {
            if (expectedOutput is null && actualOutput is null)
                return;

            if (expectedOutput == null && actualOutput != null)
            {
                GraphQLTestFrameworkProviders
                    .Assertions
                    .AssertFailure(failureMessage ?? $"{nameof(actualOutput)} was not null but was expected to be.");
                return;
            }

            if (expectedOutput != null && actualOutput == null)
            {
                GraphQLTestFrameworkProviders
                    .Assertions
                    .AssertFailure(failureMessage ?? $"{nameof(actualOutput)} was null but was expected not to be.");
                return;
            }

            GraphQLTestFrameworkProviders
                .Assertions
                .AssertEquality(expectedOutput.Count, actualOutput.Count, failureMessage);

            for (var i = 0; i < expectedOutput.Count; i++)
            {
                var expected = expectedOutput[i];
                var actual = actualOutput[i];

                if (expected is IList<object> expectedChildArray
                    && actual is IList<object> actualChildArray)
                {
                    AreEqualNestedLists(expectedChildArray, actualChildArray, failureMessage);
                }
                else
                {
                    GraphQLTestFrameworkProviders
                        .Assertions
                        .AssertEquality(expected, actual, failureMessage);
                }
            }
        }

        /// <summary>
        /// Analyzes the objects and properties, if they exist, to do a deep comparison and ensure
        /// that the objects are as equivialant as possible.
        /// </summary>
        /// <param name="expectedObject">The expected output object.</param>
        /// <param name="actualObject">The actual output object generated in a test.</param>
        /// <param name="failureMessage">The error messsage to show when asserting a failure.</param>
        public static void AreEqualObjects(object expectedObject, object actualObject, string failureMessage = null)
        {
            if (expectedObject == null && actualObject == null)
                return;

            if (expectedObject == null && actualObject != null)
            {
                GraphQLTestFrameworkProviders
                    .Assertions
                    .AssertFailure(failureMessage ?? $"{nameof(actualObject)} was not null but was expected to be.");
                return;
            }

            if (expectedObject != null && actualObject == null)
            {
                GraphQLTestFrameworkProviders
                    .Assertions
                    .AssertFailure(failureMessage ?? $"{nameof(actualObject)} was null but was expected not to be.");
                return;
            }

            var type = expectedObject.GetType();
            GraphQLTestFrameworkProviders
                    .Assertions
                    .AssertEquality(type, actualObject.GetType(), failureMessage);

            if (type.IsValueType || type == typeof(string))
            {
                GraphQLTestFrameworkProviders
                    .Assertions
                    .AssertEquality(expectedObject, actualObject, failureMessage);
                return;
            }

            var props = expectedObject.GetType().GetProperties();
            foreach (var prop in props)
            {
                var expectedValue = prop.GetValue(expectedObject);
                var actualValue = prop.GetValue(actualObject);

                if (expectedValue == null || actualValue == null)
                {
                    GraphQLTestFrameworkProviders
                        .Assertions
                        .AssertNull(expectedValue, failureMessage);
                    GraphQLTestFrameworkProviders
                        .Assertions
                        .AssertNull(actualValue, failureMessage);
                    continue;
                }

                if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    AreEqualObjects(expectedValue, actualObject, failureMessage);
                }
                else
                {
                    GraphQLTestFrameworkProviders
                        .Assertions
                        .AssertEquality(expectedValue, actualValue, failureMessage);
                }
            }
        }
    }
}