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
    using NUnit.Framework;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Tests.Framework;

    [TestFixture]
    public class AspNetExtensionTests
    {
        [Test]
        public void RetrieveUserName()
        {
            var builder = new TestServerBuilder();
            var account = builder.User
                .SetUsername("bobSmith")
                .CreateUserAccount();

            var createdUsername = account.RetrieveUsername();
            Assert.AreEqual("bobSmith", createdUsername);
        }

        [Test]
        public void RetrieveUserName_CustomClaimOverload()
        {
            var builder = new TestServerBuilder();
            var account = builder.User
                .SetUsername("bobSmith")
                .AddUserClaim("fakeClaim2", "janeDoe")
                .CreateUserAccount();

            var createdUsername = account.RetrieveUsername("fakeClaim2");
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