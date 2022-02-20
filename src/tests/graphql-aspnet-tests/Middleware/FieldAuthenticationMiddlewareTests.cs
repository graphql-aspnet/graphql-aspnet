// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.FieldSecurity.Components;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Security;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public class FieldAuthenticationMiddlewareTests
    {
        private class TestHandler : IAuthenticationHandler
        {
            public Task<AuthenticateResult> AuthenticateAsync() => Task.FromResult(null as AuthenticateResult);

            public Task ChallengeAsync(AuthenticationProperties properties) => Task.CompletedTask;

            public Task ForbidAsync(AuthenticationProperties properties) => Task.CompletedTask;

            public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context) => Task.CompletedTask;
        }

        private const string DEFAULT_SCHEME = "Default";
        private Mock<IAuthenticationSchemeProvider> _provider;
        private Mock<IUserSecurityContext> _userSecurityContext;
        private Dictionary<string, ClaimsPrincipal> _usersByScheme;

        public FieldAuthenticationMiddlewareTests()
        {
            var defaultScheme = new AuthenticationScheme(
                DEFAULT_SCHEME,
                null,
                typeof(TestHandler));

            _provider = new Mock<IAuthenticationSchemeProvider>();
            _provider.Setup(x => x.GetDefaultAuthenticateSchemeAsync())
                .ReturnsAsync(defaultScheme);

            _provider.Setup(x => x.GetAllSchemesAsync())
                .ReturnsAsync(new[] { defaultScheme });

            _userSecurityContext = new Mock<IUserSecurityContext>();

            _usersByScheme = new Dictionary<string, ClaimsPrincipal>();
            _usersByScheme.Add(DEFAULT_SCHEME, new ClaimsPrincipal());
        }

        public Task EmptyNextDelegate(GraphFieldSecurityContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        private async Task<GraphFieldSecurityContext> ExecuteTest(FieldSecurityRequirements secRequirements)
        {
            var defaultSet = false;
            foreach (var kvp in _usersByScheme)
            {
                var authResult = new Mock<IAuthenticationResult>();
                authResult.Setup(x => x.Suceeded).Returns(kvp.Value != null);
                authResult.Setup(x => x.User).Returns(kvp.Value);
                authResult.Setup(x => x.AuthenticationScheme).Returns(kvp.Key);
                _userSecurityContext?.Setup(x => x.Authenticate(kvp.Key, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(authResult.Object);

                if (kvp.Key == DEFAULT_SCHEME)
                {
                    _userSecurityContext?.Setup(x => x.Authenticate(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(authResult.Object);
                    defaultSet = true;
                }
            }

            if (!defaultSet)
            {
                _userSecurityContext?.Setup(x => x.Authenticate(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new InvalidOperationException("No Default Scheme Defined"));
            }

            var builder = new TestServerBuilder();
            var server = builder.Build();

            var contextBuilder = server.CreateQueryContextBuilder();
            contextBuilder.AddUserSecurityContext(_userSecurityContext?.Object);
            var queryContext = contextBuilder.Build();

            var field = new Mock<IGraphField>();
            var fieldSecurityRequest = new Mock<IGraphFieldSecurityRequest>();
            fieldSecurityRequest.Setup(x => x.Field)
                .Returns(field.Object);

            var fieldSecurityContext = new GraphFieldSecurityContext(queryContext, fieldSecurityRequest.Object);
            fieldSecurityContext.SecurityRequirements = secRequirements;

            var middleware = new FieldAuthenticationMiddleware(_provider?.Object);
            await middleware.InvokeAsync(fieldSecurityContext, this.EmptyNextDelegate);
            middleware.Dispose();

            return fieldSecurityContext;
        }

        [Test]
        public async Task WhenNoSchemeIsSpecified_DefaultSchemeIsUsed()
        {
            var requirements = new FieldSecurityRequirementsBuilder()
                .Build();

            var result = await this.ExecuteTest(requirements);

            Assert.IsNotNull(result.AuthenticatedUser);
            Assert.AreEqual(_usersByScheme[DEFAULT_SCHEME], result.AuthenticatedUser);
            Assert.IsNull(result.Result);
        }

        [Test]
        public async Task WhenNoSchemeIsSpecified_AndDefaultSchemeFails_NotAuthenticated()
        {
            _usersByScheme[DEFAULT_SCHEME] = null; // no default user authenticated

            // just basic authentication using the default scheme
            var requirements = new FieldSecurityRequirementsBuilder()
                .Build();

            var result = await this.ExecuteTest(requirements);

            Assert.IsNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Unauthenticated, result.Result.Status);
        }

        [Test]
        public async Task WhenNoSchemeIsSPecified_AndNoDefaultSchemeIsSet_NotAuthenticated()
        {
            var requirements = new FieldSecurityRequirementsBuilder()
                .Build();

            // mimic "no default scheme set"
            _usersByScheme.Clear();

            var result = await this.ExecuteTest(requirements);

            Assert.IsNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Unauthenticated, result.Result.Status);
        }

        [Test]
        public async Task WhenAllowAnonymous_ButDefaultAuthenticatedUserFound_UserIsReturned()
        {
            // anonymous users are allowed and no specific authentication
            // scheme is indicated. Attempt to authenticate with the known default
            var requirements = new FieldSecurityRequirementsBuilder()
                .AllowAnonymousUsers()
                .Build();

            var result = await this.ExecuteTest(requirements);

            Assert.IsNotNull(result.AuthenticatedUser);
            Assert.AreEqual(_usersByScheme[DEFAULT_SCHEME], result.AuthenticatedUser);
            Assert.IsNull(result.Result);
        }

        [Test]
        public async Task WhenAllowsAnonymous_AndNoAuthenticatedUserFound_NoUserIsReturned_ProcessingNotBlocked()
        {
            var requirements = new FieldSecurityRequirementsBuilder()
                .AllowAnonymousUsers()
                .Build();

            _usersByScheme.Clear();

            var result = await this.ExecuteTest(requirements);

            Assert.IsNull(result.AuthenticatedUser); // no user
            Assert.IsNull(result.Result);  // no assigned result yet
        }

        [Test]
        public async Task WhenSpecificSchemeRequired_AndUserContextDoesNotMatch_IsNotAuthorized()
        {
            _usersByScheme.Add("testScheme", new ClaimsPrincipal());

            // does not match auth's scheme
            var requirements = new FieldSecurityRequirementsBuilder()
                .AddAllowedAuthenticationScheme("testScheme2")
                .Build();

            var result = await this.ExecuteTest(requirements);

            Assert.IsNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Unauthenticated, result.Result.Status);
        }

        [Test]
        public async Task WhenSpecificSchemeRequired_AndUserContextMatches_IsAuthorized()
        {
            var authedUser = new ClaimsPrincipal();
            _usersByScheme.Add("testScheme", authedUser);

            // does match auth's scheme
            var requirements = new FieldSecurityRequirementsBuilder()
                .AddAllowedAuthenticationScheme("testScheme")
                .Build();

            var result = await this.ExecuteTest(requirements);

            Assert.IsNotNull(result.AuthenticatedUser);
            Assert.AreEqual(authedUser, result.AuthenticatedUser);
            Assert.IsNull(result.Result);
        }

        [Test]
        public async Task NoSecurityContext_Fails()
        {
            _userSecurityContext = null;

            var requirements = new FieldSecurityRequirementsBuilder()
                .Build();

            var result = await this.ExecuteTest(requirements);

            Assert.IsNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Failed, result.Result.Status);
        }

        [Test]
        public async Task NoSecurityRequirements_Fails()
        {
            var result = await this.ExecuteTest(null);

            Assert.IsNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Failed, result.Result.Status);
        }

        [Test]
        public async Task SecurityRequirementsAreAutoDeny_Unauthenticated()
        {
            var result = await this.ExecuteTest(FieldSecurityRequirements.AutoDeny);

            Assert.IsNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Unauthenticated, result.Result.Status);
        }
    }
}