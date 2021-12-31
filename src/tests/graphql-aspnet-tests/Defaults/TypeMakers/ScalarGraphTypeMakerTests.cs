// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Defaults.TypeMakers
{
    using GraphQL.AspNet.Defaults.TypeMakers;
    using NUnit.Framework;

    [TestFixture]
    public class ScalarGraphTypeMakerTests
    {
        [Test]
        public void RegisteredScalarIsReturned()
        {
            var maker = new ScalarGraphTypeMaker();

            var result = maker.CreateGraphType(typeof(int));
            Assert.IsNotNull(result?.GraphType);
            Assert.AreEqual(typeof(int), result.ConcreteType);
        }

        [Test]
        public void NullType_ReturnsNullResult()
        {
            var maker = new ScalarGraphTypeMaker();

            var result = maker.CreateGraphType(null);
            Assert.IsNull(result);
        }
    }
}