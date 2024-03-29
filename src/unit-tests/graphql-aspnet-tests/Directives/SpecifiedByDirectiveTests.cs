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
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Directives.Global;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using Microsoft.Extensions.DependencyInjection;
    using NSubstitute;
    using NUnit.Framework;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.QueryInputValueSteps;

    [TestFixture]
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public class SpecifiedByDirectiveTests
    {
        private ISchemaPipeline<GraphSchema, GraphDirectiveExecutionContext> _pipeline;

        private IDirective _directive;
        private IInputArgumentCollection _argCollection;
        private IInputValue _argValue;
        private string _url = null;
        private QueryExecutionRequest _queryRequest;
        private DirectiveInvocationContext _invocationContext;
        private DirectiveLocation _directiveLocation;
        private IServiceProvider _provider;
        private IServiceScope _scopedProvider;
        private object _directiveTarget;
        private ISchema _schema;
        private InputArgument _arg;
        private IGraphEventLogger _logger;

        public SpecifiedByDirectiveTests()
        {
            var server = new TestServerBuilder()
                .AddDirective<SpecifiedByDirective>()
                .AddType<TwoPropertyObject>()
                .Build();

            _directiveLocation = DirectiveLocation.SCALAR;
            _provider = server.ServiceProvider;
            _scopedProvider = _provider.CreateScope();
            _schema = server.Schema;
            _directive = server.Schema.KnownTypes.FindDirective<SpecifiedByDirective>();
            _pipeline = server.ServiceProvider
                .GetService<ISchemaPipeline<GraphSchema, GraphDirectiveExecutionContext>>();

            var arg1 = _directive.Arguments[0];

            _argValue = Substitute.For<IInputValue>();
            _argValue.Resolve(Arg.Any<IResolvedVariableCollection>())
                .Returns((x) => _url);

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
            _url = "http://somesite";
            _directiveTarget = null;

            var context = await this.ExecuteRequest();

            Assert.AreEqual(GraphMessageSeverity.Critical, context.Messages.Severity);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.IsNotNull(context.Messages[0].Exception);
            Assert.AreEqual(typeof(GraphTypeDeclarationException), context.Messages[0].Exception.GetType());
        }

        [Test]
        public async Task WhenInvalidLocation_ThrowsException()
        {
            _directiveLocation = DirectiveLocation.FIELD;
            _url = "http://somesite";
            _directiveTarget = _schema.AllSchemaItems().Single(x => x.Name == Constants.ScalarNames.STRING);

            var context = await this.ExecuteRequest();

            Assert.AreEqual(GraphMessageSeverity.Critical, context.Messages.Severity);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.EXECUTION_ERROR, context.Messages[0].Code);
        }

        [Test]
        public async Task NoUrlGiven_ThrowsException()
        {
            _url = null;
            _directiveLocation = DirectiveLocation.SCALAR;
            _directiveTarget = _schema.AllSchemaItems().Single(x => x.Name == Constants.ScalarNames.STRING);

            var context = await this.ExecuteRequest();

            Assert.AreEqual(GraphMessageSeverity.Critical, context.Messages.Severity);
            Assert.AreEqual(1, context.Messages.Count);

            // rule 5.7 deals with input parameter validation
            Assert.IsNotNull("5.7", context.Messages[0].MetaData["Rule"].ToString());
        }

        [Test]
        public async Task WhenSchemaItemIsNotASclar_SchemaFails()
        {
            _url = "http://somesite";
            _directiveTarget = _schema.AllSchemaItems().First(x => x.IsObjectGraphType<TwoPropertyObject>());

            var context = await this.ExecuteRequest();

            Assert.IsFalse(context.Messages.IsSucessful);
        }

        [Test]
        public async Task WhenSchemaItemIsAScalar_UrlIsApplied()
        {
            _url = "http://someUrl";
            _directiveTarget = _schema.AllSchemaItems().Single(x => x.Name == Constants.ScalarNames.STRING);

            var context = await this.ExecuteRequest();

            Assert.IsTrue(context.Messages.IsSucessful);

            var item = _directiveTarget as IScalarGraphType;
            Assert.AreEqual("http://someUrl", item.SpecifiedByUrl);
        }
    }
}