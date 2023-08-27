// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Schemas
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class DirectiveLocationTests
    {
        private static List<object[]> _schemaItemToLocationSource;

        static DirectiveLocationTests()
        {
            _schemaItemToLocationSource = new List<object[]>();
            _schemaItemToLocationSource.Add(
                new object[] { Substitute.For<ISchema>(), DirectiveLocation.SCHEMA });
            _schemaItemToLocationSource.Add(
                new object[] { Substitute.For<IScalarGraphType>(), DirectiveLocation.SCALAR });
            _schemaItemToLocationSource.Add(
                new object[] { Substitute.For<IObjectGraphType>(), DirectiveLocation.OBJECT });
            _schemaItemToLocationSource.Add(
                new object[] { Substitute.For<IGraphField>(), DirectiveLocation.FIELD_DEFINITION });
            _schemaItemToLocationSource.Add(
                new object[] { Substitute.For<IGraphArgument>(), DirectiveLocation.ARGUMENT_DEFINITION });
            _schemaItemToLocationSource.Add(
                new object[] { Substitute.For<IInterfaceGraphType>(), DirectiveLocation.INTERFACE });
            _schemaItemToLocationSource.Add(
                new object[] { Substitute.For<IUnionGraphType>(), DirectiveLocation.UNION });
            _schemaItemToLocationSource.Add(
                new object[] { Substitute.For<IEnumGraphType>(), DirectiveLocation.ENUM });
            _schemaItemToLocationSource.Add(
                new object[] { Substitute.For<IEnumValue>(), DirectiveLocation.ENUM_VALUE });
            _schemaItemToLocationSource.Add(
                new object[] { Substitute.For<IInputObjectGraphType>(), DirectiveLocation.INPUT_OBJECT });
            _schemaItemToLocationSource.Add(
                new object[] { Substitute.For<IInputGraphField>(), DirectiveLocation.INPUT_FIELD_DEFINITION });
        }

        [Test]
        public void Labels_AreUniqueValues()
        {
            var allValues = Enum.GetValues(typeof(DirectiveLocation)).Cast<DirectiveLocation>().ToList();
            var seenValues = new HashSet<long>();

            foreach (DirectiveLocation location in allValues)
            {
                var asLong = (long)location;
                if (seenValues.Contains(asLong))
                    Assert.Fail($"The directive location value {asLong} (Name: {location}) is declared twice.");

                seenValues.Add(asLong);

                if (location == DirectiveLocation.AllExecutionLocations ||
                    location == DirectiveLocation.AllTypeSystemLocations)
                    continue;

                if ((asLong % 2) != 0 && asLong != 1)
                {
                    Assert.Fail($"The directive location {location} (Value: {asLong}) is not a power of two.");
                }
            }
        }

        [TestCase(DirectiveLocation.QUERY, true)]
        [TestCase(DirectiveLocation.MUTATION, true)]
        [TestCase(DirectiveLocation.SUBSCRIPTION, true)]
        [TestCase(DirectiveLocation.FIELD, true)]
        [TestCase(DirectiveLocation.FRAGMENT_DEFINITION, true)]
        [TestCase(DirectiveLocation.FRAGMENT_SPREAD, true)]
        [TestCase(DirectiveLocation.INLINE_FRAGMENT, true)]
        [TestCase(DirectiveLocation.SCHEMA, false)]
        [TestCase(DirectiveLocation.SCALAR, false)]
        [TestCase(DirectiveLocation.OBJECT, false)]
        [TestCase(DirectiveLocation.FIELD_DEFINITION, false)]
        [TestCase(DirectiveLocation.ARGUMENT_DEFINITION, false)]
        [TestCase(DirectiveLocation.INTERFACE, false)]
        [TestCase(DirectiveLocation.UNION, false)]
        [TestCase(DirectiveLocation.ENUM, false)]
        [TestCase(DirectiveLocation.ENUM_VALUE, false)]
        [TestCase(DirectiveLocation.INPUT_OBJECT, false)]
        [TestCase(DirectiveLocation.INPUT_FIELD_DEFINITION, false)]
        public void IsExecutionLocation(DirectiveLocation location, bool expectedResult)
        {
            var result = location.IsExecutionLocation();
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase(DirectiveLocation.QUERY, false)]
        [TestCase(DirectiveLocation.MUTATION, false)]
        [TestCase(DirectiveLocation.SUBSCRIPTION, false)]
        [TestCase(DirectiveLocation.FIELD, false)]
        [TestCase(DirectiveLocation.FRAGMENT_DEFINITION, false)]
        [TestCase(DirectiveLocation.FRAGMENT_SPREAD, false)]
        [TestCase(DirectiveLocation.INLINE_FRAGMENT, false)]
        [TestCase(DirectiveLocation.SCHEMA, true)]
        [TestCase(DirectiveLocation.SCALAR, true)]
        [TestCase(DirectiveLocation.OBJECT, true)]
        [TestCase(DirectiveLocation.FIELD_DEFINITION, true)]
        [TestCase(DirectiveLocation.ARGUMENT_DEFINITION, true)]
        [TestCase(DirectiveLocation.INTERFACE, true)]
        [TestCase(DirectiveLocation.UNION, true)]
        [TestCase(DirectiveLocation.ENUM, true)]
        [TestCase(DirectiveLocation.ENUM_VALUE, true)]
        [TestCase(DirectiveLocation.INPUT_OBJECT, true)]
        [TestCase(DirectiveLocation.INPUT_FIELD_DEFINITION, true)]
        public void IsTypeDeclarationLocation(DirectiveLocation location, bool expectedResult)
        {
            var result = location.IsTypeDeclarationLocation();
            Assert.AreEqual(expectedResult, result);
        }

        [TestCaseSource(nameof(_schemaItemToLocationSource))]
        public void AsDirectiveLocation(ISchemaItem testItem, DirectiveLocation expectedLocation)
        {
            var result = testItem.AsDirectiveLocation();
            Assert.AreEqual(expectedLocation, result);
        }

        [Test]
        public void AsDirectiveLocation_AllSchemaLocationsAccountedFor()
        {
            // sanity test to ensure that every defined type system location
            // is accounted for in AsDirectiveLocation extension
            var allValues = Enum.GetValues(typeof(DirectiveLocation)).Cast<DirectiveLocation>().ToList();
            foreach (DirectiveLocation location in allValues)
            {
                if (location == DirectiveLocation.AllTypeSystemLocations)
                    continue;

                if ((location & DirectiveLocation.AllTypeSystemLocations) == 0)
                    continue;

                if (_schemaItemToLocationSource.All(x => (DirectiveLocation)x[1] != location))
                {
                    Assert.Fail($"The directive location {location} is not accounted for " +
                        $"in the test set for {nameof(DirectiveLocationExtensions.AsDirectiveLocation)}. Ensure " +
                        $"the value is included and that the extension method is updated.");
                }
            }
        }
    }
}