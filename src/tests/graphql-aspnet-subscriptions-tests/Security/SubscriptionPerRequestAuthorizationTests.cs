// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Security
{
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.Subscriptions.Tests.Security.SecurityTestData;
    using GraphQL.Subscriptions.Tests.TestServerExtensions;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionPerRequestAuthorizationTests
    {
        [Test]
        public async Task UnauthorizedSubscriptionRequestDenysCreation()
        {
            var builder = new TestServerBuilder()
               .AddSubscriptionServer()
                .AddGraphType<SecureSubscriptionWidgetController>();

            // define the policy (declared on the controller)
            // to require "role1"  but assign the user "role4"
            builder.Authorization.AddRolePolicy("SecureWidgetPolicy", "role1");
            builder.UserContext.AddUserRole("role4");
            var server = builder.Build();

            var queryBuilder = server.CreateQueryContextBuilder();
            queryBuilder.AddQueryText(
                @"subscription {
                    secureWidgetChanged(nameLike: ""j""){
                        id
                        name
                        description
                    }
                }");

            var result = await server.ExecuteQuery(queryBuilder);

            Assert.IsNotNull(result);
            Assert.IsNull(result.Data);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.ACCESS_DENIED, result.Messages[0].Code);
        }

        [Test]
        public async Task DownlevelSecurityRequirementNotMetDenysTheSubscription()
        {
            var builder = new TestServerBuilder()
               .AddSubscriptionServer()
                .AddGraphType<SecureSubscriptionWidgetController>();

            // define the policy (declared on the controller)
            // to require "role1"  but assign the user "role4"
            builder.Authorization.AddRolePolicy("SecureWidgetPolicy", "role1");
            builder.UserContext.AddUserRole("role4");
            var server = builder.Build();

            // setup a subscription against an insecure endpoint (unsecurewidgetChanged)
            // but request a secure field from the result (secureDate)
            // this should reslt in the subscription being denied
            var queryBuilder = server.CreateQueryContextBuilder();
            queryBuilder.AddQueryText(
                @"subscription {
                    unsecureWidgetChanged(nameLike: ""j""){
                        id
                        name
                        description
                        secureDate
                    }
                }");

            var result = await server.ExecuteQuery(queryBuilder);

            Assert.IsNotNull(result);
            Assert.IsNull(result.Data);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.ACCESS_DENIED, result.Messages[0].Code);
        }
    }
}