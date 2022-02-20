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
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.FieldSecurity.Components;
    using GraphQL.AspNet.Security;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.AspNetCore.Authorization;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public class FieldAuthorizationMiddlewareTests
    {
        private Mock<IAuthorizationService> _authService;
        private string _userName;
        private ClaimsPrincipal _user = null;

        public FieldAuthorizationMiddlewareTests()
        {
            _authService = new Mock<IAuthorizationService>();
            _userName = "john-doe";

            // auto fail any auth checks
            _authService.Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed());
            _authService.Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Failed());
        }

        public Task EmptyNextDelegate(GraphFieldSecurityContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        private async Task<GraphFieldSecurityContext> ExecuteTest(
            FieldSecurityRequirements secRequirements,
            string userRoles = null)
        {
            var rolesToInclude = new List<string>();
            if (userRoles != null)
            {
                rolesToInclude = userRoles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToList();
            }

            var builder = new TestServerBuilder();
            var buildUser = !string.IsNullOrWhiteSpace(_userName);

            // when null, do not authenticate the user
            if (buildUser)
            {
                builder.UserContext.Authenticate(_userName);

                foreach (var role in rolesToInclude)
                    builder.UserContext.AddUserRole(role);
            }

            var server = builder.Build();
            if (buildUser)
                _user = (await server.SecurityContext.Authenticate())?.User;

            var contextBuilder = server.CreateQueryContextBuilder();
            var queryContext = contextBuilder.Build();

            var field = new Mock<IGraphField>();
            var fieldSecurityRequest = new Mock<IGraphFieldSecurityRequest>();
            fieldSecurityRequest.Setup(x => x.Field)
                .Returns(field.Object);

            var fieldSecurityContext = new GraphFieldSecurityContext(queryContext, fieldSecurityRequest.Object);
            fieldSecurityContext.AuthenticatedUser = _user;
            fieldSecurityContext.SecurityRequirements = secRequirements;

            var middleware = new FieldAuthorizationMiddleware(_authService?.Object);
            await middleware.InvokeAsync(fieldSecurityContext, this.EmptyNextDelegate);

            return fieldSecurityContext;
        }

        private void SetupPolicyCheckResult(AuthorizationPolicy policy, bool successful)
        {
            AuthorizationResult result;
            if (successful)
                result = AuthorizationResult.Success();
            else
                result = AuthorizationResult.Failed();

            _authService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                policy.Requirements))
                .ReturnsAsync(result);
        }

        [Test]
        public void NoSecurityRequirements_ThrowsException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await this.ExecuteTest(null);
            });
        }

        [Test]
        public async Task NoPoliciesOrRolesToCheck_WithNoUser_Skipped()
        {
            var requirements = new FieldSecurityRequirementsBuilder()
                .Build();

            _userName = null;
            var result = await this.ExecuteTest(requirements);

            Assert.IsNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Skipped, result.Result.Status);
        }

        [Test]
        public async Task NoPoliciesOrRolesToCheck_AllowAnonymous_WithAuthenticatedUser_Skipped()
        {
            var requirements = new FieldSecurityRequirementsBuilder()
                .AllowAnonymousUsers()
                .Build();

            var result = await this.ExecuteTest(requirements);

            Assert.IsNotNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Skipped, result.Result.Status);
        }

        [Test]
        public async Task PolcicesToCheck_AllowAnonymous_WithAuthenticatedUser_ThatDoesNotMeetPolicy_Authorized()
        {
            var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .RequireClaim("claim1", "value1")
                        .Build();

            var requirements = new FieldSecurityRequirementsBuilder()
                .AllowAnonymousUsers()
                .AddEnforcePolicy("Policy1", policy)
                .Build();

            this.SetupPolicyCheckResult(policy, false);
            var result = await this.ExecuteTest(requirements);

            Assert.IsNotNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Authorized, result.Result.Status);
        }

        [Test]
        public async Task PolcicesToCheck_AllowAnonymous_WithNoUser_Authorized()
        {
            var requirements = new FieldSecurityRequirementsBuilder()
                .AllowAnonymousUsers()
                .AddEnforcePolicy(
                    "Policy1",
                    new AuthorizationPolicyBuilder()
                        .RequireClaim("claim1", "value1")
                        .Build())
                .Build();

            _userName = null;

            var result = await this.ExecuteTest(requirements);

            Assert.IsNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Authorized, result.Result.Status);
        }

        [Test]
        public async Task PolcicesToCheck_AllowAnonymous_WithAuthenticatedUser_ThatDoesMeetPolicy_Authorized()
        {
            var policy = new AuthorizationPolicyBuilder()
                        .RequireClaim("claim1", "value1")
                        .Build();

            var requirements = new FieldSecurityRequirementsBuilder()
                .AllowAnonymousUsers()
                .AddEnforcePolicy("Policy1", policy)
                .Build();

            this.SetupPolicyCheckResult(policy, true);
            var result = await this.ExecuteTest(requirements);

            Assert.IsNotNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Authorized, result.Result.Status);
        }

        [Test]
        public async Task PolcicesToCheck_WithAuthenticatedUser_ThatDoesMeetPolicy_Authorized()
        {
            var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();

            var requirements = new FieldSecurityRequirementsBuilder()
                .AddEnforcePolicy("Policy1", policy)
                .Build();

            // set claim for policy on user
            this.SetupPolicyCheckResult(policy, true);
            var result = await this.ExecuteTest(requirements);

            Assert.IsNotNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Authorized, result.Result.Status);
        }

        [Test]
        public async Task PolcicesToCheck_WithAuthenticatedUser_ThatDoesNOTMeetPolicy_UnAuthorized()
        {
            var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .RequireClaim("claim1", "value1")
                        .Build();

            var requirements = new FieldSecurityRequirementsBuilder()
                .AddEnforcePolicy("Policy1", policy)
                .Build();

            // set claim for policy on user
            this.SetupPolicyCheckResult(policy, false);
            var result = await this.ExecuteTest(requirements);

            Assert.IsNotNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Unauthorized, result.Result.Status);
        }

        [Test]
        public async Task PolcicesToCheck_WithNoUser_UnAuthorized()
        {
            var requirements = new FieldSecurityRequirementsBuilder()
                .AddEnforcePolicy(
                    "Policy1",
                    new AuthorizationPolicyBuilder()
                        .RequireClaim("claim1", "value1")
                        .Build())
                .Build();

            _userName = null;
            var result = await this.ExecuteTest(requirements);

            Assert.IsNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Unauthorized, result.Result.Status);
        }

        [Test]
        public async Task RolesToCheck_WithAuthenticatedUser_ThatDoesMeetRoles_Authorized()
        {
            var requirements = new FieldSecurityRequirementsBuilder()
                .AddRequiredRoleGroup("role1", "role2")
                .Build();

            var result = await this.ExecuteTest(requirements, "role1");

            Assert.IsNotNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Authorized, result.Result.Status);
        }

        [Test]
        public async Task RolesToCheck_WithAuthenticatedUser_ThatDoesNOTMeetRoles_UnAuthorized()
        {
            var requirements = new FieldSecurityRequirementsBuilder()
                .AddRequiredRoleGroup("role1", "role2")
                .Build();

            var result = await this.ExecuteTest(requirements, "role3");

            Assert.IsNotNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Unauthorized, result.Result.Status);
        }

        [Test]
        public async Task RolesToCheck_WithNoUser_UnAuthorized()
        {
            var requirements = new FieldSecurityRequirementsBuilder()
                .AddRequiredRoleGroup("role1", "role2")
                .Build();

            // no user
            _userName = null;
            var result = await this.ExecuteTest(requirements);

            Assert.IsNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Unauthorized, result.Result.Status);
        }

        [Test]
        public async Task RolesToCheck_AllowAnonymous_WithNoUser_Authorized()
        {
            var requirements = new FieldSecurityRequirementsBuilder()
                .AllowAnonymousUsers()
                .AddRequiredRoleGroup("role1", "role2")
                .Build();

            _userName = null;

            // set no claims on user
            var result = await this.ExecuteTest(requirements);

            Assert.IsNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Authorized, result.Result.Status);
        }

        [Test]
        public async Task RolesToCheck_AllowAnonymous_WithAuthenticatedUser_ThatDoesMeetRole_Authorized()
        {
            var requirements = new FieldSecurityRequirementsBuilder()
                .AllowAnonymousUsers()
                .AddRequiredRoleGroup("role1", "role2")
                .Build();

            var result = await this.ExecuteTest(requirements, "role1");

            Assert.IsNotNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Authorized, result.Result.Status);
        }

        [Test]
        public async Task RolesToCheck_AllowAnonymous_WithAuthenticatedUser_ThatDoesNOTMeetRole_Authorized()
        {
            var requirements = new FieldSecurityRequirementsBuilder()
                .AllowAnonymousUsers()
                .AddRequiredRoleGroup("role1", "role2")
                .Build();

            // set no claims on user
            var result = await this.ExecuteTest(requirements, null);

            Assert.IsNotNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Authorized, result.Result.Status);
        }

        [Test]
        public async Task NoAuthService_RolesToCheck_WithAuthenticatedUser_ThatDoesMeetRoles_Authorized()
        {
            var requirements = new FieldSecurityRequirementsBuilder()
                .AddRequiredRoleGroup("role1", "role2")
                .Build();

            _authService = null;
            var result = await this.ExecuteTest(requirements, "role1");

            Assert.IsNotNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Authorized, result.Result.Status);
        }

        [Test]
        public async Task NoAuthService_PolicesToCheck_Fails()
        {
            var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

            var requirements = new FieldSecurityRequirementsBuilder()
                .AddEnforcePolicy("Policy1", policy)
                .Build();

            _authService = null;
            var result = await this.ExecuteTest(requirements);

            Assert.IsNotNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Failed, result.Result.Status);
        }

        [Test]
        public async Task KitchenSink_ThatShouldSucceed_Authorized()
        {
            var policy1 = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();

            var policy2 = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();

            var requirements = new FieldSecurityRequirementsBuilder()
                .AddEnforcePolicy("Policy1", policy1)
                .AddEnforcePolicy("Policy2", policy2)
                .AddRequiredRoleGroup("role1", "role2")
                .AddRequiredRoleGroup("role3", "role4")
                .AddRequiredRoleGroup("role5", "role6", "role7")
                .Build();

            this.SetupPolicyCheckResult(policy1, true);
            this.SetupPolicyCheckResult(policy2, true);
            var result = await this.ExecuteTest(requirements, "role1, role4, role6");

            Assert.IsNotNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Authorized, result.Result.Status);
        }

        [Test]
        public async Task KitchenSink_WithOneFailedPolicy_Unauthorized()
        {
            var policy1 = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();

            var policy2 = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();

            var requirements = new FieldSecurityRequirementsBuilder()
                .AddEnforcePolicy("Policy1", policy1)
                .AddEnforcePolicy("Policy2", policy2)
                .AddRequiredRoleGroup("role1", "role2")
                .AddRequiredRoleGroup("role3", "role4")
                .AddRequiredRoleGroup("role5", "role6", "role7")
                .Build();

            this.SetupPolicyCheckResult(policy1, true);
            this.SetupPolicyCheckResult(policy2, false);
            var result = await this.ExecuteTest(requirements, "role1, role4, role6");

            Assert.IsNotNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Unauthorized, result.Result.Status);
        }

        [Test]
        public async Task KitchenSink_WithOneFailedRole_Unauthorized()
        {
            var policy1 = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();

            var policy2 = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();

            var requirements = new FieldSecurityRequirementsBuilder()
                .AddEnforcePolicy("Policy1", policy1)
                .AddEnforcePolicy("Policy2", policy2)
                .AddRequiredRoleGroup("role1", "role2")
                .AddRequiredRoleGroup("role3", "role4")
                .AddRequiredRoleGroup("role5", "role6", "role7")
                .Build();

            this.SetupPolicyCheckResult(policy1, true);
            this.SetupPolicyCheckResult(policy2, true);
            var result = await this.ExecuteTest(requirements, "role1, role4");

            Assert.IsNotNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Unauthorized, result.Result.Status);
        }

        [Test]
        public async Task KitchenSink_NoAuthService_Failed()
        {
            var policy1 = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();

            var policy2 = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();

            var requirements = new FieldSecurityRequirementsBuilder()
                .AddEnforcePolicy("Policy1", policy1)
                .AddEnforcePolicy("Policy2", policy2)
                .AddRequiredRoleGroup("role1", "role2")
                .AddRequiredRoleGroup("role3", "role4")
                .AddRequiredRoleGroup("role5", "role6", "role7")
                .Build();

            _authService = null;
            var result = await this.ExecuteTest(requirements, "role1, role4");

            Assert.IsNotNull(result.AuthenticatedUser);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Failed, result.Result.Status);
        }
    }
}