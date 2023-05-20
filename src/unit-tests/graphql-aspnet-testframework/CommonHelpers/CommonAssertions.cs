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
        /// <param name="expectedJson">The expected json.</param>
        /// <param name="actualJson">The actual data from the test.</param>
        /// <param name="messsage">The messsage to show when asserting a failure.</param>
        public static void AreEqualJsonStrings(string expectedJson, string actualJson, string messsage = null)
        {
            var options = new JsonDocumentOptions()
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip,
            };

            var expectedJsonObject = JsonDocument.Parse(expectedJson, options).RootElement;
            var actualJsonObject = JsonDocument.Parse(actualJson, options).RootElement;

            JsonComparer.AreEqualJson(expectedJsonObject, actualJsonObject, messsage);
        }

        /// <summary>
        /// Asserts that the provided lists are the same. Walks internal lists to insure that "Lists of Lists"
        /// are also equal.
        /// </summary>
        /// <param name="expectedOutput">The expected output.</param>
        /// <param name="actualOutput">The actual output.</param>
        public static void AreEqualNestedLists(IList<object> expectedOutput, IList<object> actualOutput)
        {
            GraphQLTestFrameworkProviders
                .Assertions
                .AssertEquality(expectedOutput.Count, actualOutput.Count);

            for (var i = 0; i < expectedOutput.Count; i++)
            {
                var expected = expectedOutput[i];
                var actual = actualOutput[i];

                if (expected is IList<object> expectedChildArray
                    && actual is IList<object> actualChildArray)
                {
                    AreEqualNestedLists(expectedChildArray, actualChildArray);
                }
                else
                {
                    GraphQLTestFrameworkProviders
                        .Assertions
                        .AssertEquality(expected, actual);
                }
            }
        }

        /// <summary>
        /// Analyzes the objects (and properties if they exist) to do a deep comparison and ensure
        /// that the objects are as equivialant as possible.
        /// </summary>
        /// <param name="expectedOutput">The expected output.</param>
        /// <param name="actualOutput">The actual output.</param>
        public static void AreEqualObjects(object expectedOutput, object actualOutput)
        {
            if (expectedOutput == null && actualOutput == null)
                return;

            if (expectedOutput == null && actualOutput != null)
            {
                GraphQLTestFrameworkProviders
                    .Assertions
                    .AssertFailure($"{nameof(actualOutput)} was not null but was expected to be.");
                return;
            }

            if (expectedOutput != null && actualOutput == null)
            {
                GraphQLTestFrameworkProviders
                    .Assertions
                    .AssertFailure($"{nameof(actualOutput)} was null but was expected not to be.");
                return;
            }

            var type = expectedOutput.GetType();
            GraphQLTestFrameworkProviders
                    .Assertions
                    .AssertEquality(type, actualOutput.GetType());

            if (type.IsValueType || type == typeof(string))
            {
                GraphQLTestFrameworkProviders
                    .Assertions
                    .AssertEquality(expectedOutput, actualOutput);
                return;
            }

            var props = expectedOutput.GetType().GetProperties();
            foreach (var prop in props)
            {
                var expectedValue = prop.GetValue(expectedOutput);
                var actualValue = prop.GetValue(actualOutput);

                if (expectedValue == null || actualValue == null)
                {
                    GraphQLTestFrameworkProviders
                        .Assertions
                        .AssertNull(expectedValue);
                    GraphQLTestFrameworkProviders
                        .Assertions
                        .AssertNull(actualValue);
                    continue;
                }

                if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    AreEqualObjects(expectedValue, actualOutput);
                }
                else
                {
                    GraphQLTestFrameworkProviders
                        .Assertions
                        .AssertEquality(expectedValue, actualValue);
                }
            }
        }
    }
}