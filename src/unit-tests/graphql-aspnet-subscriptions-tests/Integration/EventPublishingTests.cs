// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Integration
{
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class EventPublishingTests
    {
        [Test]
        public Task EventPublishedToServer_IsTransmittedToListeningClient()
        {
            return Task.CompletedTask;
        }
    }
}