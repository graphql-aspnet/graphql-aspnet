// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Connections
{
    using System;
    using GraphQL.AspNet.Connections.Clients;
    using NUnit.Framework;

    [TestFixture]
    public class ClientConnectionFailureResultTests
    {
        [Test]
        public void GeneralPropertyCheck()
        {
            var ex = new Exception("total failure");
            var result = new ClientConnectionFailureResult(ex, "close message");

            Assert.AreEqual(0, result.Count);
            Assert.AreEqual(ex, result.Exception);
            Assert.IsTrue(result.EndOfMessage);
            Assert.AreEqual("close message", result.CloseStatusDescription);
            Assert.AreEqual(ClientConnectionCloseStatus.InternalServerError, result.CloseStatus);
            Assert.AreEqual(ClientMessageType.Close, result.MessageType);
        }
    }
}