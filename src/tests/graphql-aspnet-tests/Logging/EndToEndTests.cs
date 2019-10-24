// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Logging.LoggerTestData;
    using Microsoft.Extensions.Logging;
    using NUnit.Framework;
    using GraphQL.AspNet.Configuration.Mvc;

    [TestFixture]
    public class EndToEndTests
    {
        private TestServer<GraphSchema> CreateTestServer(out TestLogger logInstance)
        {
            // fake a logger with a single instance that will accept
            // log entries from the run time so they can be validated in the test.
            logInstance = new TestLogger();

            var builder = new TestServerBuilder();
            builder.AddGraphType<LogTestController>();
            builder.AddGraphQLLocalQueryCache();
            builder.Logging.AddLoggerProvider(new TestLoggingProvider(logInstance));
            builder.Logging.SetMinimumLogLevel(LogLevel.Trace);
            return builder.Build();
        }

        [Test]
        public async Task ExecuteStandardPipeline_EnsureStandardLogEntriesAreRecorded()
        {
            var server = this.CreateTestServer(out var logInstance);

            var processor = server.CreateHttpQueryProcessor();
            var context = server.CreateHttpContext(
                new GraphQueryData()
                {
                    Query = "{ field1 }",
                    OperationName = null,
                });

            await processor.Invoke(context);

            var startupEvents = new Dictionary<EventId, int>();
            startupEvents.Add(LogEventIds.SchemaInstanceCreated, 1);
            startupEvents.Add(LogEventIds.SchemaPipelineInstanceCreated, 3);

            // can't test schema route regstered due to
            // not mocking IAppBuilder
            // startupEvents.Add(LogEventIds.SchemaRouteRegistered);
            var requestEvents = new HashSet<EventId>();
            requestEvents.Add(LogEventIds.RequestReceived);
            requestEvents.Add(LogEventIds.QueryCacheMiss);
            requestEvents.Add(LogEventIds.QueryCacheAdd);
            requestEvents.Add(LogEventIds.QueryPlanGenerationCompleted);
            requestEvents.Add(LogEventIds.FieldResolutionStarted);
            requestEvents.Add(LogEventIds.FieldAuthorizationStarted);
            requestEvents.Add(LogEventIds.FieldAuthorizationCompleted);
            requestEvents.Add(LogEventIds.FieldResolutionCompleted);
            requestEvents.Add(LogEventIds.ControllerInvocationStarted);
            requestEvents.Add(LogEventIds.ControllerModelValidated);
            requestEvents.Add(LogEventIds.ControllerInvocationCompleted);
            requestEvents.Add(LogEventIds.RequestCompleted);

            // check that all scoped events share the same scope level id
            string scopeId = null;
            foreach (var entry in logInstance.LogEntries)
            {
                // skip scope id check for the startup events (they will always be seperate)
                if (!requestEvents.Contains(entry.EventId))
                    continue;

                var entryScopeId = entry.First(x => x.Key == LogPropertyNames.SCOPE_ID)
                        .Value
                        .ToString();

                if (string.IsNullOrWhiteSpace(scopeId))
                {
                    scopeId = entryScopeId;
                }
                else
                {
                    Assert.AreEqual(
                        scopeId,
                        entryScopeId,
                        $"Log Entry: {entry.GetType().Name}. Scope Id for all entries of the same type must be the same");
                }
            }

            var totalEvents = 0;

            // ensure the startup events are registered the correct amount of times
            foreach (var kvp in startupEvents)
            {
                var count = logInstance.LogEntries.Count(x => x.EventId == kvp.Key);
                totalEvents += count;
                Assert.AreEqual(
                    kvp.Value,
                    count,
                    $"Expected Event Id '{kvp.Key.Name} [{kvp.Key.Id}]' to appear {kvp.Value} time(s), but it was found {count} time(s).");
            }

            // ensure each request level event is only generated once
            foreach (var eventId in requestEvents)
            {
                var count = logInstance.LogEntries.Count(x => x.EventId == eventId);
                totalEvents += count;
                Assert.AreEqual(
                    1,
                    count,
                    $"Expected Event Id '{eventId.Name} [{eventId.Id}]' to appear once, but it was found {count} times.");
            }

            // ensure there is nothing extra that was recorded
            Assert.AreEqual(totalEvents, logInstance.LogEntries.Count);
        }

        [Test]
        public async Task ActionMethodThrowsException_LogEntryIsCreated()
        {
            var server = this.CreateTestServer(out var logInstance);

            var processor = server.CreateHttpQueryProcessor();
            var context = server.CreateHttpContext(
                new GraphQueryData()
                {
                    Query = "{ fieldException }",
                    OperationName = null,
                });

            await processor.Invoke(context);

            Assert.IsTrue(logInstance.LogEntries.Count > 0);

            var logEntry = logInstance
                .LogEntries
                .SingleOrDefault(x => x.EventId == LogEventIds.ControllerUnhandledException);
            Assert.IsNotNull(logEntry);
        }

        [Test]
        public async Task ActionMethodInvocationFails_LogEntryIsCreated()
        {
            var server = this.CreateTestServer(out var logInstance);
            var processor = server.CreateHttpQueryProcessor();
            var context = server.CreateHttpContext(
                new GraphQueryData()
                {
                    Query = "{ fakeInvocationException }",
                    OperationName = null,
                });

            await processor.Invoke(context);
            Assert.IsTrue(logInstance.LogEntries.Count > 0);

            var logEntry = logInstance
                .LogEntries
                .SingleOrDefault(x => x.EventId == LogEventIds.ControllerInvocationException);
            Assert.IsNotNull(logEntry);
        }

        [Test]
        public async Task QueryPlanAlreadyCached_ResultsInCacheHit()
        {
            var server = this.CreateTestServer(out var logInstance);
            var processor = server.CreateHttpQueryProcessor();
            var context = server.CreateHttpContext(
                new GraphQueryData()
                {
                    Query = "{ field1 }",
                    OperationName = null,
                });

            await processor.Invoke(context);
            logInstance.LogEntries.Clear();

            processor = server.CreateHttpQueryProcessor();
            context = server.CreateHttpContext(
                new GraphQueryData()
                {
                    Query = "{ field1 }",
                    OperationName = null,
                });

            // should fetch from cache
            await processor.Invoke(context);

            var logEntry = logInstance
                .LogEntries
                .SingleOrDefault(x => x.EventId == LogEventIds.QueryCacheHit);
            Assert.IsNotNull(logEntry);
        }
    }
}