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
            GraphQLSubscriptionServerSettings.MaxConcurrentSubscriptionReceiverCount = randoValue;
            Assert.AreEqual(randoValue, GraphQLSubscriptionServerSettings.MaxConcurrentSubscriptionReceiverCount);
        }

        [Test]
        public void SettingNewMaxConnectedClientCount_UpdatesValue()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();

            var randoValue = _rando.Next();
            GraphQLSubscriptionServerSettings.MaxConnectedClientCount = randoValue;
            Assert.AreEqual(randoValue, GraphQLSubscriptionServerSettings.MaxConnectedClientCount);
        }
    }
}