// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.Pipelining
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Middleware.Exceptions;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Execution.Pipelining.Data;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class MiddlewarePipelineTests
    {
        [Test]
        public async Task SingularPipelineInvokesComponentsInOrder()
        {
            var serverBuilder = new TestServerBuilder<GraphSchema>(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<MiddlewareController>();

            // setup a mock pipeline to verify that each middleware piece called into the service
            // and that hte order of the invocation was in the order the pipeline was declared
            var orderOfBeforeCalls = new List<string>();
            var orderOfAfterCalls = new List<string>();

            var middlewareService = new Mock<IMiddlewareTestService>();
            middlewareService.Setup(x => x.AfterNext(It.IsAny<string>())).Callback(
                (string name) => { orderOfAfterCalls.Add(name); }).Verifiable();
            middlewareService.Setup(x => x.BeforeNext(It.IsAny<string>())).Callback(
                (string name) => { orderOfBeforeCalls.Add(name); }).Verifiable();

            // mock the calls that would be made through the primary builder to generate a fake pipeline
            var pipelineBuilder = new SchemaPipelineBuilder<GraphSchema, IGraphFieldExecutionMiddleware, GraphFieldExecutionContext>(serverBuilder.SchemaOptions);

            pipelineBuilder.AddMiddleware<TestMiddleware1>(ServiceLifetime.Singleton);
            pipelineBuilder.AddMiddleware<TestMiddleware2>(ServiceLifetime.Singleton);
            pipelineBuilder.AddMiddleware<TestMiddleware3>(ServiceLifetime.Singleton);

            // ensure types were added via the event
            serverBuilder.AddSingleton(middlewareService.Object);
            Assert.AreEqual(4, serverBuilder.Count); // 3 middleware components + the service

            // create the pipleine
            var pipeline = pipelineBuilder.Build();
            Assert.IsNotNull(pipeline);

            // fake a graph ql request context
            var server = serverBuilder.Build();
            var fieldBuilder = server.CreateFieldContextBuilder<MiddlewareController>(
                nameof(MiddlewareController.FieldOfData),
                new object());
            var executionContext = fieldBuilder.CreateExecutionContext();

            // execute the pipeline
            await pipeline.InvokeAsync(executionContext, CancellationToken.None);

            // ensure all methods that were expected to be called, were called
            middlewareService.Verify(x => x.BeforeNext(nameof(TestMiddleware1)), Times.Exactly(1));
            middlewareService.Verify(x => x.AfterNext(nameof(TestMiddleware1)), Times.Exactly(1));

            middlewareService.Verify(x => x.BeforeNext(nameof(TestMiddleware2)), Times.Exactly(1));
            middlewareService.Verify(x => x.AfterNext(nameof(TestMiddleware2)), Times.Exactly(1));

            middlewareService.Verify(x => x.BeforeNext(nameof(TestMiddleware3)), Times.Exactly(1));
            middlewareService.Verify(x => x.AfterNext(nameof(TestMiddleware3)), Times.Exactly(1));

            // ensure the invocation order was as expected
            // in ccreated order for the "before calls"
            Assert.AreEqual(3, orderOfBeforeCalls.Count);
            Assert.AreEqual(nameof(TestMiddleware1), orderOfBeforeCalls[0]);
            Assert.AreEqual(nameof(TestMiddleware2), orderOfBeforeCalls[1]);
            Assert.AreEqual(nameof(TestMiddleware3), orderOfBeforeCalls[2]);

            // in reverse order for the "after calls"
            Assert.AreEqual(3, orderOfAfterCalls.Count);
            Assert.AreEqual(nameof(TestMiddleware3), orderOfAfterCalls[0]);
            Assert.AreEqual(nameof(TestMiddleware2), orderOfAfterCalls[1]);
            Assert.AreEqual(nameof(TestMiddleware1), orderOfAfterCalls[2]);
        }

        [Test]
        public void NoFoundMiddlewareComponent_ThrowsException()
        {
            var serverBuilder = new TestServerBuilder<GraphSchema>(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<MiddlewareController>();

            var pipelineBuilder = new SchemaPipelineBuilder<GraphSchema, IGraphFieldExecutionMiddleware, GraphFieldExecutionContext>(serverBuilder.SchemaOptions);

            // a pipeline with one component
            pipelineBuilder.AddMiddleware<TestMiddleware1>(ServiceLifetime.Singleton);
            var pipeline = pipelineBuilder.Build();
            Assert.IsNotNull(pipeline);

            // clear out the service collection mimicing a scenario where
            // by some chance the pipeline is declared with a class that doesnt end up in the
            // service provider
            serverBuilder.SchemaOptions.ServiceCollection.Clear();

            // fake a graph ql request context
            var server = serverBuilder.Build();
            var fieldBuilder = server.CreateFieldContextBuilder<MiddlewareController>(
                nameof(MiddlewareController.FieldOfData),
                new object());
            var executionContext = fieldBuilder.CreateExecutionContext();

            // execute the pipeline, exception should be thrown
            Assert.ThrowsAsync<GraphPipelineMiddlewareInvocationException>(
                async () =>
                {
                    await pipeline.InvokeAsync(executionContext, CancellationToken.None);
                });
        }

        [Test]
        public async Task MiddlewareComponentThrowsExceptions_MiddlewareInvokerShouldUnwrapAndThrowTheException()
        {
            var serverBuilder = new TestServerBuilder<GraphSchema>(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<MiddlewareController>();

            // mock the calls that would be made through the primary builder to generate a fake pipeline
            var pipelineBuilder = new SchemaPipelineBuilder<GraphSchema, IGraphFieldExecutionMiddleware, GraphFieldExecutionContext>(serverBuilder.SchemaOptions);

            // a pipeline with one component
            pipelineBuilder.AddMiddleware<TestMiddlewareThrowsException>(ServiceLifetime.Singleton);

            // make sure a delegate was created
            var pipeline = pipelineBuilder.Build();
            Assert.IsNotNull(pipeline);

            // fake a graph ql request context
            var server = serverBuilder.Build();

            var builder = server.CreateFieldContextBuilder<MiddlewareController>(
                nameof(MiddlewareController.FieldOfData),
                new object());
            var context = builder.CreateExecutionContext();

            // execute the pipeline, exception should be thrown by the middleware Component
            try
            {
                await pipeline.InvokeAsync(context, CancellationToken.None);
            }
            catch (TestMiddlewareThrowsException.TestMiddlewareException)
            {
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.Fail("Inner exception from the test was not thrown, a generic exception was thrown.");
            }
        }

        [Test]
        public async Task SingletonMiddlewareComponent_IsNeverInstantiatedMoreThanOnce()
        {
            var serverBuilder = new TestServerBuilder<GraphSchema>(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<MiddlewareController>();

            var idsCalled = new List<string>();
            var middlewareService = new Mock<IMiddlewareTestService>();
            middlewareService.Setup(x => x.BeforeNext(It.IsAny<string>())).Callback(
                (string idValue) => { idsCalled.Add(idValue); }).Verifiable();

            serverBuilder.AddSingleton(middlewareService.Object);

            // mock the calls that would be made through the primary builder to generate a fake pipeline
            var pipelineBuilder = new SchemaPipelineBuilder<GraphSchema, IGraphFieldExecutionMiddleware, GraphFieldExecutionContext>(serverBuilder.SchemaOptions);
            pipelineBuilder.AddMiddleware<TestMiddlewareSingleton>(ServiceLifetime.Singleton);

            // make sure a delegate was created
            var pipeline = pipelineBuilder.Build();
            Assert.IsNotNull(pipeline);

            Assert.AreEqual(2, serverBuilder.Count); // 1 middleware component + the service

            // fake a graph ql request context
            var server = serverBuilder.Build();

            var builder = server.CreateFieldContextBuilder<MiddlewareController>(
                nameof(MiddlewareController.FieldOfData),
                new object());

            // execute the pipeline multiple times
            await pipeline.InvokeAsync(builder.CreateExecutionContext(), CancellationToken.None);
            await pipeline.InvokeAsync(builder.CreateExecutionContext(), CancellationToken.None);
            await pipeline.InvokeAsync(builder.CreateExecutionContext(), CancellationToken.None);
            await pipeline.InvokeAsync(builder.CreateExecutionContext(), CancellationToken.None);
            await pipeline.InvokeAsync(builder.CreateExecutionContext(), CancellationToken.None);

            // ensure all methods that were expected to be called, were called
            middlewareService.Verify(x => x.BeforeNext(It.IsAny<string>()), Times.Exactly(5));

            // ensure that each time the middleware was called and an id saved off
            // that hte Id never changed (it was never "newed up" more than once)
            Assert.AreEqual(5, idsCalled.Count);
            var singleton = server.ServiceProvider.GetService<TestMiddlewareSingleton>();
            foreach (var id in idsCalled)
            {
                Assert.AreEqual(singleton.Id, id);
            }
        }

        [Test]
        public async Task SingletonMiddlewareWithUserProvidedInstance_NeverAttemptsToCreateAnInstance()
        {
            var serverBuilder = new TestServerBuilder<GraphSchema>(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<MiddlewareController>();

            var idsCalled = new List<string>();
            var middlewareService = new Mock<IMiddlewareTestService>();
            middlewareService.Setup(x => x.BeforeNext(It.IsAny<string>())).Callback(
                (string idValue) => { idsCalled.Add(idValue); }).Verifiable();

            serverBuilder.AddSingleton(middlewareService.Object);

            // mock the calls that would be made through the primary builder to generate a fake pipeline
            var pipelineBuilder = new SchemaPipelineBuilder<GraphSchema, IGraphFieldExecutionMiddleware, GraphFieldExecutionContext>(serverBuilder.SchemaOptions);

            // premake the singleton middleware component
            var component = new TestMiddlewareSingleton(middlewareService.Object);

            // inject the component into the pipeline as an item, not just a type reference
            pipelineBuilder.AddMiddleware(component);

            // make sure a delegate was created
            var pipeline = pipelineBuilder.Build();
            Assert.IsNotNull(pipeline);

            var server = serverBuilder.Build();
            var builder = server.CreateFieldContextBuilder<MiddlewareController>(
                nameof(MiddlewareController.FieldOfData),
                new object());

            // make an empty service collection (preventing creation if the middleware isnt found)
            var sc = new ServiceCollection();
            builder.ServiceProvider = sc.BuildServiceProvider();

            // execute the pipeline multiple times
            await pipeline.InvokeAsync(builder.CreateExecutionContext(), CancellationToken.None);
            await pipeline.InvokeAsync(builder.CreateExecutionContext(), CancellationToken.None);
            await pipeline.InvokeAsync(builder.CreateExecutionContext(), CancellationToken.None);
            await pipeline.InvokeAsync(builder.CreateExecutionContext(), CancellationToken.None);
            await pipeline.InvokeAsync(builder.CreateExecutionContext(), CancellationToken.None);

            // ensure all methods that were expected to be called, were called
            middlewareService.Verify(x => x.BeforeNext(It.IsAny<string>()), Times.Exactly(5));

            // ensure that each time the middleware was called and an id saved off
            // that hte Id never changed (it was never "newed up" more than once and the original object id was used each time)
            Assert.AreEqual(5, idsCalled.Count);
            foreach (var id in idsCalled)
            {
                Assert.AreEqual(component.Id, id);
            }
        }
    }
}