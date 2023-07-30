// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas
{
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Mocks;
    using NUnit.Framework;

    [TestFixture]
    public class RuntimeFieldGeneralTests
    {
        [Test]
        public void General_SubscriptionField_IsRegistered()
        {
            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapSubscription("field1", (TwoPropertyObject source, int param1) => source);
                })
                .AddSubscriptionServer()
                .Build();

            var operation = server.Schema.Operations[GraphOperationType.Subscription];
            var field = operation.Fields.FindField("field1");
            Assert.IsNotNull(field);
            Assert.AreEqual("field1", field.Name);
        }
    }
}