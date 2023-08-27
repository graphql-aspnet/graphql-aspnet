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
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Middleware.SchemaItemSecurity.Components;
    using GraphQL.AspNet.Security;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using NSubstitute;
    using NSubstitute.ExceptionExtensions;
    using NUnit.Framework;

    [TestFixture]
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public class SchemaItemAuthenticationMiddlewareTests
    {
        private class TestHandler : IAuthenticationHandler
        {
            public Task<AuthenticateResult> AuthenticateAsync() => Task.FromResult(null as AuthenticateResult);

            public Task ChallengeAsync(AuthenticationProperties properties) => Task.CompletedTask;

            public Task ForbidAsync(AuthenticationProperties properties) => Task.CompletedTask;

            public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context) => Task.CompletedTask;
        }

        private const string DEFAULT_SCHEME = "Default";
        private IAuthenticationSchemeProvider _provider;
        private IUserSecurityContext _userSecurityContext;
        private Dictionary<string, ClaimsPrincipal> _usersByScheme;

        public SchemaItemAuthenticationMiddlewareTests()
        {
            var defaultScheme = new AuthenticationScheme(
                DEFAULT_SCHEME,
                null,
                typeof(TestHandler));

            _provider = Substitute.For<IAuthenticationSchemeProvider>();
            _provider.GetDefaultAuthenticateSchemeAsync()
                .Returns(defaultScheme);

            _provider.GetAllSchemesAsync()
                .Returns(new[] { defaultScheme });

            _userSecurityContext = Substitute.For<IUserSecurityContext>();
            _userSecurityContext.DefaultUser.Returns(x => null);

            _usersByScheme = new Dictionary<string, ClaimsPrincipal>();
            _usersByScheme.Add(DEFAULT_SCHEME, new ClaimsPrincipal());
        }

        public Task EmptyNextDelegate(SchemaItemSecurityChallengeContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        private async Task<SchemaItemSecurityChallengeContext> ExecuteTest(SchemaItemSecurityRequirements secRequirements)
        {
            var defaultSet = false;
            foreach (var kvp in _usersByScheme)
            {
                var authResult = Substitute.For<IAuthenticationResult>();
                authResult.Suceeded.Returns(kvp.Value != null);
                authResult.User.Returns(kvp.Value);
                authResult.AuthenticationScheme.Returns(kvp.Key);
                _userSecurityContext?.AuthenticateAsync(kvp.Key, Arg.Any<CancellationToken>())
                    .Returns(authResult);

                if (kvp.Key == DEFAULT_SCHEME)
                {
                    _userSecurityContext?.AuthenticateAsync(Arg.Any<CancellationToken>())
                        .Returns(authResult);
                    defaultSet = true;
                }
            }

            if (!defaultSet)
            {
                _userSecurityContext?.AuthenticateAsync(Arg.Any<CancellationToken>())
                    .ThrowsAsync(new InvalidOperationException("No Default Scheme Defined"));
            }

            var builder = new TestServerBuilder();
            var server = builder.Build();

            var contextBuilder = server.CreateQueryContextBuilder();
            contextBuilder.AddUserSecurityContext(_userSecurityContext);
            var queryContext = contextBuilder.Build();

            var field = Substitute.For<IGraphField>();
            var fieldSecurityRequest = Substitute.For<ISchemaItemSecurityRequest>();
            fieldSecurityRequest.SecureSchemaItem.Returns(field);

            var fieldSecurityContext = new SchemaItemSecurityChallengeContext(queryContext, fieldSecurityRequest);
            fieldSecurityContext.SecurityRequirements = secRequirements;

            var middleware = new SchemaItemAuthenticationMiddleware(_provider);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Unauthenticated, result.Result.Status);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Unauthenticated, result.Result.Status);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Unauthenticated, result.Result.Status);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Failed, result.Result.Status);
        }

        [Test]
        public async Task NoSecurityRequirements_Fails()
        {
            var result = await this.ExecuteTest(null);

            Assert.IsNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Failed, result.Result.Status);
        }

        [Test]
        public async Task SecurityRequirementsAreAutoDeny_Unauthenticated()
        {
            var result = await this.ExecuteTest(SchemaItemSecurityRequirements.AutoDeny);

            Assert.IsNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Unauthenticated, result.Result.Status);
        }
    }
}