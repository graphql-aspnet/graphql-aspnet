// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Security.SecurityTestData
{
    using Microsoft.AspNetCore.Authorization;

    public class SubscriptionSecureWidget
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}