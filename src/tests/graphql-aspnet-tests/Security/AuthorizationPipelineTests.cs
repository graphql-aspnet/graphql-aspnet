// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Security
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Security;
    using GraphQL.AspNet.Tests.Controllers.FieldAuthorizerTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class AuthorizationPipelineTests
    {
        private void AssertAuthorizationFails(FieldAuthorizationResult result)
        {
            Assert.IsNotNull(result);
            Assert.AreEqual(FieldAuthorizationStatus.Unauthorized, result.Status);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.LogMessage));
        }

        private void AssertAuthorizationSuccess(FieldAuthorizationResult result)
        {
            Assert.IsNotNull(result);
            Assert.AreEqual(FieldAuthorizationStatus.Authorized, result.Status);
            Assert.IsTrue(string.IsNullOrWhiteSpace(result.LogMessage));
        }

        private void AssertAuthorizationSkipped(FieldAuthorizationResult result)
        {
            Assert.IsNotNull(result);
            Assert.AreEqual(FieldAuthorizationStatus.Skipped, result.Status);
            Assert.IsTrue(string.IsNullOrWhiteSpace(result.LogMessage));
        }

        [Test]
        public async Task RolePolicy_UserNotInRole_Fails()
        {
            var builder = new TestServerBuilder()
                .AddGraphType<Controller_NoPolicies>();

            builder.Authorization.AddRolePolicy("RequiresRole1", "role1");
            builder.User.AddUserRole("role4");
            var server = builder.Build();

            var fieldBuilder = server.CreateFieldContextBuilder<Controller_NoPolicies>(
                nameof(Controller_NoPolicies.RequireRolePolicy_RequiresRole1));

            var authContext = fieldBuilder.CreateAuthorizationContext();

            await server.ExecuteFieldAuthorization(authContext);
            AssertAuthorizationFails(authContext.Result);
        }

        [Test]
        public async Task RolePolicy_UserInRole_Success()
        {
            var builder = new TestServerBuilder()
                .AddGraphType<Controller_NoPolicies>();

            builder.Authorization.AddRolePolicy("RequiresRole1", "role1");
            builder.User.AddUserRole("role1");
            var server = builder.Build();

            var fieldBuilder = server.CreateFieldContextBuilder<Controller_NoPolicies>(
                nameof(Controller_NoPolicies.RequireRolePolicy_RequiresRole1));

            var authContext = fieldBuilder.CreateAuthorizationContext();

            await server.ExecuteFieldAuthorization(authContext);
            AssertAuthorizationSuccess(authContext.Result);
        }

        [Test]
        public async Task MultiPolicyCheck_UserPassesAll_Success()
        {
            var builder = new TestServerBuilder()
                .AddGraphType<Controller_NoPolicies>();

            builder.Authorization.AddRolePolicy("RequireRole6", "role6");
            builder.Authorization.AddClaimPolicy("RequireClaim7", "testClaim7", "testClaim7Value");
            builder.User.AddUserRole("role6");
            builder.User.AddUserClaim("testClaim7", "testClaim7Value");

            var server = builder.Build();

            var fieldBuilder = server.CreateFieldContextBuilder<Controller_NoPolicies>(
                nameof(Controller_NoPolicies.MultiPolicyMethod_RequireRole6_RequireClaim7));

            var authContext = fieldBuilder.CreateAuthorizationContext();

            await server.ExecuteFieldAuthorization(authContext);
            AssertAuthorizationSuccess(authContext.Result);
        }

        [Test]
        public async Task MultiPolicyCheck_UserPassesOnly1_Fail()
        {
            var builder = new TestServerBuilder()
                .AddGraphType<Controller_NoPolicies>();

            builder.Authorization.AddRolePolicy("RequireRole6", "role6");
            builder.Authorization.AddClaimPolicy("RequireClaim7", "testClaim7", "testClaim7Value");

            // user has role requirements
            builder.User.AddUserRole("role6");

            // user does not have claims requirements
            builder.User.AddUserClaim("testClaim8", "testClaim8Value");

            var server = builder.Build();

            var fieldBuilder = server.CreateFieldContextBuilder<Controller_NoPolicies>(
                nameof(Controller_NoPolicies.MultiPolicyMethod_RequireRole6_RequireClaim7));

            var authContext = fieldBuilder.CreateAuthorizationContext();

            await server.ExecuteFieldAuthorization(authContext);
            AssertAuthorizationFails(authContext.Result);
        }

        [Test]
        public async Task DirectRoleCheck_UserDoesNotHaveRole_Fails()
        {
            var builder = new TestServerBuilder()
                .AddGraphType<Controller_NoPolicies>();
            builder.User.AddUserRole("role1");
            var server = builder.Build();

            // policy name isnt declared on the controller method
            var fieldBuilder = server.CreateFieldContextBuilder<Controller_NoPolicies>(
                nameof(Controller_NoPolicies.MethodHasRoles_Role5));

            var authContext = fieldBuilder.CreateAuthorizationContext();

            await server.ExecuteFieldAuthorization(authContext);
            AssertAuthorizationFails(authContext.Result);
        }

        [Test]
        public async Task DirectRoleCheck_UserHasRole_Succeeds()
        {
            var builder = new TestServerBuilder()
                .AddGraphType<Controller_NoPolicies>();
            builder.User.AddUserRole("role5");
            var server = builder.Build();

            // policy name isnt declared on the controller method
            var fieldBuilder = server.CreateFieldContextBuilder<Controller_NoPolicies>(
                nameof(Controller_NoPolicies.MethodHasRoles_Role5));

            var authContext = fieldBuilder.CreateAuthorizationContext();

            await server.ExecuteFieldAuthorization(authContext);
            AssertAuthorizationSuccess(authContext.Result);
        }

        [Test]
        public async Task ClaimsPolicy_UserDoesntHaveClaim_Fails()
        {
            var builder = new TestServerBuilder()
                .AddGraphType<Controller_NoPolicies>();

            builder.Authorization.AddClaimPolicy("RequiresTestClaim6", "testClaim6", "testClaim6Value");
            builder.User.AddUserClaim("testClaim5", "testClaim5Value");
            var server = builder.Build();

            var fieldBuilder = server.CreateFieldContextBuilder<Controller_NoPolicies>(
                nameof(Controller_NoPolicies.RequireClaimPolicy_RequiresTestClaim6));

            var authContext = fieldBuilder.CreateAuthorizationContext();

            await server.ExecuteFieldAuthorization(authContext);
            AssertAuthorizationFails(authContext.Result);
        }

        [Test]
        public async Task ClaimsPolicy_UserDoesHaveClaim_Success()
        {
            var builder = new TestServerBuilder()
                .AddGraphType<Controller_NoPolicies>();

            builder.Authorization.AddClaimPolicy("RequiresTestClaim6", "testClaim6", "testClaim6Value");
            builder.User.AddUserClaim("testClaim6", "testClaim6Value");
            var server = builder.Build();

            var fieldBuilder = server.CreateFieldContextBuilder<Controller_NoPolicies>(
                nameof(Controller_NoPolicies.RequireClaimPolicy_RequiresTestClaim6));

            var authContext = fieldBuilder.CreateAuthorizationContext();

            await server.ExecuteFieldAuthorization(authContext);
            AssertAuthorizationSuccess(authContext.Result);
        }

        [Test]
        public async Task ClaimsPolicy_UserDoesHaveClaim_ButWrongValue_Fail()
        {
            var builder = new TestServerBuilder()
                .AddGraphType<Controller_NoPolicies>();

            builder.Authorization.AddClaimPolicy("RequiresTestClaim6", "testClaim6", "testClaim6Value");
            builder.User.AddUserClaim("testClaim6", "differentValueThanRequired");
            var server = builder.Build();

            var fieldBuilder = server.CreateFieldContextBuilder<Controller_NoPolicies>(
                nameof(Controller_NoPolicies.RequireClaimPolicy_RequiresTestClaim6));

            var authContext = fieldBuilder.CreateAuthorizationContext();

            await server.ExecuteFieldAuthorization(authContext);
            AssertAuthorizationFails(authContext.Result);
        }

        [Test]
        public async Task NoUserContext_Fails()
        {
            // do not register the user account on the test builder
            var builder = new TestServerBuilder()
                .AddGraphType<Controller_NoPolicies>();
            builder.Authorization.AddRolePolicy("TestPolicy", "role1");
            var server = builder.Build();

            // policy name isnt declared on the controller method
            var fieldBuilder = server.CreateFieldContextBuilder<Controller_NoPolicies>(
                nameof(Controller_NoPolicies.GeneralSecureMethod));

            fieldBuilder.AddUser(null);
            var authContext = fieldBuilder.CreateAuthorizationContext();

            await server.ExecuteFieldAuthorization(authContext);
            AssertAuthorizationFails(authContext.Result);
        }

        [Test]
        public async Task NoAuthService_Fails()
        {
            // do not register the auth service on the builder
            var builder = new TestServerBuilder()
                .AddGraphType<Controller_NoPolicies>();
            builder.Authorization.DisableAuthorization();
            builder.Authorization.AddRolePolicy("TestPolicy", "role1");
            builder.User.AddUserRole("role1");
            var server = builder.Build();

            // policy name isnt declared on the controller method
            var fieldBuilder = server.CreateFieldContextBuilder<Controller_NoPolicies>(
                nameof(Controller_NoPolicies.GeneralSecureMethod));

            var authContext = fieldBuilder.CreateAuthorizationContext();

            await server.ExecuteFieldAuthorization(authContext);
            AssertAuthorizationFails(authContext.Result);
        }

        [Test]
        public async Task NoAuthSerivce_ButNoDefinedRules_Skipped()
        {
            // do not register the auth service on the builder
            var builder = new TestServerBuilder()
                .AddGraphType<Controller_NoPolicies>();
            builder.Authorization.DisableAuthorization();

            // user not in any declared role
            builder.Authorization.AddRolePolicy("TestPolicy", "role1");
            builder.User.AddUserRole("role5");
            var server = builder.Build();

            // policy name isnt declared on the controller method
            var fieldBuilder = server.CreateFieldContextBuilder<Controller_NoPolicies>(
                nameof(Controller_NoPolicies.NoDefinedPolicies));

            var authContext = fieldBuilder.CreateAuthorizationContext();

            await server.ExecuteFieldAuthorization(authContext);
            AssertAuthorizationSkipped(authContext.Result);
        }

        [Test]
        public async Task NoUserContext_ButNoDefinedRules_Skipped()
        {
            // do not register the auth service on the builder
            var builder = new TestServerBuilder()
                .AddGraphType<Controller_NoPolicies>();

            var server = builder.Build();

            // policy name isnt declared on the controller method
            var fieldBuilder = server.CreateFieldContextBuilder<Controller_NoPolicies>(
                nameof(Controller_NoPolicies.NoDefinedPolicies));

            fieldBuilder.AddUser(null);

            var authContext = fieldBuilder.CreateAuthorizationContext();

            await server.ExecuteFieldAuthorization(authContext);
            AssertAuthorizationSkipped(authContext.Result);
        }

        [Test]
        public async Task AllowAnon_WhenUserDoesntPassChecks_Success()
        {
            var builder = new TestServerBuilder()
                .AddGraphType<Controller_NoPolicies>();

            builder.Authorization.AddClaimPolicy("RequiresTestClaim7", "testClaim7", "testClaim7Value");
            builder.User.AddUserClaim("testClaim6", "testClaim6Value");
            var server = builder.Build();

            var fieldBuilder = server.CreateFieldContextBuilder<Controller_NoPolicies>(
                nameof(Controller_NoPolicies.RequireClaimPolicy_RequiresTestClaim7_ButAlsoAllowAnon));

            var authContext = fieldBuilder.CreateAuthorizationContext();

            await server.ExecuteFieldAuthorization(authContext);
            AssertAuthorizationSuccess(authContext.Result);
        }

        [Test]
        public async Task MultiSecurityGroup_PassesOuter_FailsInner_Fails()
        {
            var builder = new TestServerBuilder()
                .AddGraphType<Controller_NoPolicies>();

            // controller policy
            builder.Authorization.AddClaimPolicy("RequiresPolicy5", "testClaim5", "testClaim5Value");

            // method policy
            builder.Authorization.AddRolePolicy("RequiresRole1", "role1");

            // user meets controller policy
            builder.User.AddUserClaim("testClaim5", "testClaim5Value");

            // user does not meet method policy
            builder.User.AddUserRole("role5");

            var server = builder.Build();

            var fieldBuilder = server.CreateFieldContextBuilder<Controller_Policy_RequiresPolicy5>(
                nameof(Controller_Policy_RequiresPolicy5.Policy_RequiresRole1));

            var authContext = fieldBuilder.CreateAuthorizationContext();

            await server.ExecuteFieldAuthorization(authContext);
            AssertAuthorizationFails(authContext.Result);
        }

        [Test]
        public async Task MultiSecurityGroup_PassesOuter_PassesInner_Success()
        {
            var builder = new TestServerBuilder()
                .AddGraphType<Controller_NoPolicies>();

            // controller policy
            builder.Authorization.AddClaimPolicy("RequiresPolicy5", "testClaim5", "testClaim5Value");

            // method policy
            builder.Authorization.AddRolePolicy("RequiresRole1", "role1");

            // user meets controller policy
            builder.User.AddUserClaim("testClaim5", "testClaim5Value");

            // user meet method policy
            builder.User.AddUserRole("role1");

            var server = builder.Build();

            var fieldBuilder = server.CreateFieldContextBuilder<Controller_Policy_RequiresPolicy5>(
                nameof(Controller_Policy_RequiresPolicy5.Policy_RequiresRole1));

            var authContext = fieldBuilder.CreateAuthorizationContext();

            await server.ExecuteFieldAuthorization(authContext);
            AssertAuthorizationSuccess(authContext.Result);
        }
    }
}