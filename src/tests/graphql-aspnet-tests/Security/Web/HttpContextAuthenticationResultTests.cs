// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Security.Web
{
    using System.Security.Claims;
    using GraphQL.AspNet.Security;
    using GraphQL.AspNet.Security.Web;
    using Microsoft.AspNetCore.Authentication;
    using NUnit.Framework;

    [TestFixture]
    public class HttpContextAuthenticationResultTests
    {
        [Test]
        public void ValidAuthTicket_ResultsInSuccess()
        {
            var user = new ClaimsPrincipal();
            var ticket = new AuthenticationTicket(user, "testScheme");
            var authResult = AuthenticateResult.Success(ticket);

            var result = new HttpContextAuthenticationResult("testScheme", authResult);

            Assert.IsTrue(result.Suceeded);
            Assert.AreEqual("testScheme", result.AuthenticationScheme);
            Assert.AreEqual(user, result.User);
        }

        [Test]
        public void FailedTicket_ResultsInFailure()
        {
            var authResult = AuthenticateResult.Fail("failed");
            var result = new HttpContextAuthenticationResult("testScheme", authResult);

            Assert.IsFalse(result.Suceeded);
            Assert.AreEqual("testScheme", result.AuthenticationScheme);
            Assert.IsNull(result.User);
        }

        [Test]
        public void NoAuthTicket_ResultsInFailure()
        {
            var result = new HttpContextAuthenticationResult("testScheme", null);

            Assert.IsFalse(result.Suceeded);
            Assert.AreEqual("testScheme", result.AuthenticationScheme);
            Assert.IsNull(result.User);
        }
    }
}