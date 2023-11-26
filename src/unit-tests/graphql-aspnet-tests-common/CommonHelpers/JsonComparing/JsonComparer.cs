// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Common.CommonHelpers.JsonComparing
{
    using System.Text.Json;
    using NUnit.Framework;

    /// <summary>
    /// A set of helper method for comparing json data.
    /// </summary>
    public static class JsonComparer
    {
        /// <summary>
        /// A value that, if set as the value of a json property for the expected json, that json property will pass field value matching
        /// validation regardless of its value or null state.
        /// </summary>
        /// <remarks>
        /// This is useful for json validation where dates might be present as
        /// strings allowing you to bypass the exact date/time value. Especially helpful
        /// for error state checks.
        /// </remarks>
        public const string ANY_FIELD_VALUE = "<anyValue>";

        /// <summary>
        /// Deteremines if two <see cref="JsonElement"/> are identical or not.
        /// </summary>
        /// <param name="expected">The expected data to match against.</param>
        /// <param name="actual">The actual data to check.</param>
        /// <param name="message">The message to display when it fails.</param>
        /// <param name="location">The location where the parser is currently checking.</param>
        /// <param name="assertOnFailure">if set to <c>true</c> an Assert statement will be failed when a match fails, otherwise failure silently returns false.</param>
        /// <returns><c>true</c> if the json data  is equal, <c>false</c> otherwise.</returns>
        public static bool AreEqualJson(JsonElement expected, JsonElement actual, string message = null, string location = "<top>", bool assertOnFailure = true)
        {
            var result = AreEqualJsonElements(expected, actual, location);
            if (result.ElementsAreEqual)
                return true;

            if (assertOnFailure)
            {
                Assert.Fail(
                    message
                    ?? result.ErrorMessage
                    ?? $"Json Elements are not equal at location '{result.Location}' but were expected to be");
            }

            return false;
        }

        /// <summary>
        /// Deteremines if two <see cref="JsonElement" /> are identical or not.
        /// </summary>
        /// <param name="expected">The expected data to match against.</param>
        /// <param name="actual">The actual data to check.</param>
        /// <param name="location">The location where the parser is currently checking.</param>
        /// <returns><c>true</c> if the json data  is equal, <c>false</c> otherwise.</returns>
        public static JsonComparrisonResult AreEqualJsonElements(JsonElement expected, JsonElement actual, string location = "<top>")
        {
            if (expected.ValueKind == JsonValueKind.Object
                && actual.ValueKind == JsonValueKind.Object)
                return AreEqualJObject(expected, actual, location);

            if (expected.ValueKind == JsonValueKind.Array
                && actual.ValueKind == JsonValueKind.Array)
                return AreEqualJArray(expected, actual, location);

            if (expected.GetRawText() == "\"" + ANY_FIELD_VALUE + "\"")
                return JsonComparrisonResult.ElementsEqual;

            if (expected.ValueKind == actual.ValueKind)
                return AreEqualJValue(expected, actual, location);

            return JsonComparrisonResult.Failed($"Object type mismatch. Expected {expected} but got '{actual}'");
        }

        private static JsonComparrisonResult AreEqualJValue(JsonElement expectedValue, JsonElement actualValue, string location)
        {
            var isEqual = expectedValue.GetRawText() == actualValue.GetRawText() || expectedValue.GetRawText() == "\"" + ANY_FIELD_VALUE + "\"";
            if (!isEqual)
            {
                var evalue = expectedValue.GetRawText() ?? "<null>";
                var avalue = actualValue.GetRawText() ?? "<null>";

                return JsonComparrisonResult.Failed(
                    $"Expected Value '{evalue}' but got '{avalue}' at location {location}");
            }

            return JsonComparrisonResult.ElementsEqual;
        }

        private static JsonComparrisonResult AreEqualJObject(JsonElement expected, JsonElement actual, string location = "")
        {
            JsonComparrisonResult result = null;
            foreach (var prop in expected.EnumerateObject())
            {
                if (!actual.TryGetProperty(prop.Name, out var actualElement))
                {
                    return JsonComparrisonResult.Failed(
                        $"Actual object does not contain a key '{prop.Name}' " +
                        $"and was expected to. (Location: '{location}')");
                }

                result = AreEqualJsonElements(prop.Value, actualElement, $"{location}.{prop.Name}");
                if (!result.ElementsAreEqual)
                    break;
            }

            if (result == null || result.ElementsAreEqual)
            {
                foreach (var prop in actual.EnumerateObject())
                {
                    if (!expected.TryGetProperty(prop.Name, out var expectedElement))
                    {
                        return JsonComparrisonResult.Failed(
                            $"Actual object contains an extra property '{prop.Name}' " +
                            $"and was not expected to. (Location: '{location}')");
                    }

                    result = AreEqualJsonElements(expectedElement, prop.Value, $"{location}.{prop.Name}");
                    if (!result.ElementsAreEqual)
                        break;
                }
            }

            return result ?? JsonComparrisonResult.ElementsEqual;
        }

        private static JsonComparrisonResult AreEqualJArray(JsonElement expected, JsonElement actual, string location = "")
        {
            var expectedLength = expected.GetArrayLength();
            var actualLength = actual.GetArrayLength();
            if (expected.GetArrayLength() != actualLength)
            {
                return JsonComparrisonResult.Failed(
                    $"Expected {expectedLength} elements but received {actualLength} (Location: {location}).");
            }

            var i = 0;
            foreach (var expectedElement in expected.EnumerateArray())
            {
                bool matchFound = false;
                foreach (var actualElement in actual.EnumerateArray())
                {
                    var result = AreEqualJsonElements(expectedElement, actualElement, $"{location}[{i}]");
                    if (result.ElementsAreEqual)
                    {
                        matchFound = true;
                        break;
                    }
                }

                if (!matchFound)
                {
                    return JsonComparrisonResult.Failed(
                        $"Expected array element {expectedElement.ToString().Trim()} but no element was " +
                        $"found in the actual array (Location: {location}[{i}])");
                }

                i++;
            }

            return JsonComparrisonResult.ElementsEqual;
        }
    }
}