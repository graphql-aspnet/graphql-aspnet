// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal
{
    using System;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Internal.Resolvers;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Internal.ValueResolversTestData;
    using NUnit.Framework;

    [TestFixture]
    public class ObjectMethodResolverTests
    {
        [Test]
        public async Task NullSourceData_FailsRequest()
        {
            var server = new TestServerBuilder()
                .AddGraphType<ResolverObject>()
                .Build();

            var builder = server.CreateFieldContextBuilder<ResolverObject>(
                nameof(ResolverObject.MethodRetrieveData));

            var resolver = new GraphObjectMethodResolver(builder.GraphMethod.Object);

            var context = builder.CreateResolutionContext();
            await resolver.Resolve(context);

            Assert.AreEqual(null, context.Result);
            Assert.IsFalse(context.Messages.IsSucessful);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_OBJECT, context.Messages[0].Code);
        }

        [Test]
        public async Task SourceDataIsNotOfTheTemplate_FailsRequest()
        {
            var server = new TestServerBuilder()
                .AddGraphType<ResolverObject>()
                .Build();

            var builder = server.CreateFieldContextBuilder<ResolverObject>(
                nameof(ResolverObject.MethodRetrieveData));

            // source data is not of the type the resolver is for
            builder.AddSourceData(new TwoPropertyObject());
            var resolver = new GraphObjectMethodResolver(builder.GraphMethod.Object);

            var context = builder.CreateResolutionContext();
            await resolver.Resolve(context);
            Assert.AreEqual(null, context.Result);
            Assert.IsFalse(context.Messages.IsSucessful);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_OBJECT, context.Messages[0].Code);
        }

        [Test]
        public async Task MethodThrowsException_FailsRequest()
        {
            var server = new TestServerBuilder()
                .AddGraphType<ResolverObject>()
                .Build();

            var builder = server.CreateFieldContextBuilder<ResolverObject>(
                nameof(ResolverObject.MethodThrowException));

            // source data is not of the type the resolver is for
            builder.AddSourceData(new ResolverObject());
            var resolver = new GraphObjectMethodResolver(builder.GraphMethod.Object);

            var context = builder.CreateResolutionContext();
            await resolver.Resolve(context);
            Assert.AreEqual(null, context.Result);
            Assert.IsFalse(context.Messages.IsSucessful);
            Assert.IsTrue(context.Messages[0].Exception is InvalidOperationException);
            Assert.AreEqual("resolver.method.throwException", context.Messages[0].Exception.Message);
            Assert.AreEqual(Constants.ErrorCodes.UNHANDLED_EXCEPTION, context.Messages[0].Code);
        }

        [Test]
        public async Task KnownExecutionError_FailsRequest()
        {
            var server = new TestServerBuilder()
                .AddGraphType<ResolverObject>()
                .Build();

            var builder = server.CreateFieldContextBuilder<ResolverObject>(
                nameof(ResolverObject.MethodWithArgument));

            // the method declares one input argument which is not provided on this request
            // resulting in a GraphExecutionException which
            // is absorbed into a message (with no attached exception)

            // source data is not of the type the resolver is for
            builder.AddSourceData(new ResolverObject());
            var resolver = new GraphObjectMethodResolver(builder.GraphMethod.Object);

            var context = builder.CreateResolutionContext();
            await resolver.Resolve(context);
            Assert.AreEqual(null, context.Result);
            Assert.IsFalse(context.Messages.IsSucessful);
            Assert.AreEqual(null, context.Messages[0].Exception);
            Assert.AreEqual(Constants.ErrorCodes.EXECUTION_ERROR, context.Messages[0].Code);
        }
    }
}