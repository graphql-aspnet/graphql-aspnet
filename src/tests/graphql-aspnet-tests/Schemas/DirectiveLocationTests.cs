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
    }
}