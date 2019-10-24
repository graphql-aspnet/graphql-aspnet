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
    using System;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Logging.Common;
    using Microsoft.Extensions.Logging;
    using NUnit.Framework;

    [TestFixture]
    public class GraphLogEntryTests
    {
        [Test]
        public void BasicPropertyCheck()
        {
            var dt = DateTime.UtcNow;

            var eventId = new EventId(15, "Test Event");
            var entry = new GraphLogEntry(eventId, "Test Message");

            Assert.IsTrue(entry.DateTimeUTC > dt);
            Assert.AreEqual(eventId.Id, entry.EventId);
            Assert.AreEqual(eventId.Name, entry.EventName);
            Assert.AreEqual("Test Message", entry.Message);
            Assert.IsNotNull(entry.LogEntryId);
            Assert.AreEqual("Test Message", entry.ToString());
        }

        [Test]
        public void NoMessageNoEventName_ToStringDefaultsToEventId()
        {
            var entry = new GraphLogEntry(5);
            Assert.AreEqual("Id: 5", entry.ToString());
        }

        [Test]
        public void NoMessage_ToStringDefaultsToEventNameAndId()
        {
            var eventId = new EventId(15, "Test Event");
            var entry = new GraphLogEntry(eventId);
            Assert.AreEqual("Test Event (Id: 15)", entry.ToString());
        }

        [Test]
        public void NoEventId_GeneralIsAssigned()
        {
            var entry = new GraphLogEntry();
            Assert.AreEqual(LogEventIds.General.Id, entry.EventId);
            Assert.AreEqual(LogEventIds.General.Name, entry.EventName);
            Assert.IsNull(entry.Message);
        }
    }
}