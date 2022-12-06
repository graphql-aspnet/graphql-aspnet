// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Tests.Execution.TestData.AuthenticatedUserTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class AuthenticatedUserTests
    {
        [Test]
        public async Task WhenAUserIsAuthenticated_ThatUserIsPassedToTheTargetResolver()
        {
            var serverBuilder = new TestServerBuilder()
                    .AddGraphQL(o =>
                    {
                        o.AddType<AuthenticatedController>();
                        o.ResponseOptions.ExposeExceptions = true;
                    });

            serverBuilder.UserContext.AddUserClaim("test-claim", "test-value-5");
            serverBuilder.UserContext.Authenticate();
            serverBuilder.Authorization.AddClaimPolicy("User5", "test-claim", "test-value-5");

            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query { " +
                    "     isUserSupplied " +
                    "}");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""isUserSupplied"" : true
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }
    }
}