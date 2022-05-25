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
    using System.Linq;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Tests.Configuration.SchemaOptionsTestData;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class SchemaOptionsApplyDirectiveTests
    {
        [Test]
        public void ApplyDirective_AppliesToItemsInSelectionCriteria()
        {
            var directiveInstance = new CountableLateBoundDirective();

            var serverBuilder = new TestServerBuilder()
                .AddGraphType<ObjectForLateBoundDirective>()
                .AddGraphQL(options =>
                {
                    // late bind the directive to the single OBJECT
                    options.ApplyDirective<CountableLateBoundDirective>()
                        .Where(x =>
                            x is IObjectGraphType ogt
                            && ogt.ObjectType == typeof(ObjectForLateBoundDirective));
                });

            serverBuilder.AddSingleton(directiveInstance);

            serverBuilder.Build();

            Assert.AreEqual(1, directiveInstance.Invocations.Count);

            var item = directiveInstance.Invocations.First();
            var objectType = ((IObjectGraphType)item.Key).ObjectType;
            var count = item.Value;

            Assert.AreEqual(typeof(ObjectForLateBoundDirective), objectType);
            Assert.AreEqual(1, count);
        }
    }
}