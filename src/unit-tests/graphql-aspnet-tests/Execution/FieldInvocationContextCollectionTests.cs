// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution
{
    using System;
    using GraphQL.AspNet.Execution;
    using NUnit.Framework;

    [TestFixture]
    public class FieldInvocationContextCollectionTests
    {
        [Test]
        public void Add_NotItemSupplied_ReturnsNoFailure()
        {
            var collection = new FieldInvocationContextCollection();

            collection.Add(null);
            Assert.AreEqual(0, collection.Count);
        }

        [TestCase(null)]
        [TestCase(typeof(FieldInvocationContextCollectionTests))]
        public void CanAcceptType_ShouldBeFale(Type type)
        {
            var collection = new FieldInvocationContextCollection();

            var result = collection.CanAcceptSourceType(type);
            Assert.IsFalse(result);
        }
    }
}