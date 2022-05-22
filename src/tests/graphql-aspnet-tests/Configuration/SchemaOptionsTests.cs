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
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Tests.Configuration.SchemaOptionsTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class SchemaOptionsTests
    {
        [Test]
        public void ApplyDirective_SimpleTest()
        {
            var server = new TestServerBuilder()
                .AddGraphType<ObjectForLateBoundDirective>()
                .AddGraphQL(options =>
                {
                    // late bind the directive to the type
                    options.ApplyDirective<SimpleLateBoundObjectDirective>()
                        .Where(x => x is IObjectGraphType ogt && ogt.ObjectType == typeof(ObjectForLateBoundDirective));
                });

            server.Build();
            Assert.AreEqual(1, SimpleLateBoundObjectDirective.TOTAL_INVOCATIONS);
        }
    }
}