// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Common
{
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Tests.Common.ExpressionExtensionTestData;
    using NUnit.Framework;

    [TestFixture]
    public class ExpressionExtensionsTests
    {
        [Test]
        public static void DeclaredItems_ExpectedPropertyIsRetrieved()
        {
            var expected = typeof(ExpressionTestObject).GetProperty(nameof(ExpressionTestObject.SomeProperty));
            var result = ExpressionExtensions.RetrievePropertyInfo<ExpressionTestObject>(x => x.SomeProperty);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void DeclaredItems_ExpectedMethodIsRetrieved()
        {
            var expected = typeof(ExpressionTestObject).GetMethod(nameof(ExpressionTestObject.SomeMethod));
            var result = ExpressionExtensions.RetrieveMethodInfo<ExpressionTestObject>(x => x.SomeMethod);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void InheritedItems_ExpectedPropertyIsRetrieved()
        {
            var expected = typeof(SubClassedExpressionTestObject).GetProperty(nameof(SubClassedExpressionTestObject.SomeProperty));
            var result = ExpressionExtensions.RetrievePropertyInfo<SubClassedExpressionTestObject>(x => x.SomeProperty);

            Assert.AreEqual(expected.Name, result.Name);
            Assert.AreEqual(expected.DeclaringType, result.DeclaringType);
        }

        [Test]
        public static void InheritedItems_ExpectedMethodIsRetrieved()
        {
            var expected = typeof(SubClassedExpressionTestObject).GetMethod(nameof(SubClassedExpressionTestObject.SomeMethod));
            var result = ExpressionExtensions.RetrieveMethodInfo<SubClassedExpressionTestObject>(x => x.SomeMethod);

            Assert.AreEqual(expected.Name, result.Name);
            Assert.AreEqual(expected.DeclaringType, result.DeclaringType);
        }

        [Test]
        public static void ParameterizedMethod_ExpectedMethodIsRetrieved()
        {
            var expected = typeof(ParameterizedMethodExpressionTestObject).GetMethod(nameof(ParameterizedMethodExpressionTestObject.SomeMethod));
            var result = ExpressionExtensions.RetrieveMethodInfo<ParameterizedMethodExpressionTestObject>(x => x.SomeMethod);

            Assert.AreEqual(expected, result);
        }
    }
}