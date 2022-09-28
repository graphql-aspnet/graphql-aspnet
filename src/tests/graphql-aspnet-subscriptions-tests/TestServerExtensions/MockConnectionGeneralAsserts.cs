// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.TestServerExtensions
{
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Tests.Framework.Clients;
    using NUnit.Framework;

    public static class MockConnectionGeneralAsserts
    {
        internal static void AssertClientClosedConnection(
            this MockClientConnection connection,
            ConnectionCloseStatus? expectedCloseStatus = null,
            bool dequeue = true)
        {
            Assert.AreEqual(0, connection.ResponseMessageCount, "Messages still in queue.");

            Assert.IsTrue(connection.CloseStatus.HasValue && connection.ClosedForever);
            Assert.AreEqual(MockClientConnectionClosedByStatus.ClosedByClient, connection.ClosedBy);
            if (expectedCloseStatus.HasValue)
                Assert.AreEqual(expectedCloseStatus.Value, connection.CloseStatus.Value);
        }

        internal static void AssertServerClosedConnection(
            this MockClientConnection connection,
            ConnectionCloseStatus? closeStatus = null,
            bool dequeue = true)
        {
            Assert.AreEqual(0, connection.ResponseMessageCount, "Messages still in queue.");

            Assert.IsTrue(connection.CloseStatus.HasValue || connection.ClosedForever);
            Assert.AreEqual(MockClientConnectionClosedByStatus.ClosedByServer, connection.ClosedBy);
            if (closeStatus.HasValue)
                Assert.AreEqual(closeStatus.Value, connection.CloseStatus.Value);
        }

        internal static void AssertConnectionIsOpen(this MockClientConnection connection)
        {
            Assert.AreEqual(ClientConnectionState.Open, connection.State);

            if (connection.CloseStatus.HasValue || connection.ClosedForever)
                Assert.Fail($"Connection was closed with event {connection.CloseStatus.Value}");
        }
    }
}