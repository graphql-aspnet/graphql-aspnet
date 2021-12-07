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
    using GraphQL.AspNet.Security;
    using GraphQL.AspNet.Security.Web;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Moq;
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

            var authService = new Mock<IAuthenticationService>();
            authService.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
                .ReturnsAsync(AuthenticateResult.Success(new AuthenticationTicket(user, schemeToTest)));

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(It.IsAny<Type>()))
                .Returns(authService.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.User = user;
            httpContext.RequestServices = serviceProvider.Object;

            using var userContext = new HttpUserSecurityContext(httpContext);
            var result = await userContext.Authenticate(schemeToTest);

            Assert.IsNotNull(result);
            Assert.AreEqual(schemeToTest, result.AuthenticationScheme);
            Assert.AreEqual(user, result.User);
            Assert.IsTrue(result.Suceeded);
        }

        [Test]
        public async Task AuthenticateScheme_UsesNullScheme()
        {
            var user = new ClaimsPrincipal();

            var authService = new Mock<IAuthenticationService>();
            authService.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>(), null))
                .ReturnsAsync(AuthenticateResult.Success(new AuthenticationTicket(user, null)));

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(It.IsAny<Type>()))
                .Returns(authService.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.User = user;
            httpContext.RequestServices = serviceProvider.Object;

            using var userContext = new HttpUserSecurityContext(httpContext);
            var result = await userContext.Authenticate();

            Assert.IsNotNull(result);
            Assert.IsNull(result.AuthenticationScheme);
            Assert.AreEqual(user, result.User);
            Assert.IsTrue(result.Suceeded);
        }

        [Test]
        public async Task RepeatedAuthCalls_ResultsAreCached()
        {
            var user = new ClaimsPrincipal();

            var authService = new Mock<IAuthenticationService>();
            authService.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>(), null))
                .ReturnsAsync(AuthenticateResult.Success(new AuthenticationTicket(user, null)));

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(It.IsAny<Type>()))
                .Returns(authService.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.User = user;
            httpContext.RequestServices = serviceProvider.Object;

            using var userContext = new HttpUserSecurityContext(httpContext);
            var result = await userContext.Authenticate();
            var result1 = await userContext.Authenticate();

            Assert.IsNotNull(result);
            Assert.IsNull(result.AuthenticationScheme);
            Assert.AreEqual(user, result.User);
            Assert.IsTrue(result.Suceeded);
            Assert.AreEqual(result, result1);

            authService.Verify(x => x.AuthenticateAsync(It.IsAny<HttpContext>(), null), Times.Once());
        }
    }
}