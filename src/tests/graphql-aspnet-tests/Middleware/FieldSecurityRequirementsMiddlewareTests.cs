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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Controllers;
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
            var method = typeof(TController).GetMethod(methodName);
            if (method == null)
                Assert.Fail("Invalid Method, can't create test field");

            var controllerGroup = FieldSecurityAppliedPolicyGroup.FromAttributeCollection(typeof(TController));
            var methodGroup = FieldSecurityAppliedPolicyGroup.FromAttributeCollection(method);

            var list = new List<FieldSecurityAppliedPolicyGroup>();
            list.Add(controllerGroup);
            list.Add(methodGroup);

            var field = new Mock<IGraphField>();
            field.Setup(x => x.SecurityGroups).Returns(list);
            field.Setup(x => x.Route).Returns(new GraphFieldPath(AspNet.Execution.GraphCollection.Query, "some", "path"));

            _field = field.Object;
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

            var component = new FieldSecurityRequirementsMiddleware(_policyProvider.Object);
            await component.InvokeAsync(securityContext, this.EmptyNextDelegate);

            return securityContext;
        }

        [Test]
        public async Task BasicAuthorize_NoSpecificSchemeOnDefaultPolicy_FallbackisAllowed()
        {
            this.SetupFieldForControllerMethod<NoRequriedSchemeOnAuthorize>(nameof(NoRequriedSchemeOnAuthorize.BasicAuthorize));

            var result = await this.ExecuteTest();

            Assert.IsNotNull(result?.SecurityRequirements);
            Assert.IsFalse(result.SecurityRequirements.AllowAnonymous);

            CollectionAssert.IsEmpty(result.SecurityRequirements.AllowedAuthenticationSchemes);
            CollectionAssert.IsEmpty(result.SecurityRequirements.EnforcedRoleGroups);

            // enforces the default policy and no others present
            Assert.AreEqual(1, result.SecurityRequirements.EnforcedPolicies.Count());
            Assert.AreEqual(_defaultPolicy, result.SecurityRequirements.EnforcedPolicies.First().Policy);
        }
    }
}