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
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal.Resolvers;
    using GraphQL.AspNet.Tests.Extensions.DiExtensionTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.AspNet.Tests.Internal.ValueResolversTestData;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ObjectPropertyResolverTests
    {
        [Test]
        public async Task NullSourceData_FailsRequest()
        {
            var server = new TestServerBuilder()
                .AddGraphType<ResolverObject>()
                .Build();

            var fieldContextBuilder = server.CreateFieldContextBuilder<ResolverObject>(
                nameof(ResolverObject.Address1),
                null);

            fieldContextBuilder.AddSourceData(null);
            var resolutionContext = fieldContextBuilder.CreateResolutionContext();

            var resolver = new GraphObjectPropertyResolver(fieldContextBuilder.GraphMethod.Object);
            await resolver.Resolve(resolutionContext);

            Assert.AreEqual(null, resolutionContext.Result);
            Assert.IsFalse(resolutionContext.Messages.IsSucessful);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_OBJECT, resolutionContext.Messages[0].Code);
        }

        [Test]
        public async Task TemplateIsInterface_SourceDataDoesImplementInterface_RendersCorrectly()
        {
            var server = new TestServerBuilder()
                .AddGraphType<ResolverObject>()
                .AddGraphType<IResolverInterface>()
                .Build();

            var fieldContextBuilder = server.CreateFieldContextBuilder<ResolverObject>(
                nameof(ResolverObject.Address1),
                null);

            var item = new ResolverObject();
            item.Address1 = "15th Street";
            fieldContextBuilder.AddSourceData(item);

            // set properties parent to be an interface that the source data is castable
            // this scenario shouldnt be possible in general execution but exists
            // in case of developers extending the framework
            var parentMock = new Mock<IGraphTypeTemplate>();
            parentMock.Setup(x => x.ObjectType).Returns(typeof(IResolverInterface));

            fieldContextBuilder.GraphMethod.Setup(x => x.Parent).Returns(parentMock.Object);
            var resolver = new GraphObjectPropertyResolver(fieldContextBuilder.GraphMethod.Object);

            var resolutionContext = fieldContextBuilder.CreateResolutionContext();
            await resolver.Resolve(resolutionContext);
            Assert.AreEqual("15th Street", resolutionContext.Result);
            Assert.IsTrue(resolutionContext.Messages.IsSucessful);
        }

        [Test]
        public async Task TemplateIsInterface_SourceDataDoesNotImplementInterface_FailsRequest()
        {
            var server = new TestServerBuilder()
                .AddGraphType<ResolverObject>()
                .Build();

            var fieldContextBuilder = server.CreateFieldContextBuilder<ResolverObject>(
                nameof(ResolverObject.Address1),
                null);

            fieldContextBuilder.AddSourceData(new TwoPropertyObject());

            // set properties parent to be an interface
            // that hte source data is not castable to
            var parentMock = new Mock<IGraphTypeTemplate>();
            parentMock.Setup(x => x.ObjectType).Returns(typeof(ITestInterface));

            fieldContextBuilder.GraphMethod.Setup(x => x.Parent).Returns(parentMock.Object);

            var resolver = new GraphObjectPropertyResolver(fieldContextBuilder.GraphMethod.Object);

            var resolutionContext = fieldContextBuilder.CreateResolutionContext();
            await resolver.Resolve(resolutionContext);
            Assert.AreEqual(null, resolutionContext.Result);
            Assert.IsFalse(resolutionContext.Messages.IsSucessful);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_OBJECT, resolutionContext.Messages[0].Code);
        }

        [Test]
        public async Task SourceDataIsNotOfTheTemplate_FailsRequest()
        {
            var server = new TestServerBuilder()
                .AddGraphType<ResolverObject>()
                .Build();

            // resolving structA, but supplying structB as source
            var fieldContextBuilder = server.CreateFieldContextBuilder<ResolverStructA>(
                nameof(ResolverStructA.Prop1),
                new ResolverStructB("struct"));

            // source data is not of the type the resolver is for
            var resolver = new GraphObjectPropertyResolver(fieldContextBuilder.GraphMethod.Object);

            var resolutionContext = fieldContextBuilder.CreateResolutionContext();
            await resolver.Resolve(resolutionContext);
            Assert.AreEqual(null, resolutionContext.Result);
            Assert.IsFalse(resolutionContext.Messages.IsSucessful);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_OBJECT, resolutionContext.Messages[0].Code);
        }

        [Test]
        public async Task PropertyThrowsException_FailsRequest()
        {
            var server = new TestServerBuilder()
                .AddGraphType<ResolverObject>()
                .Build();

            var fieldContextBuilder = server.CreateFieldContextBuilder<ResolverObject>(
                nameof(ResolverObject.PropertyThrowException),
                new ResolverObject());

            // source data is not of the type the resolver is for
            fieldContextBuilder.AddSourceData(new ResolverObject());
            var resolver = new GraphObjectPropertyResolver(fieldContextBuilder.GraphMethod.Object);

            var resolutionContext = fieldContextBuilder.CreateResolutionContext();
            await resolver.Resolve(resolutionContext);
            Assert.AreEqual(null, resolutionContext.Result);
            Assert.IsFalse(resolutionContext.Messages.IsSucessful);
            Assert.IsTrue(resolutionContext.Messages[0].Exception is InvalidOperationException);
            Assert.AreEqual("resolver.property.throwException", resolutionContext.Messages[0].Exception.Message);
            Assert.AreEqual(Constants.ErrorCodes.UNHANDLED_EXCEPTION, resolutionContext.Messages[0].Code);
        }

        [Test]
        public async Task AsyncProperty_ValidSourceData_ReturnsData()
        {
            var server = new TestServerBuilder()
                .AddGraphType<ResolverObject>()
                .Build();

            var fieldContextBuilder = server.CreateFieldContextBuilder<ResolverObject>(
                nameof(ResolverObject.Address1Async),
                new ResolverObject());

            var resolutionContext = fieldContextBuilder.CreateResolutionContext();

            var resolver = new GraphObjectPropertyResolver(fieldContextBuilder.GraphMethod.Object);
            await resolver.Resolve(resolutionContext);

            Assert.IsNotNull(resolutionContext.Result);
            Assert.True(resolutionContext.Messages.IsSucessful);
            Assert.AreEqual("AddressAsync", resolutionContext.Result?.ToString());
        }

        [Test]
        public async Task AsyncProperty_ThrowsException_FailsRequest()
        {
            var server = new TestServerBuilder()
                .AddGraphType<ResolverObject>()
                .Build();

            var fieldContextBuilder = server.CreateFieldContextBuilder<ResolverObject>(
                nameof(ResolverObject.AsyncPropException),
                new ResolverObject());

            // source data is not of the type the resolver is for
            fieldContextBuilder.AddSourceData(new ResolverObject());
            var resolver = new GraphObjectPropertyResolver(fieldContextBuilder.GraphMethod.Object);

            var resolutionContext = fieldContextBuilder.CreateResolutionContext();
            await resolver.Resolve(resolutionContext);
            Assert.AreEqual(null, resolutionContext.Result);
            Assert.IsFalse(resolutionContext.Messages.IsSucessful);
            Assert.IsTrue(resolutionContext.Messages[0].Exception is InvalidOperationException);
            Assert.AreEqual("resolver.property.throwException", resolutionContext.Messages[0].Exception.Message);
            Assert.AreEqual(Constants.ErrorCodes.UNHANDLED_EXCEPTION, resolutionContext.Messages[0].Code);
        }
    }
}