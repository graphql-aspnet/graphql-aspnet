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
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Logging.GeneralEvents;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Security;
    using Microsoft.Extensions.Logging;
    using Moq;
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
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();

            logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(shouldLogLevelBeEnabled);

            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            IGraphLogEntry recordedEntry = null;

            if (shouldLogLevelBeEnabled)
            {
                logger.Setup(x => x.Log(
                     expectedLogLevel.Value,
                     expectedEventId.Value,
                     It.IsAny<IGraphLogEntry>(),
                     It.IsAny<Exception>(),
                     It.IsAny<Func<IGraphLogEntry, Exception, string>>()))
                    .Callback((
                        LogLevel level,
                        EventId evtId,
                        IGraphLogEntry entry,
                        Exception ex,
                        Func<IGraphLogEntry, Exception, string> func) =>
                    {
                        recordedEntry = entry;
                    });
            }

            var eventLogger = new DefaultGraphEventLogger(loggerFactory.Object);
            execute(eventLogger);

            if (shouldLogLevelBeEnabled)
            {
                logger.Verify(
                   x => x.Log(
                        expectedLogLevel.Value,
                        expectedEventId.Value,
                        It.IsAny<IGraphLogEntry>(),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<IGraphLogEntry, Exception, string>>()),
                   Times.Once());

                Assert.AreEqual(logEntryType, recordedEntry.GetType());
            }
            else
            {
                logger.Verify(
                   x => x.Log(
                        It.IsAny<LogLevel>(),
                        It.IsAny<EventId>(),
                        It.IsAny<IGraphLogEntry>(),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<IGraphLogEntry, Exception, string>>()),
                   Times.Never());

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
                        new Mock<ISchemaPipeline>().Object),
                    true,
                    typeof(SchemaPipelineRegisteredLogEntry<GraphSchema>),
                    LogLevel.Debug,
                    LogEventIds.SchemaPipelineInstanceCreated,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.SchemaPipelineRegistered<GraphSchema>(
                        new Mock<ISchemaPipeline>().Object),
                    false,
                    null,
                    null,
                    null,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.SchemaRouteRegistered<GraphSchema>(null),
                    true,
                    typeof(SchemaRouteRegisteredLogEntry<GraphSchema>),
                    LogLevel.Debug,
                    LogEventIds.SchemaRouteRegistered,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.SchemaRouteRegistered<GraphSchema>(null),
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
                    typeof(QueryPlanCacheHitLogEntry<GraphSchema>),
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
                    typeof(QueryPlanCacheMissLogEntry<GraphSchema>),
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
                    typeof(QueryPlanCacheAddLogEntry),
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
                    typeof(QueryPlanGeneratedLogEntry),
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
                        new Mock<IDirective>().Object,
                        new Mock<IDocumentPart>().Object),
                    true,
                    typeof(ExecutionDirectiveAppliedLogEntry<GraphSchema>),
                    LogLevel.Trace,
                    LogEventIds.ExecutionDirectiveApplied,
                });

            _eventLoggerTestData.Add(
                new object[]
                {
                    (DefaultGraphEventLogger x) => x.ExecutionDirectiveApplied<GraphSchema>(
                        new Mock<IDirective>().Object,
                        new Mock<IDocumentPart>().Object),
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
            var context = new Mock<IMiddlewareExecutionContext>();
            var secRequest = new Mock<ISchemaItemSecurityRequest>();
            context.Setup(x => x.ServiceProvider).Returns(new Mock<IServiceProvider>().Object);
            context.Setup(x => x.OperationRequest).Returns(new Mock<IQueryOperationRequest>().Object);
            context.Setup(x => x.Session).Returns(new Mock<IQuerySession>().Object);

            var failedAuthResult = new SchemaItemSecurityChallengeContext(
                            context.Object,
                            secRequest.Object);
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
            var context = new Mock<IMiddlewareExecutionContext>();
            var secRequest = new Mock<ISchemaItemSecurityRequest>();
            context.Setup(x => x.ServiceProvider).Returns(new Mock<IServiceProvider>().Object);
            context.Setup(x => x.OperationRequest).Returns(new Mock<IQueryOperationRequest>().Object);
            context.Setup(x => x.Session).Returns(new Mock<IQuerySession>().Object);

            var nonFailResult = new SchemaItemSecurityChallengeContext(
                          context.Object,
                          secRequest.Object);
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
            var context = new Mock<IMiddlewareExecutionContext>();
            var secRequest = new Mock<ISchemaItemSecurityRequest>();
            context.Setup(x => x.ServiceProvider).Returns(new Mock<IServiceProvider>().Object);
            context.Setup(x => x.OperationRequest).Returns(new Mock<IQueryOperationRequest>().Object);
            context.Setup(x => x.Session).Returns(new Mock<IQuerySession>().Object);

            var nonResult = new SchemaItemSecurityChallengeContext(
                          context.Object,
                          secRequest.Object);
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
            var context = new Mock<IMiddlewareExecutionContext>();
            var secRequest = new Mock<ISchemaItemSecurityRequest>();
            context.Setup(x => x.ServiceProvider).Returns(new Mock<IServiceProvider>().Object);
            context.Setup(x => x.OperationRequest).Returns(new Mock<IQueryOperationRequest>().Object);
            context.Setup(x => x.Session).Returns(new Mock<IQuerySession>().Object);

            var nonResult = new SchemaItemSecurityChallengeContext(
                          context.Object,
                          secRequest.Object);
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
            var context = new Mock<IMiddlewareExecutionContext>();
            var secRequest = new Mock<ISchemaItemSecurityRequest>();
            context.Setup(x => x.ServiceProvider).Returns(new Mock<IServiceProvider>().Object);
            context.Setup(x => x.OperationRequest).Returns(new Mock<IQueryOperationRequest>().Object);
            context.Setup(x => x.Session).Returns(new Mock<IQuerySession>().Object);

            var nonResult = new SchemaItemSecurityChallengeContext(
                          context.Object,
                          secRequest.Object);
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
            var context = new Mock<IMiddlewareExecutionContext>();
            var secRequest = new Mock<ISchemaItemSecurityRequest>();
            context.Setup(x => x.ServiceProvider).Returns(new Mock<IServiceProvider>().Object);
            context.Setup(x => x.OperationRequest).Returns(new Mock<IQueryOperationRequest>().Object);
            context.Setup(x => x.Session).Returns(new Mock<IQuerySession>().Object);

            var authResult = new Mock<IAuthenticationResult>();
            authResult.Setup(x => x.Suceeded).Returns(false);

            this.ExecuteTest(
                    (DefaultGraphEventLogger x) => x.SchemaItemAuthenticationChallengeResult(null, authResult.Object),
                    true,
                    typeof(SchemaItemAuthenticationCompletedLogEntry),
                    LogLevel.Warning,
                    LogEventIds.SchemaItemAuthenticationCompleted);
        }

        [Test]
        public void AuthenticationChallengeResult_SucessfulAuthenticationResult_LoggedAtTrace()
        {
            var context = new Mock<IMiddlewareExecutionContext>();
            var secRequest = new Mock<ISchemaItemSecurityRequest>();
            context.Setup(x => x.ServiceProvider).Returns(new Mock<IServiceProvider>().Object);
            context.Setup(x => x.OperationRequest).Returns(new Mock<IQueryOperationRequest>().Object);
            context.Setup(x => x.Session).Returns(new Mock<IQuerySession>().Object);

            var authResult = new Mock<IAuthenticationResult>();
            authResult.Setup(x => x.Suceeded).Returns(true);

            this.ExecuteTest(
                    (DefaultGraphEventLogger x) => x.SchemaItemAuthenticationChallengeResult(null, authResult.Object),
                    true,
                    typeof(SchemaItemAuthenticationCompletedLogEntry),
                    LogLevel.Trace,
                    LogEventIds.SchemaItemAuthenticationCompleted);
        }
    }
}