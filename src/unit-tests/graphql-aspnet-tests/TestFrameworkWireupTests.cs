// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests
{
    using System;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class TestFrameworkWireupTests
    {
        [Test]
        public void EnsureTestFrameworkAssertionsThrowsExceptionWhenNull()
        {
            var value = GraphQLTestFrameworkProviders.Assertions;
            GraphQLTestFrameworkProviders.Assertions = null;
            Assert.Throws<InvalidOperationException>(() =>
            {
                CommonAssertions.AreEqualJsonStrings("{ \"prop\": 34}", "{ \"prop\": 35}");
            });

            GraphQLTestFrameworkProviders.Assertions = value;
        }
    }
}