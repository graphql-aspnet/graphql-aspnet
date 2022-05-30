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
    using System.Linq;
    using System.Reflection;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Logging.Common;
    using GraphQL.AspNet.Logging.SubscriptionEvents;
    using GraphQL.AspNet.Schemas;
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

        [Test]
        public void AllLogEntriesCreatableWithNoData_AndHaveUniqueEventIds()
        {
            var coreTypes = typeof(GraphLogEntry).Assembly.GetTypes()
                .Where(x => Validation.IsCastable<GraphLogEntry>(x) && x != typeof(GraphLogEntry))
                .ToList();

            var subscriptionTypes = typeof(SubscriptionEventReceivedLogEntry).Assembly.GetTypes()
                .Where(x => Validation.IsCastable<GraphLogEntry>(x) && x != typeof(GraphLogEntry))
                .ToList();

            var types = coreTypes.Concat(subscriptionTypes).ToList();

            var idSet = new Dictionary<Type, HashSet<int>>();

            foreach (var type in types)
            {
                var typeToInvoke = type;

                if (typeToInvoke.GetGenericArguments().Count() == 1)
                {
                    typeToInvoke = typeToInvoke.MakeGenericType(typeof(GraphSchema));
                }

                var constructors = typeToInvoke.GetConstructors();
                foreach (var constructor in constructors)
                {
                    var arr = new object[constructor.GetParameters().Count()];
                    for (var i = 0; i < arr.Length; i++)
                        arr[i] = null;

                    try
                    {
                        var instance = constructor.Invoke(arr) as GraphLogEntry;
                        if (!idSet.ContainsKey(type))
                            idSet.Add(type, new HashSet<int>());

                        idSet[type].Add(instance.EventId);
                    }
                    catch
                    {
                        Assert.Fail($"Unable to create log entry '{type.Name}' from {arr.Length} parameter constructor.");
                    }
                }
            }

            var differingIds = idSet.Where(x => x.Value.Count != 1).ToList();
            if (differingIds.Count > 0)
            {
                Assert.Fail($"{differingIds[0].Key.Name} has a different event id for different constructors.");
            }

            var duplicateIdsAcrossEntries = new HashSet<int>();
            foreach (var kvp in idSet)
            {
                var id = kvp.Value.First();
                if (duplicateIdsAcrossEntries.Contains(id))
                    Assert.Fail($"{id} is used on more than one log entry.");

                duplicateIdsAcrossEntries.Add(id);
            }
        }
    }
}