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
    using GraphQL.AspNet.Common.Extensions;
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
    public class FieldAuthenticationMiddlewareTests
    {
        public Task EmptyNextDelegate(GraphFieldSecurityContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        [Test]
        public async Task WhenDefaultSchemeIsUsed_DefaultUserOncontextIsReturned()
        {
            var builder = new TestServerBuilder();
            builder.UserContext.Authenticate();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder().Build();
            var expectedUser = (await queryContext.SecurityContext.Authenticate(builder.Authentication.DefaultAuthScheme))?.User;

            var testGroup = AppliedSecurityPolicyGroup.FromAttributeCollection(typeof(NoRequriedSchemeOnAuthorize));

            var field = new Mock<IGraphField>();
            field.Setup(x => x.SecurityGroups).Returns(testGroup.AsEnumerable<AppliedSecurityPolicyGroup>());

            var fieldSecurityRequest = new Mock<IGraphFieldSecurityRequest>();
            fieldSecurityRequest.Setup(x => x.Field)
                .Returns(field.Object);

            var requirements = new FieldSecurityRequirementsBuilder()
                .Build();

            var securityContext = new GraphFieldSecurityContext(queryContext, fieldSecurityRequest.Object);
            securityContext.SecurityRequirements = requirements;

            var middleware = new FieldAuthenticationMiddleware();
            await middleware.InvokeAsync(securityContext, this.EmptyNextDelegate);

            Assert.IsNotNull(securityContext.AuthenticatedUser);
            Assert.AreEqual(expectedUser, securityContext.AuthenticatedUser);
            Assert.IsNull(securityContext.Result);
        }

        [Test]
        public async Task WhenAllowsAnonymous_DefaultUserOnContextIsReturned_WhenFound()
        {
            var builder = new TestServerBuilder();
            builder.UserContext.Authenticate();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder().Build();
            var expectedUser = queryContext.SecurityContext.DefaultUser;

            var testGroup = AppliedSecurityPolicyGroup.FromAttributeCollection(typeof(AllowAnonymousOnAuthorize));

            var field = new Mock<IGraphField>();
            field.Setup(x => x.SecurityGroups).Returns(testGroup.AsEnumerable<AppliedSecurityPolicyGroup>());

            var fieldSecurityRequest = new Mock<IGraphFieldSecurityRequest>();
            fieldSecurityRequest.Setup(x => x.Field)
                .Returns(field.Object);

            var securityContext = new GraphFieldSecurityContext(queryContext, fieldSecurityRequest.Object);
            securityContext.SecurityRequirements = new FieldSecurityRequirementsBuilder()
                .AllowAnonymousUsers()
                .Build();

            var middleware = new FieldAuthenticationMiddleware();
            await middleware.InvokeAsync(securityContext, this.EmptyNextDelegate);

            Assert.IsNotNull(securityContext.AuthenticatedUser);
            Assert.AreEqual(expectedUser, securityContext.AuthenticatedUser);
            Assert.IsNull(securityContext.Result);
        }

        [Test]
        public async Task WhenAllowsAnonymous_ButNoAuthenticatedUserFound_ProcessingIsNotStopped()
        {
            var builder = new TestServerBuilder();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder().Build();

            var testGroup = AppliedSecurityPolicyGroup.FromAttributeCollection(typeof(AllowAnonymousOnAuthorize));

            var field = new Mock<IGraphField>();
            field.Setup(x => x.SecurityGroups).Returns(testGroup.AsEnumerable<AppliedSecurityPolicyGroup>());

            var fieldSecurityRequest = new Mock<IGraphFieldSecurityRequest>();
            fieldSecurityRequest.Setup(x => x.Field)
                .Returns(field.Object);

            var requirements = new FieldSecurityRequirementsBuilder()
                .AllowAnonymousUsers()
                .Build();

            var securityContext = new GraphFieldSecurityContext(queryContext, fieldSecurityRequest.Object);
            securityContext.SecurityRequirements = requirements;

            var middleware = new FieldAuthenticationMiddleware();
            await middleware.InvokeAsync(securityContext, this.EmptyNextDelegate);

            Assert.IsNull(securityContext.AuthenticatedUser);
            Assert.IsNull(securityContext.Result);
        }

        [Test]
        public async Task WhenRequirementsExist_ButNoUserContext_NotAuthenticated()
        {
            var builder = new TestServerBuilder();
            var server = builder.Build();

            var contextBuilder = server.CreateQueryContextBuilder();
            contextBuilder.AddSecurityContext(null); // no context
            var queryContext = contextBuilder.Build();

            var testGroup = AppliedSecurityPolicyGroup.FromAttributeCollection(typeof(NoRequriedSchemeOnAuthorize));

            var field = new Mock<IGraphField>();
            var fieldSecurityRequest = new Mock<IGraphFieldSecurityRequest>();
            fieldSecurityRequest.Setup(x => x.Field)
                .Returns(field.Object);

            var requirements = new FieldSecurityRequirementsBuilder()
                .Build();

            var securityContext = new GraphFieldSecurityContext(queryContext, fieldSecurityRequest.Object);
            securityContext.SecurityRequirements = requirements;

            var middleware = new FieldAuthenticationMiddleware();
            await middleware.InvokeAsync(securityContext, this.EmptyNextDelegate);

            Assert.IsNull(securityContext.AuthenticatedUser);
            Assert.IsNotNull(securityContext.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Failed, securityContext.Result.Status);
        }

        [Test]
        public async Task WhenSpecificSchemeRequired_AndUserContextDoesNotMatch_IsNotAuthorized()
        {
            var builder = new TestServerBuilder();
            builder.UserContext.Authenticate(authScheme: "testScheme");
            var server = builder.Build();

            var contextBuilder = server.CreateQueryContextBuilder();
            var queryContext = contextBuilder.Build();

            // does not match auth's scheme
            var requirements = new FieldSecurityRequirementsBuilder()
                .AddAllowedAuthenticationScheme("testScheme2")
                .Build();

            var field = new Mock<IGraphField>();

            var fieldSecurityRequest = new Mock<IGraphFieldSecurityRequest>();
            fieldSecurityRequest.Setup(x => x.Field)
                .Returns(field.Object);

            var securityContext = new GraphFieldSecurityContext(queryContext, fieldSecurityRequest.Object);
            securityContext.SecurityRequirements = requirements;

            var middleware = new FieldAuthenticationMiddleware();
            await middleware.InvokeAsync(securityContext, this.EmptyNextDelegate);

            Assert.IsNull(securityContext.AuthenticatedUser);
            Assert.IsNotNull(securityContext.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Unauthenticated, securityContext.Result.Status);
        }

        [Test]
        public async Task WhenSpecificSchemeRequired_AndUserContextMatches_IsAuthorized()
        {
            var builder = new TestServerBuilder();
            builder.UserContext.Authenticate(authScheme: "testScheme1");
            var server = builder.Build();

            var contextBuilder = server.CreateQueryContextBuilder();
            var queryContext = contextBuilder.Build();

            var expectedUser = (await queryContext.SecurityContext.Authenticate("testScheme1"))?.User;

            var requirements = new FieldSecurityRequirementsBuilder()
                .AddAllowedAuthenticationScheme("testScheme1")
                .Build();

            var field = new Mock<IGraphField>();
            field.Setup(x => x.Route).Returns(new GraphFieldPath(AspNet.Execution.GraphCollection.Types, "segment1"));

            var fieldSecurityRequest = new Mock<IGraphFieldSecurityRequest>();
            fieldSecurityRequest.Setup(x => x.Field)
                .Returns(field.Object);

            var securityContext = new GraphFieldSecurityContext(queryContext, fieldSecurityRequest.Object);
            securityContext.SecurityRequirements = requirements;

            var middleware = new FieldAuthenticationMiddleware();
            await middleware.InvokeAsync(securityContext, this.EmptyNextDelegate);

            Assert.IsNotNull(securityContext.AuthenticatedUser);
            Assert.AreEqual(expectedUser, securityContext.AuthenticatedUser);
            Assert.IsNull(securityContext.Result);
        }

        [Test]
        public async Task WhenDefaultSchemeFails_AndOthersNotAllowed_FailsToAuthenticate()
        {
            var builder = new TestServerBuilder();
            var server = builder.Build();

            var defaultResult = new Mock<IAuthenticationResult>();
            defaultResult.Setup(x => x.Suceeded).Returns(false);

            // fails for default
            var userSecurityContext = new Mock<IUserSecurityContext>();
            userSecurityContext.Setup(x => x.Authenticate(It.IsAny<CancellationToken>()))
                .ReturnsAsync(defaultResult.Object);

            // exception for specific scheme if its inadverantly called (will fail test)
            userSecurityContext.Setup(x => x.Authenticate(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("unknown scheme"));

            var contextBuilder = server.CreateQueryContextBuilder();
            contextBuilder.AddSecurityContext(userSecurityContext.Object);
            var queryContext = contextBuilder.Build();

            // just basic authentication using the default scheme
            var requirements = new FieldSecurityRequirementsBuilder()
                .Build();

            var field = new Mock<IGraphField>();
            field.Setup(x => x.Route).Returns(new GraphFieldPath(AspNet.Execution.GraphCollection.Types, "segment1"));

            var fieldSecurityRequest = new Mock<IGraphFieldSecurityRequest>();
            fieldSecurityRequest.Setup(x => x.Field)
                .Returns(field.Object);

            var securityContext = new GraphFieldSecurityContext(queryContext, fieldSecurityRequest.Object);
            securityContext.SecurityRequirements = requirements;

            var middleware = new FieldAuthenticationMiddleware();
            await middleware.InvokeAsync(securityContext, this.EmptyNextDelegate);

            Assert.IsNull(securityContext.AuthenticatedUser);
            Assert.IsNotNull(securityContext.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Unauthenticated, securityContext.Result.Status);
        }
    }
}