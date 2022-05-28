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
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Execution;
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
                        .ToItems(x =>
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

        [Test]
        public void ApplyDirective_ToQueryObject_IsApplied()
        {
            var directiveInstance = new CountableLateBoundDirective();

            var serverBuilder = new TestServerBuilder()
                .AddGraphQL(options =>
                {
                    // late bind the directive to the single OBJECT
                    options.ApplyDirective<CountableLateBoundDirective>()
                        .Clear()
                        .ToItems(x => x is IGraphOperation go);
                });

            serverBuilder.AddSingleton(directiveInstance);

            serverBuilder.Build();

            Assert.AreEqual(1, directiveInstance.Invocations.Count);

            var item = directiveInstance.Invocations.First();
            var operationType = ((IGraphOperation)item.Key).OperationType;
            var count = item.Value;

            Assert.AreEqual(GraphCollection.Query, operationType);
            Assert.AreEqual(1, count);
        }

        [Test]
        public void ApplyDirective_ToNonDefaultOperations_IsApplied()
        {
            var directiveInstance = new CountableLateBoundDirective();

            var serverBuilder = new TestServerBuilder()
                .AddGraphQL(options =>
                {
                    // late bind the directive to the single OBJECT
                    options.AddController<MutationAndQueryController>();
                    options.ApplyDirective<CountableLateBoundDirective>()
                        .Clear()
                        .ToItems(x => x is IGraphOperation go);
                });

            serverBuilder.AddSingleton(directiveInstance);

            serverBuilder.Build();

            var foundOperations = new HashSet<GraphCollection>();
            foreach (var item in directiveInstance.Invocations)
            {
                foundOperations.Add(((IGraphOperation)item.Key).OperationType);
            }

            Assert.AreEqual(2, foundOperations.Count);
            Assert.IsTrue(foundOperations.Contains(GraphCollection.Query));
            Assert.IsTrue(foundOperations.Contains(GraphCollection.Mutation));
        }
    }
}