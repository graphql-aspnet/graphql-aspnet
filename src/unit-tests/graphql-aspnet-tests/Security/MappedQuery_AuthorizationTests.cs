// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Security
{
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class MappedQuery_AuthorizationTests
    {
        [Test]
        public async Task MappedQuery_AuthorizedUser_AccessAllowed()
        {
            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddGraphQL(o =>
            {
                o.MapQuery("/field1/field2", (int a, int b) =>
                {
                    return a + b;
                })
                .RequireAuthorization("policy1");
            });

            serverBuilder.Authorization.AddClaimPolicy("policy1", "claim1", "claimValue1");
            serverBuilder.UserContext.Authenticate();
            serverBuilder.UserContext.AddUserClaim("claim1", "claimValue1");

            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 5, b: 33 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 38
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task MappedQuery_UnauthorizedUser_AccessDenied()
        {
            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddGraphQL(o =>
            {
                o.MapQuery("/field1/field2", (int a, int b) =>
                {
                    return a + b;
                })
                .RequireAuthorization("policy1");
            });

            serverBuilder.Authorization.AddClaimPolicy("policy1", "claim1", "claimValue1");
            serverBuilder.UserContext.Authenticate();

            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 5, b: 33 } } }");

            var result = await server.ExecuteQuery(builder);

            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, result.Messages[0].Severity);
            Assert.AreEqual("ACCESS_DENIED", result.Messages[0].Code);
        }

        [Test]
        public async Task MappedQuery_UnauthenticatedUser_AccessDenied()
        {
            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddGraphQL(o =>
            {
                o.MapQuery("/field1/field2", (int a, int b) =>
                {
                    return a + b;
                })
                .RequireAuthorization("policy1");
            });

            serverBuilder.Authorization.AddClaimPolicy("policy1", "claim1", "claimValue1");
            serverBuilder.UserContext.Authenticate();

            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 5, b: 33 } } }");

            var result = await server.ExecuteQuery(builder);

            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, result.Messages[0].Severity);
            Assert.AreEqual("ACCESS_DENIED", result.Messages[0].Code);
        }
    }
}