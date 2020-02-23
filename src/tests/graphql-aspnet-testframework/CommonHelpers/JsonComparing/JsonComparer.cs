// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework.CommonHelpers.JsonComparing
{
    using System.Text.Json;
    using NUnit.Framework;

    /// <summary>
    /// A set of helper method for comparing json data.
    /// </summary>
    public static class JsonComparer
    {
        /// <summary>
        /// Deteremines if two <see cref="JsonElement"/> are identical or not.
        /// </summary>
        /// <param name="expected">The expected.</param>
        /// <param name="actual">The actual.</param>
        /// <param name="message">The message.</param>
        /// <param name="location">The location.</param>
        /// <param name="assertOnFailure">if set to <c>true</c> [assert on failure].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool AreEqualJson(JsonElement expected, JsonElement actual, string message = null, string location = "<top>", bool assertOnFailure = true)
        {
            if (expected.ValueKind == JsonValueKind.Object
                && actual.ValueKind == JsonValueKind.Object)
            {
                return AreEqualJObject(expected, actual, location, assertOnFailure, message);
            }

            if (expected.ValueKind == JsonValueKind.Array
                && actual.ValueKind == JsonValueKind.Array)
            {
                return AreEqualJArray(expected, actual, location, assertOnFailure, message);
            }

            if (expected.ValueKind == actual.ValueKind)
            {
                return AreEqualJValue(expected, actual, location, assertOnFailure, message);
            }

            if (assertOnFailure)
                Assert.Fail(message ?? $"Object type mismatch. Expected {expected} but got '{actual}'");

            return false;
        }

        private static bool AreEqualJValue(JsonElement expectedValue, JsonElement actualValue, string location, bool assertOnFailure, string message = null)
        {
            var isEqual = expectedValue.GetRawText() == actualValue.GetRawText() || expectedValue.GetRawText() == "\"<anyValue>\"";
            if (!isEqual)
            {
                if (assertOnFailure)
                {
                    var evalue = expectedValue.GetRawText() ?? "<null>";
                    var avalue = actualValue.GetRawText() ?? "<null>";
                    Assert.Fail(message ?? $"Expected Value '{evalue}' but got '{avalue}' at location {location}");
                }
            }

            return isEqual;
        }

        private static bool AreEqualJObject(JsonElement expected, JsonElement actual, string location = "", bool assertOnFailure = true, string message = null)
        {
            var isEqual = true;
            foreach (var prop in expected.EnumerateObject())
            {
                if (!actual.TryGetProperty(prop.Name, out var actualElement))
                {
                    if (assertOnFailure)
                        Assert.Fail(message ?? $"Actual object does not contain a key '{prop.Name}' and was expected to. (Location: '{location}')");

                    return false;
                }

                isEqual = AreEqualJson(prop.Value, actualElement, message, $"{location}.{prop.Name}", assertOnFailure);
                if (!isEqual)
                    break;
            }

            if (isEqual)
            {
                foreach (var prop in actual.EnumerateObject())
                {
                    if (!expected.TryGetProperty(prop.Name, out var expectedElement))
                    {
                        if (assertOnFailure)
                            Assert.Fail(message ?? $"Actual object contains an extra property '{prop.Name}' and was not expected to. (Location: '{location}')");
                        return false;
                    }

                    isEqual = AreEqualJson(expectedElement, prop.Value, message, $"{location}.{prop.Name}", assertOnFailure);
                    if (!isEqual)
                        break;
                }
            }

            return isEqual;
        }

        private static bool AreEqualJArray(JsonElement expected, JsonElement actual, string location = "", bool assertOnFailure = true, string message = null)
        {
            var expectedLength = expected.GetArrayLength();
            var actualLength = actual.GetArrayLength();
            if (expected.GetArrayLength() != actualLength)
            {
                if (assertOnFailure)
                    Assert.Fail(message ?? $"Expected {expectedLength} elements but received {actualLength} (Location: {location}).");
                return false;
            }

            var i = 0;
            foreach (var expectedElement in expected.EnumerateArray())
            {
                bool matchFound = false;
                foreach (var actualElement in actual.EnumerateArray())
                {
                    if (AreEqualJson(expectedElement, actualElement, message, $"{location}[{i}]", false))
                    {
                        matchFound = true;
                        break;
                    }
                }

                if (!matchFound)
                {
                    if (assertOnFailure)
                        Assert.Fail(message ?? $"Expected array element {expectedElement.ToString().Trim()} but no element was found in the actual array (Location: {location}[{i}])");

                    return false;
                }

                i++;
            }

            return true;
        }
    }
}