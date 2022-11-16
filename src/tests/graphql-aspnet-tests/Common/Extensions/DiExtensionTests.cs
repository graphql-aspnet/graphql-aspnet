// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Extensions
{
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Tests.Extensions.DiExtensionTestData;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class DiExtensionTests
    {
        [Test]
        public void Replace_TypeReference_ExistingServiceDescriptor_IsReplaced()
        {
            IServiceCollection collection = new ServiceCollection();

            collection.AddScoped<ITestInterface, TestObjectA>();

            var descriptor = collection[0];
            Assert.AreEqual(typeof(ITestInterface), descriptor.ServiceType);
            Assert.AreEqual(typeof(TestObjectA), descriptor.ImplementationType);
            Assert.AreEqual(ServiceLifetime.Scoped, descriptor.Lifetime);

            collection.Replace<ITestInterface, TestObjectB>(ServiceLifetime.Singleton);

            descriptor = collection[0];
            Assert.AreEqual(typeof(ITestInterface), descriptor.ServiceType);
            Assert.AreEqual(typeof(TestObjectB), descriptor.ImplementationType);
            Assert.AreEqual(ServiceLifetime.Singleton, descriptor.Lifetime);
        }

        [Test]
        public void Replace_TypeReference_UpdatingScope_IsReplaced()
        {
            IServiceCollection collection = new ServiceCollection();

            collection.AddScoped<ITestInterface, TestObjectA>();

            var descriptor = collection[0];
            Assert.AreEqual(typeof(ITestInterface), descriptor.ServiceType);
            Assert.AreEqual(typeof(TestObjectA), descriptor.ImplementationType);
            Assert.AreEqual(ServiceLifetime.Scoped, descriptor.Lifetime);

            collection.Replace<ITestInterface, TestObjectA>(ServiceLifetime.Transient);

            descriptor = collection[0];
            Assert.AreEqual(typeof(ITestInterface), descriptor.ServiceType);
            Assert.AreEqual(typeof(TestObjectA), descriptor.ImplementationType);
            Assert.AreEqual(ServiceLifetime.Transient, descriptor.Lifetime);
        }

        [Test]
        public void Replace_TypeReference_NonExistingType_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                IServiceCollection collection = new ServiceCollection();
                collection.Replace<ITestInterface, TestObjectB>(ServiceLifetime.Singleton);
            });
        }

        [Test]
        public void Replace_ServiceFactory_ExistingServiceDescriptor_IsReplaced()
        {
            IServiceCollection collection = new ServiceCollection();
            Func<IServiceProvider, ITestInterface> factory = (IServiceProvider sp) => new TestObjectA();
            Func<IServiceProvider, ITestInterface> factory2 = (IServiceProvider sp) => new TestObjectB();

            collection.AddScoped(factory);

            var descriptor = collection[0];
            Assert.AreEqual(typeof(ITestInterface), descriptor.ServiceType);
            Assert.AreEqual(null, descriptor.ImplementationType);
            Assert.AreEqual(factory, descriptor.ImplementationFactory);
            Assert.AreEqual(ServiceLifetime.Scoped, descriptor.Lifetime);

            collection.Replace(factory2, ServiceLifetime.Transient);

            descriptor = collection[0];
            Assert.AreEqual(null, descriptor.ImplementationType);
            Assert.AreEqual(factory2, descriptor.ImplementationFactory);
            Assert.AreEqual(ServiceLifetime.Transient, descriptor.Lifetime);
        }

        [Test]
        public void Replace_ServiceFactory_UpdatingScope_IsReplaced()
        {
            IServiceCollection collection = new ServiceCollection();
            Func<IServiceProvider, ITestInterface> factory = (IServiceProvider sp) => new TestObjectA();

            collection.AddScoped(factory);

            var descriptor = collection[0];
            Assert.AreEqual(typeof(ITestInterface), descriptor.ServiceType);
            Assert.AreEqual(null, descriptor.ImplementationType);
            Assert.AreEqual(factory, descriptor.ImplementationFactory);
            Assert.AreEqual(ServiceLifetime.Scoped, descriptor.Lifetime);

            collection.Replace(factory, ServiceLifetime.Transient);

            descriptor = collection[0];
            Assert.AreEqual(null, descriptor.ImplementationType);
            Assert.AreEqual(factory, descriptor.ImplementationFactory);
            Assert.AreEqual(ServiceLifetime.Transient, descriptor.Lifetime);
        }

        [Test]
        public void Replace_ServiceFactory_NonExistingType_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                IServiceCollection collection = new ServiceCollection();
                collection.Replace<ITestInterface>((sp) => new TestObjectA(), ServiceLifetime.Scoped);
            });
        }
    }
}