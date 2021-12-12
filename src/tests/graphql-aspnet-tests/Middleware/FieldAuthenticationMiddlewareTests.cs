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
        public async Task WhenNullSecurityGroupsOnField_DefaultUserOncontextIsReturned()
        {
            var builder = new TestServerBuilder();
            builder.UserContext.Authenticate();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder().Build();
            var expectedUser = queryContext.SecurityContext.DefaultUser;

            var field = new Mock<IGraphField>();
            field.Setup(x => x.SecurityGroups).Returns(null as IEnumerable<FieldSecurityGroup>);

            var fieldSecurityRequest = new Mock<IGraphFieldSecurityRequest>();
            fieldSecurityRequest.Setup(x => x.Field)
                .Returns(field.Object);

            var securityContext = new GraphFieldSecurityContext(queryContext, fieldSecurityRequest.Object);

            var middleware = new FieldAuthenticationMiddleware();
            await middleware.InvokeAsync(securityContext, this.EmptyNextDelegate);

            Assert.IsNotNull(securityContext.AuthenticatedUser);
            Assert.AreEqual(expectedUser, securityContext.AuthenticatedUser);
            Assert.IsNull(securityContext.Result);
        }

        [Test]
        public async Task WhenEmptySecurityGroupsOnField_DefaultUserOncontextIsReturned()
        {
            var builder = new TestServerBuilder();
            builder.UserContext.Authenticate();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder().Build();
            var expectedUser = queryContext.SecurityContext.DefaultUser;

            var field = new Mock<IGraphField>();
            field.Setup(x => x.SecurityGroups).Returns(Enumerable.Empty<FieldSecurityGroup>());

            var fieldSecurityRequest = new Mock<IGraphFieldSecurityRequest>();
            fieldSecurityRequest.Setup(x => x.Field)
                .Returns(field.Object);

            var securityContext = new GraphFieldSecurityContext(queryContext, fieldSecurityRequest.Object);

            var middleware = new FieldAuthenticationMiddleware();
            await middleware.InvokeAsync(securityContext, this.EmptyNextDelegate);

            Assert.IsNotNull(securityContext.AuthenticatedUser);
            Assert.AreEqual(expectedUser, securityContext.AuthenticatedUser);
            Assert.IsNull(securityContext.Result);
        }

        [Test]
        public async Task WhenNoRequiredSchemesOnSecurityGroupsOnField_DefaultUserOncontextIsReturned()
        {
            var builder = new TestServerBuilder();
            builder.UserContext.Authenticate();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder().Build();
            var expectedUser = queryContext.SecurityContext.DefaultUser;

            var testGroup = FieldSecurityGroup.FromAttributeCollection(typeof(NoRequriedSchemeOnAuthorize));

            var field = new Mock<IGraphField>();
            field.Setup(x => x.SecurityGroups).Returns(testGroup.AsEnumerable<FieldSecurityGroup>());

            var fieldSecurityRequest = new Mock<IGraphFieldSecurityRequest>();
            fieldSecurityRequest.Setup(x => x.Field)
                .Returns(field.Object);

            var securityContext = new GraphFieldSecurityContext(queryContext, fieldSecurityRequest.Object);

            var middleware = new FieldAuthenticationMiddleware();
            await middleware.InvokeAsync(securityContext, this.EmptyNextDelegate);

            Assert.IsNotNull(securityContext.AuthenticatedUser);
            Assert.AreEqual(expectedUser, securityContext.AuthenticatedUser);
            Assert.IsNull(securityContext.Result);
        }

        [Test]
        public async Task WhenAllowsAnonymousOnSecurityGroupsOnField_DefaultUserOncontextIsReturned()
        {
            var builder = new TestServerBuilder();
            builder.UserContext.Authenticate();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder().Build();
            var expectedUser = queryContext.SecurityContext.DefaultUser;

            var testGroup = FieldSecurityGroup.FromAttributeCollection(typeof(AllowAnonymousOnAuthorize));

            var field = new Mock<IGraphField>();
            field.Setup(x => x.SecurityGroups).Returns(testGroup.AsEnumerable<FieldSecurityGroup>());

            var fieldSecurityRequest = new Mock<IGraphFieldSecurityRequest>();
            fieldSecurityRequest.Setup(x => x.Field)
                .Returns(field.Object);

            var securityContext = new GraphFieldSecurityContext(queryContext, fieldSecurityRequest.Object);

            var middleware = new FieldAuthenticationMiddleware();
            await middleware.InvokeAsync(securityContext, this.EmptyNextDelegate);

            Assert.IsNotNull(securityContext.AuthenticatedUser);
            Assert.AreEqual(expectedUser, securityContext.AuthenticatedUser);
            Assert.IsNull(securityContext.Result);
        }

        [Test]
        public async Task WhenSecurityGroupsOnField_ButNoUserContext_NotAuthenticated()
        {
            var builder = new TestServerBuilder();
            var server = builder.Build();

            var contextBuilder = server.CreateQueryContextBuilder();
            contextBuilder.AddSecurityContext(null);
            var queryContext = contextBuilder.Build();

            var testGroup = FieldSecurityGroup.FromAttributeCollection(typeof(NoRequriedSchemeOnAuthorize));

            var field = new Mock<IGraphField>();
            field.Setup(x => x.SecurityGroups).Returns(testGroup.AsEnumerable<FieldSecurityGroup>());

            var fieldSecurityRequest = new Mock<IGraphFieldSecurityRequest>();
            fieldSecurityRequest.Setup(x => x.Field)
                .Returns(field.Object);

            var securityContext = new GraphFieldSecurityContext(queryContext, fieldSecurityRequest.Object);

            var middleware = new FieldAuthenticationMiddleware();
            await middleware.InvokeAsync(securityContext, this.EmptyNextDelegate);

            Assert.IsNull(securityContext.AuthenticatedUser);
            Assert.IsNotNull(securityContext.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Failed, securityContext.Result.Status);
        }

        [Test]
        public async Task WhenSecurityGroupsOnFieldWithRequiredScheme_ButUserContextDoesntMatch_NotAuthenticated()
        {
            var builder = new TestServerBuilder();
            builder.UserContext.Authenticate("testScheme");
            var server = builder.Build();

            var contextBuilder = server.CreateQueryContextBuilder();
            var queryContext = contextBuilder.Build();

            // has "testScheme2" required
            var testGroup = FieldSecurityGroup.FromAttributeCollection(typeof(WithRequiredUnmatchedSchemeOnAuthorize));

            var field = new Mock<IGraphField>();
            field.Setup(x => x.SecurityGroups).Returns(testGroup.AsEnumerable<FieldSecurityGroup>());

            var fieldSecurityRequest = new Mock<IGraphFieldSecurityRequest>();
            fieldSecurityRequest.Setup(x => x.Field)
                .Returns(field.Object);

            var securityContext = new GraphFieldSecurityContext(queryContext, fieldSecurityRequest.Object);

            var middleware = new FieldAuthenticationMiddleware();
            await middleware.InvokeAsync(securityContext, this.EmptyNextDelegate);

            Assert.IsNull(securityContext.AuthenticatedUser);
            Assert.IsNotNull(securityContext.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Unauthenticated, securityContext.Result.Status);
        }

        [Test]
        public async Task WhenSecurityGroupsOnFieldWithRequiredScheme_AndUserContextMatches_IsAuthenticated()
        {
            var builder = new TestServerBuilder();
            builder.UserContext.Authenticate("testScheme");
            var server = builder.Build();

            var contextBuilder = server.CreateQueryContextBuilder();
            var queryContext = contextBuilder.Build();

            var expectedUser = queryContext.SecurityContext.DefaultUser;

            // has "testScheme" required
            var testGroup = FieldSecurityGroup.FromAttributeCollection(typeof(WithRequiredMatchedSchemeOnAuthorize));

            var field = new Mock<IGraphField>();
            field.Setup(x => x.SecurityGroups).Returns(testGroup.AsEnumerable<FieldSecurityGroup>());

            var fieldSecurityRequest = new Mock<IGraphFieldSecurityRequest>();
            fieldSecurityRequest.Setup(x => x.Field)
                .Returns(field.Object);

            var securityContext = new GraphFieldSecurityContext(queryContext, fieldSecurityRequest.Object);

            var middleware = new FieldAuthenticationMiddleware();
            await middleware.InvokeAsync(securityContext, this.EmptyNextDelegate);

            Assert.IsNotNull(securityContext.AuthenticatedUser);
            Assert.AreEqual(expectedUser, securityContext.AuthenticatedUser);
            Assert.IsNull(securityContext.Result);
        }

        [Test]
        public async Task WhenNestedMismatchedGroupsEncountered_AuthIsFailed()
        {
            var builder = new TestServerBuilder();
            builder.UserContext.Authenticate("testScheme");
            var server = builder.Build();

            var contextBuilder = server.CreateQueryContextBuilder();
            var queryContext = contextBuilder.Build();

            var expectedUser = queryContext.SecurityContext.DefaultUser;

            // has "testScheme" required
            var testGroupTop = FieldSecurityGroup.FromAttributeCollection(typeof(WithNestedMisMatchSchemesOnAuthorize));
            var testGroupInner = FieldSecurityGroup.FromAttributeCollection(typeof(WithNestedMisMatchSchemesOnAuthorize).GetMethod(nameof(WithNestedMisMatchSchemesOnAuthorize.TestMethod)));

            var field = new Mock<IGraphField>();
            field.Setup(x => x.SecurityGroups).Returns(new List<FieldSecurityGroup>() { testGroupTop, testGroupInner });
            field.Setup(x => x.Route).Returns(new GraphFieldPath(AspNet.Execution.GraphCollection.Types, "segment1"));

            var fieldSecurityRequest = new Mock<IGraphFieldSecurityRequest>();
            fieldSecurityRequest.Setup(x => x.Field)
                .Returns(field.Object);

            var securityContext = new GraphFieldSecurityContext(queryContext, fieldSecurityRequest.Object);

            var middleware = new FieldAuthenticationMiddleware();
            await middleware.InvokeAsync(securityContext, this.EmptyNextDelegate);

            Assert.IsNull(securityContext.AuthenticatedUser);
            Assert.IsNotNull(securityContext.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Failed, securityContext.Result.Status);
        }

        [Test]
        public async Task WhenNestedGroupsEncountered_AndUserContextMatches_IsAuthorized()
        {
            var builder = new TestServerBuilder();
            builder.UserContext.Authenticate("testScheme1");
            var server = builder.Build();

            var contextBuilder = server.CreateQueryContextBuilder();
            var queryContext = contextBuilder.Build();

            var expectedUser = queryContext.SecurityContext.DefaultUser;

            // has "testScheme1" required
            var testGroupTop = FieldSecurityGroup.FromAttributeCollection(typeof(WithNestedMatchedSchemesOnAuthorize));
            var testGroupInner = FieldSecurityGroup.FromAttributeCollection(typeof(WithNestedMatchedSchemesOnAuthorize).GetMethod(nameof(WithNestedMatchedSchemesOnAuthorize.TestMethod)));

            var field = new Mock<IGraphField>();
            field.Setup(x => x.SecurityGroups).Returns(new List<FieldSecurityGroup>() { testGroupTop, testGroupInner });
            field.Setup(x => x.Route).Returns(new GraphFieldPath(AspNet.Execution.GraphCollection.Types, "segment1"));

            var fieldSecurityRequest = new Mock<IGraphFieldSecurityRequest>();
            fieldSecurityRequest.Setup(x => x.Field)
                .Returns(field.Object);

            var securityContext = new GraphFieldSecurityContext(queryContext, fieldSecurityRequest.Object);

            var middleware = new FieldAuthenticationMiddleware();
            await middleware.InvokeAsync(securityContext, this.EmptyNextDelegate);

            Assert.IsNotNull(securityContext.AuthenticatedUser);
            Assert.AreEqual(expectedUser, securityContext.AuthenticatedUser);
            Assert.IsNull(securityContext.Result);
        }

        [Test]
        public async Task WhenFailsDefaultScheme_AndNoOtherSchemes_FailsToAuthenticate()
        {
            var builder = new TestServerBuilder();
            var server = builder.Build();

            var defaultResult = new Mock<IAuthenticationResult>();
            defaultResult.Setup(x => x.Suceeded).Returns(false);

            var userSecurityContext = new Mock<IUserSecurityContext>();
            userSecurityContext.Setup(x => x.Authenticate(It.IsAny<CancellationToken>()))
                .ReturnsAsync(defaultResult.Object);

            userSecurityContext.Setup(x => x.Authenticate(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("unknown scheme"));

            var contextBuilder = server.CreateQueryContextBuilder();
            contextBuilder.AddSecurityContext(userSecurityContext.Object);
            var queryContext = contextBuilder.Build();

            // has "default" required
            var testGroup = FieldSecurityGroup.FromAttributeCollection(typeof(NoRequriedSchemeOnAuthorize));

            var field = new Mock<IGraphField>();
            field.Setup(x => x.SecurityGroups).Returns(new List<FieldSecurityGroup>() { testGroup });
            field.Setup(x => x.Route).Returns(new GraphFieldPath(AspNet.Execution.GraphCollection.Types, "segment1"));

            var fieldSecurityRequest = new Mock<IGraphFieldSecurityRequest>();
            fieldSecurityRequest.Setup(x => x.Field)
                .Returns(field.Object);

            var securityContext = new GraphFieldSecurityContext(queryContext, fieldSecurityRequest.Object);

            var middleware = new FieldAuthenticationMiddleware();
            await middleware.InvokeAsync(securityContext, this.EmptyNextDelegate);

            Assert.IsNull(securityContext.AuthenticatedUser);
            Assert.IsNotNull(securityContext.Result);
            Assert.AreEqual(FieldSecurityChallengeStatus.Unauthenticated, securityContext.Result.Status);
        }
    }
}