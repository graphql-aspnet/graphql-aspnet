// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Logging
{
    using System;
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
            // get all the basic event ids of the core library
            var coreEventIds = typeof(LogEventIds).GetFields(BindingFlags.Public | BindingFlags.Static);

            // get all the ids of the additonal subscription events
            var subscriptionEventIds = typeof(SubscriptionLogEventIds).GetFields(BindingFlags.Public | BindingFlags.Static);

            var idSet = new HashSet<int>();
            var nameSet = new HashSet<string>();

            // account for all core events
            foreach (var field in coreEventIds)
            {
                var eventId = (EventId)field.GetValue(null);
                idSet.Add(eventId.Id);
                nameSet.Add(eventId.Name);
            }

            // test that each subscription event doesn't overlap anywhere
            foreach (var field in subscriptionEventIds)
            {
                var eventId = (EventId)field.GetValue(null);
                if (idSet.Contains(eventId.Id))
                    Assert.Fail($"Subscription Event id value {eventId.Id} is used by more than one predefined log event");

                idSet.Add(eventId.Id);

                if (nameSet.Contains(eventId.Name))
                    Assert.Fail($"Subscription Event id name '{eventId.Name}' is used by more than one predefined log event");

                nameSet.Add(eventId.Name);
            }
        }
    }
}