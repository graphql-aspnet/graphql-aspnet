// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Configuration
{
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class ServiceToRegisterTests
    {
        [Test]
        public void SingleSuppliedType_IsBothTheImplementationAndServiceType()
        {
            var result = new ServiceToRegister(
                typeof(ServiceToRegisterTests),
                ServiceLifetime.Scoped,
                true);

            Assert.AreEqual(typeof(ServiceToRegisterTests), result.ServiceType);
            Assert.AreEqual(typeof(ServiceToRegisterTests), result.ImplementationType);
            Assert.AreEqual(ServiceLifetime.Scoped, result.ServiceLifeTime);
            Assert.IsTrue(result.Required);
        }

        [Test]
        public void PropertyCheck()
        {
            var result = new ServiceToRegister(
                typeof(ITwoPropertyObject),
                typeof(TwoPropertyObject),
                ServiceLifetime.Scoped,
                true);

            Assert.AreEqual(typeof(ITwoPropertyObject), result.ServiceType);
            Assert.AreEqual(typeof(TwoPropertyObject), result.ImplementationType);
            Assert.AreEqual(ServiceLifetime.Scoped, result.ServiceLifeTime);
            Assert.IsTrue(result.Required);
        }
    }
}