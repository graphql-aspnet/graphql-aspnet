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
    using System.Linq;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class SortedFieldExecutionContextListTests
    {
        private GraphFieldExecutionContext CreateFakeContext()
        {
            var parentContext = Substitute.For<IGraphQLMiddlewareExecutionContext>();
            parentContext.QueryRequest.Returns(Substitute.For<IQueryExecutionRequest>());
            parentContext.ServiceProvider.Returns(Substitute.For<IServiceProvider>());
            parentContext.Session.Returns(new QuerySession());

            var request = Substitute.For<IGraphFieldRequest>();
            var variableData = Substitute.For<IResolvedVariableCollection>();

            return new GraphFieldExecutionContext(parentContext, request, variableData);
        }

        [Test]
        public void IsolatedContext_IsAddedToIsolatedList()
        {
            var list = new SortedFieldExecutionContextList();
            list.Add(this.CreateFakeContext(), true);

            Assert.AreEqual(1, list.IsolatedContexts.Count());
            Assert.AreEqual(0, list.ParalellContexts.Count());
        }

        [Test]
        public void ParalellContext_IsAddedToParalellList()
        {
            var list = new SortedFieldExecutionContextList();
            list.Add(this.CreateFakeContext(), false);

            Assert.AreEqual(0, list.IsolatedContexts.Count());
            Assert.AreEqual(1, list.ParalellContexts.Count());
        }

        [Test]
        public void ListOrderIsPreserved_WhenMultipleContextsAreAdded()
        {
            var list = new SortedFieldExecutionContextList();

            var parentContext = Substitute.For<IGraphQLMiddlewareExecutionContext>();
            var request = Substitute.For<IGraphFieldRequest>();
            var variableData = Substitute.For<IResolvedVariableCollection>();

            var context1 = this.CreateFakeContext();
            var context2 = this.CreateFakeContext();
            var context3 = this.CreateFakeContext();
            var context4 = this.CreateFakeContext();

            list.Add(context1, true);
            list.Add(context2, false);
            list.Add(context3, true);
            list.Add(context4, false);

            Assert.AreEqual(context1, list.IsolatedContexts.ElementAt(0));
            Assert.AreEqual(context3, list.IsolatedContexts.ElementAt(1));
            Assert.AreEqual(context2, list.ParalellContexts.ElementAt(0));
            Assert.AreEqual(context4, list.ParalellContexts.ElementAt(1));
        }
    }
}