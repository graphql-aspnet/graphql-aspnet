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
    using System;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class SchemaTypeToRegisterTests
    {
        [TestCase(null, null, true)]
        [TestCase(typeof(TwoPropertyObject), typeof(TwoPropertyObject), true)]
        [TestCase(typeof(TwoPropertyObject), null, false)]
        [TestCase(null, typeof(TwoPropertyObject), false)]
        [TestCase(typeof(TwoPropertyObject), typeof(TwoPropertyObjectV2), false)]
        [TestCase(typeof(TwoPropertyObjectV2), typeof(TwoPropertyObject), false)]
        [TestCase(typeof(TwoPropertyObjectV2), typeof(int), false)]
        public void DefaultComparer_IdentifiesEqualInstances(Type type1, Type type2, bool areEqual)
        {
            var instance1 = type1 != null ? new SchemaTypeToRegister(type1) : null;
            var instance2 = type2 != null ? new SchemaTypeToRegister(type2) : null;

            var comparer = SchemaTypeToRegister.DefaultComparer;
            var result = comparer.Equals(instance1, instance2);

            Assert.AreEqual(areEqual, result);
        }
    }
}