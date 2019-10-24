// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.CommonHelpers
{
    using System.Text.Json;
    using GraphQL.AspNet.Tests.CommonHelpers.JsonComparing;
    using NUnit.Framework;

    /// <summary>
    /// Controlled test values to assert that some custom assertion methods are working as intended.
    /// </summary>
    [TestFixture]
    public class JsonComparingTests
    {
        [Test]
        public void AreEqualJson_Objects_SameButDifferentPropertyOrder_AreqEquivilant()
        {
            var expected = "{ \"prop1\" : \"Value1\", \"prop2\": 5}";
            var actual = "{ \"prop2\" : 5, \"prop1\": \"Value1\"}";

            var expectedObject = JsonDocument.Parse(expected).RootElement;
            var actualObject = JsonDocument.Parse(actual).RootElement;

            JsonComparer.AreEqualJson(expectedObject, actualObject);
        }

        [Test]
        public void AreEqualJson_ArraysButDifferentOrder_AreqEquivilant()
        {
            var expected = "[1,2,3,4,5]";
            var actual = "[5,4,3,2,1]";

            var expectedObject = JsonDocument.Parse(expected).RootElement;
            var actualObject = JsonDocument.Parse(actual).RootElement;

            JsonComparer.AreEqualJson(expectedObject, actualObject);
        }

        [Test]
        public void AreEqualJson_ArraysAsPropertyButDifferentOrder_AreqEquivilant()
        {
            var expected = "{ \"prop1\" : [1,2,3,4,5]}";
            var actual = "{ \"prop1\" : [1,2,3,4,5] }";

            var expectedObject = JsonDocument.Parse(expected).RootElement;
            var actualObject = JsonDocument.Parse(actual).RootElement;

            JsonComparer.AreEqualJson(expectedObject, actualObject);
        }

        [Test]
        public void AreEqualJson_ArraysOfObjectsAsPropertyButDifferentOrder_AreqEquivilant()
        {
            var expected = "{ \"prop1\" : [{\"prop3\": \"test\", \"prop4\": 18}, {\"prop3\": \"test5\", \"prop4\": 44}]}";
            var actual = "{ \"prop1\" : [{\"prop3\": \"test5\", \"prop4\": 44}, {\"prop3\": \"test\", \"prop4\": 18}]}";

            var expectedObject = JsonDocument.Parse(expected).RootElement;
            var actualObject = JsonDocument.Parse(actual).RootElement;

            JsonComparer.AreEqualJson(expectedObject, actualObject);
        }
    }
}