﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Directives
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Directives.Global;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using Microsoft.Extensions.DependencyInjection;
    using NSubstitute;
    using NUnit.Framework;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.Execution.QueryPlans.InputArguments;

    [TestFixture]
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public class DeprecatedDirectiveTests
    {
        public enum TestEnum
        {
            Value1,
            Value2,
        }

        private ISchemaPipeline<GraphSchema, GraphDirectiveExecutionContext> _pipeline;

        private IDirective _directive;
        private IInputArgumentCollection _argCollection;
        private IInputValue _argValue;
        private string _reason = null;
        private QueryExecutionRequest _queryRequest;
        private DirectiveInvocationContext _invocationContext;
        private DirectiveLocation _directiveLocation;
        private IServiceProvider _provider;
        private IServiceScope _scopedProvider;
        private object _directiveTarget;
        private ISchema _schema;
        private InputArgument _arg;
        private IGraphEventLogger _logger;

        public DeprecatedDirectiveTests()
        {
            var server = new TestServerBuilder()
                .AddDirective<DeprecatedDirective>()
                .AddType<TwoPropertyObject>()
                .AddType<TestEnum>()
                .Build();

            _provider = server.ServiceProvider;
            _scopedProvider = _provider.CreateScope();
            _schema = server.Schema;
            _directive = server.Schema.KnownTypes.FindDirective<DeprecatedDirective>();
            _pipeline = server.ServiceProvider
                .GetService<ISchemaPipeline<GraphSchema, GraphDirectiveExecutionContext>>();

            var arg1 = _directive.Arguments[0];

            _argValue = Substitute.For<IInputValue>();
            _argValue.Resolve(Arg.Any<IResolvedVariableCollection>())
                .Returns((x) => _reason);

            _arg = new InputArgument(
                _directive.Arguments[0],
                _argValue,
                SourceOrigin.None);

            var argList = new List<InputArgument>();
            argList.Add(_arg);

            _argCollection = Substitute.For<IInputArgumentCollection>();
            _argCollection.GetEnumerator()
                .Returns((x) => argList.GetEnumerator());

            _logger = Substitute.For<IGraphEventLogger>();
        }

        private async Task<GraphDirectiveExecutionContext> ExecuteRequest()
        {
            var executionArgs = new ExecutionArgumentCollection();
            if (_reason == "do-not-add")
                executionArgs.Add(new ExecutionArgument(_directive.Arguments[0], _reason));

            _queryRequest = new QueryExecutionRequest(GraphQueryData.Empty);

            _invocationContext = new DirectiveInvocationContext(
                _directive,
                _directiveLocation,
                SourceOrigin.None,
                _argCollection);

            var directiveRequest = new GraphDirectiveRequest(
                _invocationContext,
                DirectiveInvocationPhase.SchemaGeneration,
                _directiveTarget);

            var context = new GraphDirectiveExecutionContext(
                _schema,
                directiveRequest,
                _queryRequest,
                _scopedProvider.ServiceProvider,
                new QuerySession(),
                logger: _logger);

            await _pipeline.InvokeAsync(context, default);
            return context;
        }

        [Test]
        public async Task NoDirectiveTarget_ThrowsException()
        {
            _directiveLocation = DirectiveLocation.FIELD_DEFINITION;
            _reason = "A valid Reason";
            _directiveTarget = null;

            var context = await this.ExecuteRequest();

            Assert.AreEqual(GraphMessageSeverity.Critical, context.Messages.Severity);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.IsNotNull(context.Messages[0].Exception);
            Assert.AreEqual(typeof(GraphTypeDeclarationException), context.Messages[0].Exception.GetType());
        }

        [Test]
        public async Task NullReasonGiven_ReasonIsSetToNull()
        {
            _directiveLocation = DirectiveLocation.FIELD_DEFINITION;
            _reason = null;
            _directiveTarget = _schema.AllSchemaItems().First(x => x.IsField<TwoPropertyObject>("property1"));

            var context = await this.ExecuteRequest();

            var item = _directiveTarget as IGraphField;
            Assert.IsTrue(item.IsDeprecated);
            Assert.IsNull(item.DeprecationReason);
        }

        [Test]
        public async Task WhenSchemaItemIsNotAFieldOrEnum_SchemaFails()
        {
            _directiveLocation = DirectiveLocation.FIELD_DEFINITION;
            _reason = "because reason";
            _directiveTarget = _schema.AllSchemaItems().First(x => x.IsObjectGraphType<TwoPropertyObject>());

            var context = await this.ExecuteRequest();

            Assert.IsFalse(context.Messages.IsSucessful);
        }

        [Test]
        public async Task WhenSchemaItemIsEnumValue_ItsDeprecated()
        {
            _directiveLocation = DirectiveLocation.FIELD_DEFINITION;
            _reason = "because reason";
            _directiveTarget = _schema.AllSchemaItems().First(x => x.IsEnumValue<TestEnum>(TestEnum.Value2));

            var context = await this.ExecuteRequest();

            Assert.IsTrue(context.Messages.IsSucessful);

            var item = _directiveTarget as IEnumValue;
            Assert.IsTrue(item.IsDeprecated);
            Assert.AreEqual(_reason, item.DeprecationReason);
        }

        [Test]
        public async Task WhenSchemaItemIsField_ItsDeprecated()
        {
            _directiveLocation = DirectiveLocation.FIELD_DEFINITION;
            _reason = "because reason";
            _directiveTarget = _schema.AllSchemaItems().First(x => x.IsField<TwoPropertyObject>("property1"));

            var context = await this.ExecuteRequest();

            Assert.IsTrue(context.Messages.IsSucessful);

            var item = _directiveTarget as IGraphField;
            Assert.IsTrue(item.IsDeprecated);
            Assert.AreEqual(_reason, item.DeprecationReason);
        }
    }
}