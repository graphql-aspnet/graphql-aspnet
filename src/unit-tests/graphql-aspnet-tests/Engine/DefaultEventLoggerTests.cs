// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Security.Claims;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Logging.GeneralEvents;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Security;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class DefaultEventLoggerTests
    {
        public void ExecuteTest(
            Action<DefaultGraphEventLogger> execute,
            bool shouldLogLevelBeEnabled,
            Type logEntryType = null,
            LogLevel? expectedLogLevel = null,
            EventId? expectedEventId = null)
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var logger = Substitute.For<ILogger>();

            logger.IsEnabled(Arg.Any<LogLevel>()).Returns(shouldLogLevelBeEnabled);

            loggerFactory.CreateLogger(Arg.Any<string>()).Returns(logger);

            IGraphLogEntry recordedEntry = null;

            if (shouldLogLevelBeEnabled)
            {
                logger.When(x => x.Log(
                     expectedLogLevel.Value,
                     expectedEventId.Value,
                     Arg.Any<IGraphLogEntry>(),
                     Arg.Any<Exception>(),
                     Arg.Any<Func<IGraphLogEntry, Exception, string>>()))
                    .Do(x =>
                    {
                        recordedEntry = (IGraphLogEntry)x[2];
                    });
            }

            var eventLogger = new DefaultGraphEventLogger(loggerFactory);
            execute(eventLogger);

            if (shouldLogLevelBeEnabled)
            {
                logger.Received(1).Log(
                        expectedLogLevel.Value,
                        expectedEventId.Value,
                        Arg.Any<IGraphLogEntry>(),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<IGraphLogEntry, Exception, string>>());

                Assert.AreEqual(logEntryType, recordedEntry.GetType());
            }
            else
            {
                logger.Received(0).Log(
                        Arg.Any<LogLevel>(),
                        Arg.Any<EventId>(),
                        Arg.Any<IGraphLogEntry>(),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<IGraphLogEntry, Exception, string>>());

                Assert.IsNull(recordedEntry);
            }
        }

        private static List<object[]> _eventLoggerTestData;

        static DefaultEventLoggerTests()
        {
            _eventLoggerTestData = new List<object[]>();
            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.SchemaInstanceCreated<GraphSchema>(
                        new GraphSchema()),
                    true,
                    typeof(SchemaInstanceCreatedLogEntry<GraphSchema>),
                    LogLevel.Debug,
                    LogEventIds.SchemaInstanceCreated,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.SchemaInstanceCreated<GraphSchema>(
                        new GraphSchema()),
                    false,
                    null,
                    null,
                    null,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.SchemaPipelineRegistered<GraphSchema>(
                        Substitute.For<ISchemaPipeline>()),
                    true,
                    typeof(SchemaPipelineRegisteredLogEntry<GraphSchema>),
                    LogLevel.Debug,
                    LogEventIds.SchemaPipelineInstanceCreated,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.SchemaPipelineRegistered<GraphSchema>(
                        Substitute.For<ISchemaPipeline>()),
                    false,
                    null,
                    null,
                    null,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.SchemaUrlRouteRegistered<GraphSchema>(null),
                    true,
                    typeof(SchemaRouteRegisteredLogEntry<GraphSchema>),
                    LogLevel.Debug,
                    LogEventIds.SchemaUrlRouteRegistered,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.SchemaUrlRouteRegistered<GraphSchema>(null),
                    false,
                    null,
                    null,
                    null,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.RequestReceived(null),
                    true,
                    typeof(RequestReceivedLogEntry),
                    LogLevel.Trace,
                    LogEventIds.RequestReceived,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.RequestReceived(null),
                    false,
                    null,
                    null,
                    null,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.QueryPlanCacheFetchHit<GraphSchema>(null),
                    true,
                    typeof(QueryExecutionPlanCacheHitLogEntry<GraphSchema>),
                    LogLevel.Trace,
                    LogEventIds.QueryCacheHit,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.QueryPlanCacheFetchHit<GraphSchema>(null),
                    false,
                    null,
                    null,
                    null,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.QueryPlanCacheFetchMiss<GraphSchema>(null),
                    true,
                    typeof(QueryExecutionPlanCacheMissLogEntry<GraphSchema>),
                    LogLevel.Trace,
                    LogEventIds.QueryCacheMiss,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.QueryPlanCacheFetchMiss<GraphSchema>(null),
                    false,
                    null,
                    null,
                    null,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.QueryPlanCached(null, null),
                    true,
                    typeof(QueryExecutionPlanCacheAddLogEntry),
                    LogLevel.Debug,
                    LogEventIds.QueryCacheAdd,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.QueryPlanCached(null, null),
                    false,
                    null,
                    null,
                    null,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.QueryPlanGenerated(null),
                    true,
                    typeof(QueryExecutionPlanGeneratedLogEntry),
                    LogLevel.Trace,
                    LogEventIds.QueryPlanGenerationCompleted,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.QueryPlanGenerated(null),
                    false,
                    null,
                    null,
                    null,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.FieldResolutionStarted(null),
                    true,
                    typeof(FieldResolutionStartedLogEntry),
                    LogLevel.Trace,
                    LogEventIds.FieldResolutionStarted,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.FieldResolutionStarted(null),
                    false,
                    null,
                    null,
                    null,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.SchemaItemAuthorizationChallenge(null),
                    true,
                    typeof(SchemaItemAuthorizationStartedLogEntry),
                    LogLevel.Trace,
                    LogEventIds.SchemaItemAuthorizationStarted,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.SchemaItemAuthorizationChallenge(null),
                    false,
                    null,
                    null,
                    null,
                });

            _eventLoggerTestData.Add(
               new object[]
               {
                    (DefaultGraphEventLogger x) => x.SchemaItemAuthorizationChallengeResult(null),
                    false,
                    null,
                    null,
                    null,
               });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.SchemaItemAuthenticationChallenge(null),
                    true,
                    typeof(SchemaItemAuthenticationStartedLogEntry),
                    LogLevel.Trace,
                    LogEventIds.SchemaItemAuthenticationStarted,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.SchemaItemAuthenticationChallenge(null),
                    false,
                    null,
                    null,
                    null,
                });

            _eventLoggerTestData.Add(
              new object[]
              {
                    (DefaultGraphEventLogger x) => x.SchemaItemAuthenticationChallengeResult(null, null),
                    false,
                    null,
                    null,
                    null,
              });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.FieldResolutionCompleted(null),
                    true,
                    typeof(FieldResolutionCompletedLogEntry),
                    LogLevel.Trace,
                    LogEventIds.FieldResolutionCompleted,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.FieldResolutionCompleted(null),
                    false,
                    null,
                    null,
                    null,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.ActionMethodInvocationRequestStarted(null, null),
                    true,
                    typeof(ActionMethodInvocationStartedLogEntry),
                    LogLevel.Trace,
                    LogEventIds.ControllerInvocationStarted,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.ActionMethodInvocationRequestStarted(null, null),
                    false,
                    null,
                    null,
                    null,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.ActionMethodModelStateValidated(null, null, null),
                    true,
                    typeof(ActionMethodModelStateValidatedLogEntry),
                    LogLevel.Trace,
                    LogEventIds.ControllerModelValidated,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.ActionMethodModelStateValidated(null, null, null),
                    false,
                    null,
                    null,
                    null,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.ActionMethodInvocationException(null, null, null),
                    true,
                    typeof(ActionMethodInvocationExceptionLogEntry),
                    LogLevel.Error,
                    LogEventIds.ControllerInvocationException,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.ActionMethodInvocationException(null, null, null),
                    false,
                    null,
                    null,
                    null,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.ActionMethodUnhandledException(null, null, null),
                    true,
                    typeof(ActionMethodUnhandledExceptionLogEntry),
                    LogLevel.Error,
                    LogEventIds.ControllerUnhandledException,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.ActionMethodUnhandledException(null, null, null),
                    false,
                    null,
                    null,
                    null,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.ActionMethodInvocationCompleted(null, null, null),
                    true,
                    typeof(ActionMethodInvocationCompletedLogEntry),
                    LogLevel.Trace,
                    LogEventIds.ControllerInvocationCompleted,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.ActionMethodInvocationCompleted(null, null, null),
                    false,
                    null,
                    null,
                    null,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.RequestCompleted(null),
                    true,
                    typeof(RequestCompletedLogEntry),
                    LogLevel.Trace,
                    LogEventIds.RequestCompleted,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.RequestCompleted(null),
                    false,
                    null,
                    null,
                    null,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.RequestTimedOut(null),
                    true,
                    typeof(RequestTimedOutLogEntry),
                    LogLevel.Warning,
                    LogEventIds.RequestTimeout,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.RequestTimedOut(null),
                    false,
                    null,
                    null,
                    null,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.RequestCancelled(null),
                    true,
                    typeof(RequestCancelledLogEntry),
                    LogLevel.Information,
                    LogEventIds.RequestCancelled,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.RequestCancelled(null),
                    false,
                    null,
                    null,
                    null,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.TypeSystemDirectiveApplied<GraphSchema>(null, null),
                    true,
                    typeof(TypeSystemDirectiveAppliedLogEntry<GraphSchema>),
                    LogLevel.Debug,
                    LogEventIds.TypeSystemDirectiveApplied,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.TypeSystemDirectiveApplied<GraphSchema>(null, null),
                    false,
                    null,
                    null,
                    null,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.ExecutionDirectiveApplied<GraphSchema>(
                        Substitute.For<IDirective>(),
                        Substitute.For<IDocumentPart>()),
                    true,
                    typeof(ExecutionDirectiveAppliedLogEntry<GraphSchema>),
                    LogLevel.Trace,
                    LogEventIds.ExecutionDirectiveApplied,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.ExecutionDirectiveApplied<GraphSchema>(
                        Substitute.For<IDirective>(),
                        Substitute.For<IDocumentPart>()),
                    false,
                    null,
                    null,
                    null,
                });
        }

        [TestCaseSource(nameof(_eventLoggerTestData))]
        public void GeneralTests(
            Action<DefaultGraphEventLogger> testAction,
            bool shouldBeLogged,
            Type recordedLogType = null,
            LogLevel? logLevel = null,
            EventId? eventId = null)
        {
            this.ExecuteTest(testAction, shouldBeLogged, recordedLogType, logLevel, eventId);
        }

        [Test]
        public void AuthorizationChallengeResult_Unauthorized()
        {
            var context = Substitute.For<IGraphQLMiddlewareExecutionContext>();
            var secRequest = Substitute.For<ISchemaItemSecurityRequest>();
            context.ServiceProvider.Returns(Substitute.For<IServiceProvider>());
            context.QueryRequest.Returns(Substitute.For<IQueryExecutionRequest>());
            context.Session.Returns(Substitute.For<IQuerySession>());

            var failedAuthResult = new SchemaItemSecurityChallengeContext(
                            context,
                            secRequest);
            failedAuthResult.Result = SchemaItemSecurityChallengeResult.Unauthorized("fail");

            this.ExecuteTest(
                    (DefaultGraphEventLogger x) => x.SchemaItemAuthorizationChallengeResult(failedAuthResult),
                    true,
                    typeof(SchemaItemAuthorizationCompletedLogEntry),
                    LogLevel.Warning,
                    LogEventIds.SchemaItemAuthorizationCompleted);
        }

        [Test]
        public void AuthorizationChallengeResult_Success()
        {
            var context = Substitute.For<IGraphQLMiddlewareExecutionContext>();
            var secRequest = Substitute.For<ISchemaItemSecurityRequest>();
            context.ServiceProvider.Returns(Substitute.For<IServiceProvider>());
            context.QueryRequest.Returns(Substitute.For<IQueryExecutionRequest>());
            context.Session.Returns(Substitute.For<IQuerySession>());

            var nonFailResult = new SchemaItemSecurityChallengeContext(
                          context,
                          secRequest);
            nonFailResult.Result = SchemaItemSecurityChallengeResult.Success(new ClaimsPrincipal());

            this.ExecuteTest(
                    (DefaultGraphEventLogger x) => x.SchemaItemAuthorizationChallengeResult(nonFailResult),
                    true,
                    typeof(SchemaItemAuthorizationCompletedLogEntry),
                    LogLevel.Trace,
                    LogEventIds.SchemaItemAuthorizationCompleted);
        }

        [Test]
        public void AuthorizationChallengeResult_Skip()
        {
            var context = Substitute.For<IGraphQLMiddlewareExecutionContext>();
            var secRequest = Substitute.For<ISchemaItemSecurityRequest>();
            context.ServiceProvider.Returns(Substitute.For<IServiceProvider>());
            context.QueryRequest.Returns(Substitute.For<IQueryExecutionRequest>());
            context.Session.Returns(Substitute.For<IQuerySession>());

            var nonResult = new SchemaItemSecurityChallengeContext(
                          context,
                          secRequest);
            nonResult.Result = SchemaItemSecurityChallengeResult.Skipped(new ClaimsPrincipal());

            this.ExecuteTest(
                    (DefaultGraphEventLogger x) => x.SchemaItemAuthorizationChallengeResult(nonResult),
                    true,
                    typeof(SchemaItemAuthorizationCompletedLogEntry),
                    LogLevel.Trace,
                    LogEventIds.SchemaItemAuthorizationCompleted);
        }

        [Test]
        public void AuthenticationChallengeResult_FailedResult_LoggedAtWarning()
        {
            var context = Substitute.For<IGraphQLMiddlewareExecutionContext>();
            var secRequest = Substitute.For<ISchemaItemSecurityRequest>();
            context.ServiceProvider.Returns(Substitute.For<IServiceProvider>());
            context.QueryRequest.Returns(Substitute.For<IQueryExecutionRequest>());
            context.Session.Returns(Substitute.For<IQuerySession>());

            var nonResult = new SchemaItemSecurityChallengeContext(
                          context,
                          secRequest);
            nonResult.Result = SchemaItemSecurityChallengeResult.Fail("null");

            this.ExecuteTest(
                    (DefaultGraphEventLogger x) => x.SchemaItemAuthenticationChallengeResult(nonResult, null),
                    true,
                    typeof(SchemaItemAuthenticationCompletedLogEntry),
                    LogLevel.Warning,
                    LogEventIds.SchemaItemAuthenticationCompleted);
        }

        [Test]
        public void AuthenticationChallengeResult_SuccessResult_LoggedAtTrace()
        {
            var context = Substitute.For<IGraphQLMiddlewareExecutionContext>();
            var secRequest = Substitute.For<ISchemaItemSecurityRequest>();
            context.ServiceProvider.Returns(Substitute.For<IServiceProvider>());
            context.QueryRequest.Returns(Substitute.For<IQueryExecutionRequest>());
            context.Session.Returns(Substitute.For<IQuerySession>());

            var nonResult = new SchemaItemSecurityChallengeContext(
                          context,
                          secRequest);
            nonResult.Result = SchemaItemSecurityChallengeResult.Success(new ClaimsPrincipal());

            this.ExecuteTest(
                    (DefaultGraphEventLogger x) => x.SchemaItemAuthenticationChallengeResult(nonResult, null),
                    true,
                    typeof(SchemaItemAuthenticationCompletedLogEntry),
                    LogLevel.Trace,
                    LogEventIds.SchemaItemAuthenticationCompleted);
        }

        [Test]
        public void AuthenticationChallengeResult_NullEverything_LoggedAtWarning()
        {
            this.ExecuteTest(
                    (DefaultGraphEventLogger x) => x.SchemaItemAuthenticationChallengeResult(null, null),
                    true,
                    typeof(SchemaItemAuthenticationCompletedLogEntry),
                    LogLevel.Warning,
                    LogEventIds.SchemaItemAuthenticationCompleted);
        }

        [Test]
        public void AuthenticationChallengeResult_UnSucessfulAuthenticationResult_LoggedAtTrace()
        {
            var context = Substitute.For<IGraphQLMiddlewareExecutionContext>();
            var secRequest = Substitute.For<ISchemaItemSecurityRequest>();
            context.ServiceProvider.Returns(Substitute.For<IServiceProvider>());
            context.QueryRequest.Returns(Substitute.For<IQueryExecutionRequest>());
            context.Session.Returns(Substitute.For<IQuerySession>());

            var authResult = Substitute.For<IAuthenticationResult>();
            authResult.Suceeded.Returns(false);

            this.ExecuteTest(
                    (DefaultGraphEventLogger x) => x.SchemaItemAuthenticationChallengeResult(null, authResult),
                    true,
                    typeof(SchemaItemAuthenticationCompletedLogEntry),
                    LogLevel.Warning,
                    LogEventIds.SchemaItemAuthenticationCompleted);
        }

        [Test]
        public void AuthenticationChallengeResult_SucessfulAuthenticationResult_LoggedAtTrace()
        {
            var context = Substitute.For<IGraphQLMiddlewareExecutionContext>();
            var secRequest = Substitute.For<ISchemaItemSecurityRequest>();
            context.ServiceProvider.Returns(Substitute.For<IServiceProvider>());
            context.QueryRequest.Returns(Substitute.For<IQueryExecutionRequest>());
            context.Session.Returns(Substitute.For<IQuerySession>());

            var authResult = Substitute.For<IAuthenticationResult>();
            authResult.Suceeded.Returns(true);

            this.ExecuteTest(
                    (DefaultGraphEventLogger x) => x.SchemaItemAuthenticationChallengeResult(null, authResult),
                    true,
                    typeof(SchemaItemAuthenticationCompletedLogEntry),
                    LogLevel.Trace,
                    LogEventIds.SchemaItemAuthenticationCompleted);
        }
    }
}