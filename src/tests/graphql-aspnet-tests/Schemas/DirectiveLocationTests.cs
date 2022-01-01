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
    using GraphQL.AspNet.Schemas.TypeSystem;
    using NUnit.Framework;

    [TestFixture]
    public class DirectiveLocationTests
    {
        [Test]
        public void Labels_AreUniqueValues()
        {
            var allValues = Enum.GetValues<DirectiveLocation>();
            var seenValues = new HashSet<long>();

            foreach (DirectiveLocation location in allValues)
            {
                var asLong = (long)location;
                if (seenValues.Contains(asLong))
                    Assert.Fail($"The directive location value {asLong} (Name: {location}) is declared twice.");

                seenValues.Add(asLong);

                if (location == DirectiveLocation.AllExecutionLocations ||
                    location == DirectiveLocation.AllTypeDeclarationLocations)
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
    }
}