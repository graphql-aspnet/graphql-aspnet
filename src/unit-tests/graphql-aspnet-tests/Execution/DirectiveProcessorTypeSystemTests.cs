﻿// *************************************************************
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Configuration.Exceptions;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Middleware;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Generation;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Execution.TestData.GraphSchemaProcessorTestData;
    using Microsoft.Extensions.DependencyInjection;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public class DirectiveProcessorTypeSystemTests
    {
        private IServiceProvider _serviceProvider = null;
        private IServiceCollection _serviceCollection = null;
        private DirectiveProcessorTypeSystem<GraphSchema> _instance = null;
        private ISchemaPipeline<GraphSchema, GraphDirectiveExecutionContext> _directivePipeline = null;
        private List<object> _itemsExecuted;
        private GraphSchema _schemaInstance;
        private List<Type> _typesToAdd;

        public DirectiveProcessorTypeSystemTests()
        {
            _serviceCollection = new ServiceCollection();

            _itemsExecuted = new List<object>();
            _typesToAdd = new List<Type>();
        }

        private void BuildInstance(
            bool buildRequiredDirectivePipeline = true,
            GraphMiddlewareInvocationDelegate<GraphDirectiveExecutionContext> delegateToExecute = null)
        {
            // build the schema
            var options = new SchemaOptions<GraphSchema>(_serviceCollection);

            var schemaFactory = new DefaultGraphQLSchemaFactory<GraphSchema>(
                processTypeSystemDirectives: false);

            _schemaInstance = schemaFactory.CreateInstance(
                _serviceCollection.BuildServiceProvider().CreateScope(),
                options.CreateConfiguration(),
                _typesToAdd.Select(x => new SchemaTypeToRegister(x)));

            // build hte directive pipeline instance
            if (buildRequiredDirectivePipeline)
            {
                GraphMiddlewareInvocationDelegate<GraphDirectiveExecutionContext> defaultDelegate = (GraphDirectiveExecutionContext context, CancellationToken token) =>
                    {
                        _itemsExecuted.Add(context.Request.DirectiveTarget);
                        return Task.CompletedTask;
                    };

                var invocationDelegate = delegateToExecute ?? defaultDelegate;
                _directivePipeline = Substitute.For<ISchemaPipeline<GraphSchema, GraphDirectiveExecutionContext>>();
                _directivePipeline.InvokeAsync.Returns(invocationDelegate);
            }

            _serviceCollection.AddTransient<ISchemaPipeline<GraphSchema, GraphDirectiveExecutionContext>>(
                (sp) => _directivePipeline);

            // build the test object
            _serviceProvider = _serviceCollection.BuildServiceProvider();
            _instance = new DirectiveProcessorTypeSystem<GraphSchema>(_serviceProvider, new QuerySession());
        }

        [Test]
        public void SimpleItemWithDirectiveIsExecuted()
        {
            _typesToAdd.Add(typeof(ObjectTypeWithDirective));
            this.BuildInstance();

            _instance.ApplyDirectives(_schemaInstance);

            Assert.AreEqual(1, _itemsExecuted.Count);

            var graphType = _schemaInstance.KnownTypes.FindGraphType(typeof(ObjectTypeWithDirective));
            Assert.AreEqual(graphType, _itemsExecuted[0]);
        }

        [Test]
        public void DirectiveContextFieldValidation()
        {
            GraphDirectiveExecutionContext executedContext = null;
            GraphMiddlewareInvocationDelegate<GraphDirectiveExecutionContext> delegateInstance = (GraphDirectiveExecutionContext context, CancellationToken token) =>
            {
                executedContext = context;
                return Task.CompletedTask;
            };

            _typesToAdd.Add(typeof(ObjectTypeWithDirective));
            this.BuildInstance(true, delegateInstance);
            var graphType = _schemaInstance.KnownTypes.FindGraphType(typeof(ObjectTypeWithDirective));
            var directive = _schemaInstance.KnownTypes.FindDirective(typeof(SimpleTestDirectiveForProcessor));

            _instance.ApplyDirectives(_schemaInstance);

            Assert.IsNotNull(executedContext);
            Assert.AreEqual(graphType, executedContext.Request.DirectiveTarget);
            Assert.AreEqual(DirectiveInvocationPhase.SchemaGeneration, executedContext.Request.DirectivePhase);
            Assert.AreEqual(DirectiveLocation.OBJECT, executedContext.Request.InvocationContext.Location);
            Assert.AreEqual(directive, executedContext.Request.InvocationContext.Directive);
        }

        [Test]
        public void ParameterizedDirective_CapturesParameters()
        {
            GraphDirectiveExecutionContext executedContext = null;
            GraphMiddlewareInvocationDelegate<GraphDirectiveExecutionContext> delegateInstance = (GraphDirectiveExecutionContext context, CancellationToken token) =>
            {
                executedContext = context;
                return Task.CompletedTask;
            };

            _typesToAdd.Add(typeof(ObjectTypeWithDirectiveArguments));
            this.BuildInstance(true, delegateInstance);
            var graphType = _schemaInstance.KnownTypes.FindGraphType(typeof(ObjectTypeWithDirectiveArguments));
            var directive = _schemaInstance.KnownTypes.FindDirective(typeof(ParameterizedDirectiveForProcessor));

            _instance.ApplyDirectives(_schemaInstance);

            Assert.IsNotNull(executedContext);

            var argCollection = executedContext.Request.InvocationContext.Arguments;

            Assert.IsNotNull(argCollection);
            Assert.AreEqual(2, argCollection.Count);
            var param1 = argCollection["param1"];
            var param2 = argCollection["param2"];

            Assert.AreEqual(5, param1.Value.Resolve(null));
            Assert.AreEqual("string 6", param2.Value.Resolve(null));
        }

        [Test]
        public void MissingPipelineInstance_ThrowsExpectedException()
        {
            _typesToAdd.Add(typeof(ObjectTypeWithDirective));
            this.BuildInstance(false);

            Assert.Throws<SchemaConfigurationException>(() =>
            {
                _instance.ApplyDirectives(_schemaInstance);
            });
        }

        [Test]
        public void MissingDirectiveWhenAppliedByName_ThrowsExpectedException()
        {
            // has directive called "myDirective"
            // no such instance can exist in the schema
            _typesToAdd.Add(typeof(ObjectTypeWithDirectiveAppliedByName));
            this.BuildInstance();

            Assert.Throws<SchemaConfigurationException>(() =>
            {
                _instance.ApplyDirectives(_schemaInstance);
            });
        }

        [Test]
        public void CancelledContext_WithNoReason_ThrowsExceptionWithDefaultReasonAttached()
        {
            GraphMiddlewareInvocationDelegate<GraphDirectiveExecutionContext> delegateInstance = (GraphDirectiveExecutionContext context, CancellationToken token) =>
            {
                // mimic what would normally would be handled by the invoked directive
                // but with a fake pipeline the directive is never invoked
                context.Cancel();
                return Task.CompletedTask;
            };

            _typesToAdd.Add(typeof(ObjectTypeWithDirective));
            this.BuildInstance(true, delegateInstance);

            try
            {
                _instance.ApplyDirectives(_schemaInstance);
            }
            catch (SchemaConfigurationException ex)
            {
                // ensure that the default casual exception
                // is attached to the thrown execution execption
                var innerException = ex.InnerException as GraphTypeDeclarationException;
                Assert.IsNotNull(innerException);
                Assert.AreEqual(typeof(ObjectTypeWithDirective), innerException.FailedObjectType);
            }
            catch
            {
                Assert.Fail("Unexpected exception was thrown");
            }
        }

        [Test]
        public void ContextWithErrorMessages_ThrowsExceptionWithErrorsAttached()
        {
            GraphMiddlewareInvocationDelegate<GraphDirectiveExecutionContext> delegateInstance = (GraphDirectiveExecutionContext context, CancellationToken token) =>
            {
                // mimic what would normally would be handled by the invoked directive
                // but with a fake pipeline the directive is never invoked
                context.Messages.Critical("Error 1", "FAIL CODE");
                context.Messages.Critical("Error 2", "FAIL CODE");
                context.Messages.Critical("Error 3", "FAIL CODE");
                return Task.CompletedTask;
            };

            _typesToAdd.Add(typeof(ObjectTypeWithDirective));
            this.BuildInstance(true, delegateInstance);

            try
            {
                _instance.ApplyDirectives(_schemaInstance);
            }
            catch (SchemaConfigurationException ex)
            {
                Assert.IsNotNull(ex.InnerException);

                Assert.AreEqual("FAIL CODE : Error 3", ex.InnerException.Message);
                Assert.AreEqual("FAIL CODE : Error 2", ex.InnerException.InnerException.Message);
                Assert.AreEqual("FAIL CODE : Error 1", ex.InnerException.InnerException.InnerException.Message);
                Assert.IsNull(ex.InnerException.InnerException.InnerException.InnerException);
            }
            catch
            {
                Assert.Fail("Unexpected exception was thrown");
            }
        }

        [Test]
        public void ContextWithErrorExceptions_ThrowsExceptionWithErrorAttached()
        {
            var expectedThrownException = new Exception("FAIL ON 2");
            GraphMiddlewareInvocationDelegate<GraphDirectiveExecutionContext> delegateInstance = (GraphDirectiveExecutionContext context, CancellationToken token) =>
            {
                // mimic what would normally would be handled by the invoked directive
                // but with a fake pipeline the directive is never invoked
                context.Messages.Critical("Error 1", "FAIL CODE");
                context.Messages.Critical("Error 2", "FAIL CODE", exceptionThrown: expectedThrownException);
                context.Messages.Critical("Error 3", "FAIL CODE");
                return Task.CompletedTask;
            };

            _typesToAdd.Add(typeof(ObjectTypeWithDirective));
            this.BuildInstance(true, delegateInstance);

            try
            {
                _instance.ApplyDirectives(_schemaInstance);
            }
            catch (SchemaConfigurationException ex)
            {
                Assert.AreEqual(expectedThrownException, ex.InnerException);
                Assert.IsNull(ex.InnerException.InnerException);
            }
            catch
            {
                Assert.Fail("Unexpected exception was thrown");
            }
        }

        [Test]
        public void UncaughtExceptionDuringPipelineExecution_IsCaughtAndPackagedByProcessor()
        {
            var thrownException = new Exception("UNCAUGHT EXCEPTION");
            GraphMiddlewareInvocationDelegate<GraphDirectiveExecutionContext> delegateInstance = (GraphDirectiveExecutionContext context, CancellationToken token) =>
            {
                throw thrownException;
            };

            _typesToAdd.Add(typeof(ObjectTypeWithDirective));
            this.BuildInstance(true, delegateInstance);

            try
            {
                _instance.ApplyDirectives(_schemaInstance);
            }
            catch (SchemaConfigurationException ex)
            {
                Assert.AreEqual(thrownException, ex.InnerException);
                Assert.IsNull(ex.InnerException.InnerException);
            }
            catch
            {
                Assert.Fail("Unexpected exception was thrown");
            }
        }
    }
}