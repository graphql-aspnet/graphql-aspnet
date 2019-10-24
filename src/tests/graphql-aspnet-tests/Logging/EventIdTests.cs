// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Logging
{
    using System.Collections.Generic;
    using System.Reflection;
    using GraphQL.AspNet.Logging;
    using Microsoft.Extensions.Logging;
    using NUnit.Framework;

    [TestFixture]
    public class EventIdTests
    {
        [Test]
        public void EnsureAllEventIdsAreUnique()
        {
            var fields = typeof(LogEventIds).GetFields(BindingFlags.Public | BindingFlags.Static);
            var idSet = new HashSet<int>();
            var nameSet = new HashSet<string>();

            foreach (var field in fields)
            {
                var eventId = (EventId)field.GetValue(null);
                if (idSet.Contains(eventId.Id))
                    Assert.Fail($"Event id value {eventId.Id} is used by more than one predefined log event");

                idSet.Add(eventId.Id);

                if (nameSet.Contains(eventId.Name))
                    Assert.Fail($"Event id name '{eventId.Name}' is used by more than one predefined log event");

                nameSet.Add(eventId.Name);
            }
        }
    }
}