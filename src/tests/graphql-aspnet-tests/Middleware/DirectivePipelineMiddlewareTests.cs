// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Middleware
{
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.DirectiveExecution.Components;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.AspNet.Tests.Middleware.DirectiveMiddlewareTestData;
    using NUnit.Framework;

    [TestFixture]
    public class DirectivePipelineMiddlewareTests
    {
        public Task EmptyNextDelegate(GraphDirectiveExecutionContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        [Test]
        public async Task ValidationMiddlewareForwardsRequestToRuleSet()
        {
            var server = new TestServerBuilder()
                .AddGraphType<TwoPropertyObject>()
              .AddGraphType<PipelineTestDirective>()
              .Build();

            var context = server.CreateDirectiveExecutionContext<PipelineTestDirective>(
                DirectiveLocation.OBJECT,
                new TwoPropertyObject(),
                DirectiveInvocationPhase.BeforeFieldResolution,
                SourceOrigin.None,
                new object[] { 5 }); // directive requires 2 argument, only 1 supplied

            Assert.IsTrue(context.IsValid);

            var component = new ValidateDirectiveExecutionMiddleware<GraphSchema>();
            await component.InvokeAsync(context, this.EmptyNextDelegate);

            Assert.IsFalse(context.IsValid);
        }

        [Test]
        public async Task InvocationMiddlewareCallsResolver()
        {
            var server = new TestServerBuilder()
                .AddGraphType<TwoPropertyObject>()
              .AddGraphType<PipelineTestDirective>()
              .Build();

            var testObject = new TwoPropertyObject();

            var context = server.CreateDirectiveExecutionContext<PipelineTestDirective>(
                DirectiveLocation.OBJECT,
                testObject,
                DirectiveInvocationPhase.BeforeFieldResolution,
                SourceOrigin.None,
                new object[] { "testValue", 5 });

            Assert.IsTrue(context.IsValid);

            // invocation of hte directive will set the passed in values
            // to the object
            var component = new InvokeDirectiveResolverMiddleware<GraphSchema>(server.Schema);
            await component.InvokeAsync(context, this.EmptyNextDelegate);

            Assert.AreEqual("testValue", testObject.Property1);
            Assert.AreEqual(5, testObject.Property2);
        }
    }
}