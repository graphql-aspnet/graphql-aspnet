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
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Web.Security;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class HttpUserSecurityContextTests
    {
        [Test]
        public void DefaultUser_ReturnsHttpContextUser()
        {
            var user = new ClaimsPrincipal();
            var httpContext = new DefaultHttpContext();
            httpContext.User = user;

            var userContext = new HttpUserSecurityContext(httpContext);
            var result = userContext.DefaultUser;

            Assert.AreEqual(result, user);
        }

        [Test]
        public async Task AuthenticateScheme_EncapsulatesAnAuthTicketForSameScheme()
        {
            var schemeToTest = "MyTestScheme";

            var user = new ClaimsPrincipal();

            var authService = Substitute.For<IAuthenticationService>();
            authService.AuthenticateAsync(Arg.Any<HttpContext>(), Arg.Any<string>())
                .Returns(AuthenticateResult.Success(new AuthenticationTicket(user, schemeToTest)));

            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(Arg.Any<Type>())
                .Returns(authService);

            var httpContext = new DefaultHttpContext();
            httpContext.User = user;
            httpContext.RequestServices = serviceProvider;

            using var userContext = new HttpUserSecurityContext(httpContext);
            var result = await userContext.AuthenticateAsync(schemeToTest);

            Assert.IsNotNull(result);
            Assert.AreEqual(schemeToTest, result.AuthenticationScheme);
            Assert.AreEqual(user, result.User);
            Assert.IsTrue(result.Suceeded);
        }

        [Test]
        public async Task AuthenticateScheme_UsesNullScheme()
        {
            var user = new ClaimsPrincipal();

            var authService = Substitute.For<IAuthenticationService>();
            authService.AuthenticateAsync(Arg.Any<HttpContext>(), null)
                .Returns(AuthenticateResult.Success(new AuthenticationTicket(user, null)));

            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(Arg.Any<Type>())
                .Returns(authService);

            var httpContext = new DefaultHttpContext();
            httpContext.User = user;
            httpContext.RequestServices = serviceProvider;

            using var userContext = new HttpUserSecurityContext(httpContext);
            var result = await userContext.AuthenticateAsync();

            Assert.IsNotNull(result);
            Assert.IsNull(result.AuthenticationScheme);
            Assert.AreEqual(user, result.User);
            Assert.IsTrue(result.Suceeded);
        }

        [Test]
        public async Task RepeatedAuthCalls_ResultsAreCached()
        {
            var user = new ClaimsPrincipal();

            var authService = Substitute.For<IAuthenticationService>();
            authService.AuthenticateAsync(Arg.Any<HttpContext>(), null)
                .Returns(AuthenticateResult.Success(new AuthenticationTicket(user, null)));

            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(Arg.Any<Type>())
                .Returns(authService);

            var httpContext = new DefaultHttpContext();
            httpContext.User = user;
            httpContext.RequestServices = serviceProvider;

            using var userContext = new HttpUserSecurityContext(httpContext);
            var result = await userContext.AuthenticateAsync();
            var result1 = await userContext.AuthenticateAsync();

            Assert.IsNotNull(result);
            Assert.IsNull(result.AuthenticationScheme);
            Assert.AreEqual(user, result.User);
            Assert.IsTrue(result.Suceeded);
            Assert.AreEqual(result, result1);

            await authService.Received(1).AuthenticateAsync(Arg.Any<HttpContext>(), null);
        }
    }
}