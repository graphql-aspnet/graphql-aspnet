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
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.FieldSecurity.Components;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Security;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Middleware.FildSecurityMiddlewareTestData;
    using Microsoft.AspNetCore.Authorization;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public class FieldSecurityRequirementsMiddlewareTests
    {
        private IGraphField _field;
        private Mock<IAuthorizationPolicyProvider> _policyProvider;
        private AuthorizationPolicy _defaultPolicy;

        public FieldSecurityRequirementsMiddlewareTests()
        {
            _policyProvider = new Mock<IAuthorizationPolicyProvider>();
            _policyProvider.Setup(x => x.GetDefaultPolicyAsync())
                .ReturnsAsync(() => _defaultPolicy);

            _defaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        }

        public Task EmptyNextDelegate(GraphFieldSecurityContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        private void SetupFieldForControllerMethod<TController>(string methodName)
        {
            this.SetupFieldForControllerMethod(typeof(TController), methodName);
        }

        private void SetupFieldMock(List<AppliedSecurityPolicyGroup> securityGroups)
        {
            var field = new Mock<IGraphField>();
            field.Setup(x => x.SecurityGroups).Returns(securityGroups);
            field.Setup(x => x.Route).Returns(new GraphFieldPath(AspNet.Execution.GraphCollection.Query, "some", "path"));
            _field = field.Object;
        }

        private void SetupFieldForControllerMethod(Type controllerType, string methodName)
        {
            var method = controllerType.GetMethod(methodName);
            if (method == null)
                Assert.Fail("Invalid Method, can't create test field");

            var controllerGroup = AppliedSecurityPolicyGroup.FromAttributeCollection(controllerType);
            var methodGroup = AppliedSecurityPolicyGroup.FromAttributeCollection(method);

            var list = new List<AppliedSecurityPolicyGroup>();
            list.Add(controllerGroup);
            list.Add(methodGroup);

            this.SetupFieldMock(list);
        }

        private async Task<GraphFieldSecurityContext> ExecuteTest()
        {
            var builder = new TestServerBuilder();
            var server = builder.Build();

            var defaultResult = new Mock<IAuthenticationResult>();
            defaultResult.Setup(x => x.Suceeded).Returns(false);

            var contextBuilder = server.CreateQueryContextBuilder();
            var queryContext = contextBuilder.Build();

            var fieldSecurityRequest = new Mock<IGraphFieldSecurityRequest>();
            fieldSecurityRequest.Setup(x => x.Field)
                .Returns(_field);

            var securityContext = new GraphFieldSecurityContext(queryContext, fieldSecurityRequest.Object);

            var component = new FieldSecurityRequirementsMiddleware(_policyProvider?.Object);
            await component.InvokeAsync(securityContext, this.EmptyNextDelegate);

            return securityContext;
        }

        [Test]
        public async Task SimpleAuthorizeOnMethod_BasicPropertyCheck()
        {
            this.SetupFieldForControllerMethod<NothingOnClass>(
                nameof(NothingOnClass.AuthorizeOnMethod));

            var result = await this.ExecuteTest();

            Assert.IsNotNull(result?.SecurityRequirements);
            Assert.IsNull(result.Result);
            Assert.IsFalse(result.SecurityRequirements.AllowAnonymous);

            CollectionAssert.IsEmpty(result.SecurityRequirements.AllowedAuthenticationSchemes);
            CollectionAssert.IsEmpty(result.SecurityRequirements.EnforcedRoleGroups);

            // enforces the default policy and no others present
            Assert.AreEqual(1, result.SecurityRequirements.EnforcedPolicies.Count());
            Assert.AreEqual(_defaultPolicy, result.SecurityRequirements.EnforcedPolicies.First().Policy);
        }

        [TestCase(typeof(NothingOnClass), nameof(NothingOnClass.AuthorizeOnMethod), false)]
        [TestCase(typeof(NothingOnClass), nameof(NothingOnClass.AnonOnMethod), true)]
        [TestCase(typeof(NothingOnClass), nameof(NothingOnClass.AnonAndAuthorizeOnMethod), true)]
        [TestCase(typeof(NothingOnClass), nameof(NothingOnClass.NothingOnMethod), true)]
        [TestCase(typeof(AuthorizeOnClass), nameof(AuthorizeOnClass.AuthorizeOnMethod), false)]
        [TestCase(typeof(AuthorizeOnClass), nameof(AuthorizeOnClass.AnonOnMethod), true)]
        [TestCase(typeof(AuthorizeOnClass), nameof(AuthorizeOnClass.AnonAndAuthorizeOnMethod), true)]
        [TestCase(typeof(AuthorizeOnClass), nameof(AuthorizeOnClass.NothingOnMethod), false)]
        [TestCase(typeof(AllowAnonymousOnClass), nameof(AllowAnonymousOnClass.AuthorizeOnMethod), true)]
        [TestCase(typeof(AllowAnonymousOnClass), nameof(AllowAnonymousOnClass.AnonOnMethod), true)]
        [TestCase(typeof(AllowAnonymousOnClass), nameof(AllowAnonymousOnClass.AnonAndAuthorizeOnMethod), true)]
        [TestCase(typeof(AllowAnonymousOnClass), nameof(AllowAnonymousOnClass.NothingOnMethod), true)]
        [TestCase(typeof(AuthorizeAndAnonOnClass), nameof(AuthorizeAndAnonOnClass.AuthorizeOnMethod), true)]
        [TestCase(typeof(AuthorizeAndAnonOnClass), nameof(AuthorizeAndAnonOnClass.AnonOnMethod), true)]
        [TestCase(typeof(AuthorizeAndAnonOnClass), nameof(AuthorizeAndAnonOnClass.AnonAndAuthorizeOnMethod), true)]
        [TestCase(typeof(AuthorizeAndAnonOnClass), nameof(AuthorizeAndAnonOnClass.NothingOnMethod), true)]
        public async Task AllowAnonymousChecks(Type controller, string methodName, bool shouldAllowAnonymous)
        {
            this.SetupFieldForControllerMethod(controller, methodName);

            var result = await this.ExecuteTest();

            Assert.IsNotNull(result?.SecurityRequirements);
            Assert.IsNull(result.Result);
            Assert.AreEqual(shouldAllowAnonymous, result.SecurityRequirements.AllowAnonymous);
        }

        [TestCase(nameof(SchemeOnClass.AllSchemesOnMethod), "scheme1, scheme2, scheme3")]
        [TestCase(nameof(SchemeOnClass.SubsetOnMethod), "scheme1, scheme2")]
        [TestCase(nameof(SchemeOnClass.SingleSchemeOnMethod), "scheme1")]
        [TestCase(nameof(SchemeOnClass.NoSchemesOnMethod), "scheme1, scheme2, scheme3")]
        public async Task WhenClassAndMethodSchemesDiffer_OnlyTheSubsetIsTaken(string methodName, string expectedSchemes)
        {
            this.SetupFieldForControllerMethod<SchemeOnClass>(methodName);

            var result = await this.ExecuteTest();

            var schemes = expectedSchemes
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList();

            Assert.IsNotNull(result?.SecurityRequirements);
            Assert.IsNull(result.Result);
            Assert.IsFalse(result.SecurityRequirements.AllowAnonymous);

            CollectionAssert.AreEquivalent(
                schemes,
                result.SecurityRequirements
                    .AllowedAuthenticationSchemes
                    .Select(x => x.AuthScheme)
                    .ToList());
        }

        [Test]
        public async Task WhenNoSchemeMatchesAllGroups_ErrorResultIsSet()
        {
            this.SetupFieldForControllerMethod<SchemeOnClass>(
                nameof(SchemeOnClass.DifferentSchemeOnMethod));

            var result = await this.ExecuteTest();

            Assert.AreEqual(FieldSecurityRequirements.AutoDeny, result.SecurityRequirements);
            Assert.IsFalse(result.SecurityRequirements.AllowAnonymous);

            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Failed, result.Result.Status);
        }

        [Test]
        public async Task WhenPolicyNotFound_ErrorResultIsSet()
        {
            this.SetupFieldForControllerMethod<NothingOnClass>(
                nameof(NothingOnClass.PolicyOnMethod));

            // no policy defined
            var result = await this.ExecuteTest();

            Assert.AreEqual(FieldSecurityRequirements.AutoDeny, result.SecurityRequirements);
            Assert.IsFalse(result.SecurityRequirements.AllowAnonymous);

            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Failed, result.Result.Status);
        }

        [Test]
        public async Task WhenPolicyDefined_ButProviderNotAssigned_ErrorResultIsSet()
        {
            this.SetupFieldForControllerMethod<NothingOnClass>(
                nameof(NothingOnClass.PolicyOnMethod));

            // no policy provider "found" in DI container
            _policyProvider = null;

            var result = await this.ExecuteTest();

            Assert.AreEqual(FieldSecurityRequirements.AutoDeny, result.SecurityRequirements);
            Assert.IsFalse(result.SecurityRequirements.AllowAnonymous);

            Assert.IsNotNull(result.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Failed, result.Result.Status);
        }

        [Test]
        public async Task CustomDefaultPolicyIsEnforced_WhenNoPolicyNameDefined()
        {
            this.SetupFieldForControllerMethod<NothingOnClass>(
                nameof(NothingOnClass.AuthorizeOnMethod));

            var schemes = new List<string>();
            schemes.Add("scheme1");
            schemes.Add("scheme2");

            // set custom policy with scheme
            _defaultPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(schemes.ToArray())
                .RequireAuthenticatedUser()
                .Build();

            var result = await this.ExecuteTest();

            Assert.IsNotNull(result.SecurityRequirements);
            Assert.IsFalse(result.SecurityRequirements.AllowAnonymous);

            Assert.IsNull(result.Result);

            CollectionAssert.AreEquivalent(
                schemes,
                result.SecurityRequirements
                    .AllowedAuthenticationSchemes
                    .Select(x => x.AuthScheme)
                    .ToList());
        }

        [Test]
        public async Task RoleGroups_OnClassAndMethod_AreAddedCorrectly()
        {
            this.SetupFieldForControllerMethod<RolesOnClass>(
                nameof(RolesOnClass.RolesOnMethod));

            var result = await this.ExecuteTest();

            Assert.IsNotNull(result.SecurityRequirements);
            Assert.IsFalse(result.SecurityRequirements.AllowAnonymous);

            Assert.IsNull(result.Result);

            Assert.AreEqual(2, result.SecurityRequirements.EnforcedRoleGroups.Count);

            var outer = result.SecurityRequirements.EnforcedRoleGroups[0];
            var inner = result.SecurityRequirements.EnforcedRoleGroups[1];

            CollectionAssert.AreEquivalent(
                new string[] { "role1", "role2" },
                outer);

            CollectionAssert.AreEquivalent(
                new string[] { "role2", "role3" },
                inner);
        }

        [Test]
        public async Task RoleGroups_OnClass_AreAddedCorrectly()
        {
            this.SetupFieldForControllerMethod<RolesOnClass>(
                nameof(RolesOnClass.NoRolesOnMethod));

            var result = await this.ExecuteTest();

            Assert.IsNotNull(result.SecurityRequirements);
            Assert.IsFalse(result.SecurityRequirements.AllowAnonymous);

            Assert.IsNull(result.Result);

            Assert.AreEqual(1, result.SecurityRequirements.EnforcedRoleGroups.Count);

            var outer = result.SecurityRequirements.EnforcedRoleGroups[0];

            CollectionAssert.AreEquivalent(
                new string[] { "role1", "role2" },
                outer);
        }

        [Test]
        public async Task RoleGroups_OnMethod_AreAddedCorrectly()
        {
            this.SetupFieldForControllerMethod<NothingOnClass>(
                nameof(NothingOnClass.RolesOnMethod));

            var result = await this.ExecuteTest();

            Assert.IsNotNull(result.SecurityRequirements);
            Assert.IsFalse(result.SecurityRequirements.AllowAnonymous);

            Assert.IsNull(result.Result);

            Assert.AreEqual(1, result.SecurityRequirements.EnforcedRoleGroups.Count);

            var inner = result.SecurityRequirements.EnforcedRoleGroups[0];

            CollectionAssert.AreEquivalent(
                new string[] { "role3", "role4" },
                inner);
        }

        [Test]
        public async Task NamedPolicyWithSchemes_SchemesAreExtracted()
        {
            this.SetupFieldForControllerMethod<NothingOnClass>(
                nameof(NothingOnClass.PolicyOnMethod));

            var policy1 = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes("scheme1", "scheme46")
                .Build();

            _policyProvider.Setup(x => x.GetPolicyAsync("Policy1"))
                .ReturnsAsync(policy1);

            var result = await this.ExecuteTest();

            Assert.IsNotNull(result.SecurityRequirements);
            Assert.IsFalse(result.SecurityRequirements.AllowAnonymous);

            Assert.IsNull(result.Result);

            CollectionAssert.AreEquivalent(
                policy1.AuthenticationSchemes,
                result.SecurityRequirements
                    .AllowedAuthenticationSchemes
                    .Select(x => x.AuthScheme)
                    .ToList());
        }

        [Test]
        public async Task WhenNoSecurityGroupsDefined_AutoDenyIsSet()
        {
            this.SetupFieldForControllerMethod<NothingOnClass>(
                nameof(NothingOnClass.AuthorizeOnMethod));

            this.SetupFieldMock(null);

            var result = await this.ExecuteTest();

            Assert.IsNotNull(result.SecurityRequirements);
            Assert.AreEqual(FieldSecurityRequirements.AutoDeny, result.SecurityRequirements);
            Assert.IsFalse(result.SecurityRequirements.AllowAnonymous);
            Assert.IsNull(result.Result);
        }
    }
}