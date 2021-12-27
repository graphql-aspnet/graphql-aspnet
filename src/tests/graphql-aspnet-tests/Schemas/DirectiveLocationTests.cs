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
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Directives;
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

                if ((asLong % 2) != 0 && asLong != 1)
                {
                    Assert.Fail($"The directive location {location} (Value: {asLong}) is not a power of two.");
                }

                seenValues.Add(asLong);
            }
        }

        [Test]
        public void AllLocations_HaveAPhase()
        {
            var allValues = Enum.GetValues<DirectiveLocation>();
            var seenValues = new HashSet<long>();

            foreach (DirectiveLocation location in allValues)
            {
                if (location == DirectiveLocation.NONE)
                    continue;

                var dleattribute = location.SingleAttributeOrDefault<DirectiveLifeCycleEventAttribute>();
                Assert.IsNotNull(dleattribute);

                Assert.AreNotEqual(DirectiveLifeCycleEvent.Unknown, dleattribute.LifeCycleEvent);
            }
        }
    }
}