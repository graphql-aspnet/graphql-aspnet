﻿// *************************************************************
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
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Middleware.SchemaItemSecurity.Components;
    using GraphQL.AspNet.Security;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public class SchemaItemAuthorizationMiddlewareTests
    {
        private IAuthorizationService _authService;
        private string _userName;
        private ClaimsPrincipal _user = null;

        public SchemaItemAuthorizationMiddlewareTests()
        {
            _authService = Substitute.For<IAuthorizationService>();
            _userName = "john-doe";

            // auto fail any auth checks
            _authService.AuthorizeAsync(
                    Arg.Any<ClaimsPrincipal>(),
                    Arg.Any<object>(),
                    Arg.Any<string>())
                .Returns(AuthorizationResult.Failed());
            _authService.AuthorizeAsync(
                    Arg.Any<ClaimsPrincipal>(),
                    Arg.Any<object>(),
                    Arg.Any<IEnumerable<IAuthorizationRequirement>>())
                .Returns(AuthorizationResult.Failed());
        }

        public Task EmptyNextDelegate(SchemaItemSecurityChallengeContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        private async Task<SchemaItemSecurityChallengeContext> ExecuteTest(
            SchemaItemSecurityRequirements secRequirements,
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

            // remove all auth service instances to limit to just
            // those cases being tested
            if (_authService == null)
                builder.Authorization.DisableAuthorization();
            else
                builder.Authorization.ReplaceAuthorizationService(_authService);

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
                _user = (await server.SecurityContext.AuthenticateAsync())?.User;

            var contextBuilder = server.CreateQueryContextBuilder();
            var queryContext = contextBuilder.Build();

            var field = Substitute.For<IGraphField>();
            var securityRequest = Substitute.For<ISchemaItemSecurityRequest>();
            securityRequest.SecureSchemaItem
                .Returns(field);

            var fieldSecurityContext = new SchemaItemSecurityChallengeContext(queryContext, securityRequest);
            fieldSecurityContext.AuthenticatedUser = _user;
            fieldSecurityContext.SecurityRequirements = secRequirements;

            var middleware = new SchemaItemAuthorizationMiddleware();
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

            _authService.AuthorizeAsync(
                Arg.Any<ClaimsPrincipal>(),
                Arg.Any<object>(),
                policy.Requirements)
                .Returns(result);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Skipped, result.Result.Status);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Skipped, result.Result.Status);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Authorized, result.Result.Status);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Authorized, result.Result.Status);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Authorized, result.Result.Status);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Authorized, result.Result.Status);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Unauthorized, result.Result.Status);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Unauthorized, result.Result.Status);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Authorized, result.Result.Status);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Unauthorized, result.Result.Status);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Unauthorized, result.Result.Status);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Authorized, result.Result.Status);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Authorized, result.Result.Status);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Authorized, result.Result.Status);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Authorized, result.Result.Status);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Failed, result.Result.Status);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Authorized, result.Result.Status);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Unauthorized, result.Result.Status);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Unauthorized, result.Result.Status);
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
            Assert.AreEqual(SchemaItemSecurityChallengeStatus.Failed, result.Result.Status);
        }
    }
}