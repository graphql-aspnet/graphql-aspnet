// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests
{
    using System;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    public class SubscriptionServerSettingsTests
    {
        private Random _rando = new Random();

        [Test]
        public void SettingNewConcurrentRecieverCount_UpdatesValue()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();

            var randoValue = _rando.Next();
            SubscriptionServerSettings.MaxConcurrentSubscriptionReceiverCount = randoValue;
            Assert.AreEqual(randoValue, SubscriptionServerSettings.MaxConcurrentSubscriptionReceiverCount);
        }

        [Test]
        public void SettingNewMaxConnectedClientCount_UpdatesValue()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();

            var randoValue = _rando.Next();
            SubscriptionServerSettings.MaxConnectedClientCount = randoValue;
            Assert.AreEqual(randoValue, SubscriptionServerSettings.MaxConnectedClientCount);
        }
    }
}