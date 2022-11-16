// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Extensions
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class AspNetExtensionTests
    {
        [Test]
        public async Task RetrieveUserName()
        {
            var builder = new TestServerBuilder();
            var context = builder.UserContext
                .Authenticate("bobSmith")
                .CreateSecurityContext();

            var result = await context.Authenticate();
            var createdUsername = result.User.RetrieveUsername();
            Assert.AreEqual("bobSmith", createdUsername);
        }

        [Test]
        public async Task RetrieveUserName_CustomClaimOverload()
        {
            var builder = new TestServerBuilder();
            var context = builder.UserContext
                .Authenticate("bobSmith")
                .AddUserClaim("fakeClaim2", "janeDoe")
                .CreateSecurityContext();

            var result = await context.Authenticate();
            var createdUsername = result.User.RetrieveUsername("fakeClaim2");
            Assert.AreEqual("janeDoe", createdUsername);
        }

        [Test]
        public void RetrieveUserName_NullUser_YieldsNull()
        {
            var createdUsername = AspNetExtensions.RetrieveUsername(null);
            Assert.IsNull(createdUsername);
        }
    }
}