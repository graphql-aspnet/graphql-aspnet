// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Controllers.InputModel;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Logging.GeneralEvents;
    using GraphQL.AspNet.Security;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The default logger implementation for use in graphql operations. This logger will automatically append
    /// a unique instance id to each log entry created through it. When injected into a DI container as a
    /// "scoped" lifetime (the default behavior) this has an effect of attaching a unique id to all messages generated for each graphql request coming
    /// through the system for easy tracking.
    /// </summary>
    public class DefaultGraphEventLogger : IGraphEventLogger
    {
        private readonly ILogger _logger;

        // a unique id under which all entries of this logger are recorded
        // since (by default) the logger is created in a scoped setting
        // this id will be unique per http request
        private readonly Guid _loggerInstanceId;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGraphEventLogger" /> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory from which to generate the underlying <see cref="ILogger" />.</param>
        public DefaultGraphEventLogger(ILoggerFactory loggerFactory)
        {
            Validation.ThrowIfNull(loggerFactory, nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger(Constants.Logging.LOG_CATEGORY);
            _loggerInstanceId = Guid.NewGuid();
        }

        /// <summary>
        /// Logs the given entry at the provided level. A scope id property is automatically added to the log
        /// entry indicating this specific logger instance wrote the entry.
        /// </summary>
        /// <param name="logLevel">The log level to record the entry at.</param>
        /// <param name="logEntry">The log entry to record.</param>
        public virtual void LogEvent(LogLevel logLevel, GraphLogEntry logEntry)
        {
            logEntry.AddProperty(LogPropertyNames.SCOPE_ID, _loggerInstanceId);
            this.Log(logLevel, logEntry);
        }

        /// <inheritdoc />
        public virtual void SchemaInstanceCreated<TSchema>(TSchema schema)
            where TSchema : class, ISchema
        {
            if (!this.IsEnabled(LogLevel.Debug))
                return;

            var entry = new SchemaInstanceCreatedLogEntry<TSchema>(schema);
            this.LogEvent(LogLevel.Debug, entry);
        }

        /// <inheritdoc />
        public virtual void SchemaPipelineRegistered<TSchema>(ISchemaPipeline pipleine)
            where TSchema : class, ISchema
        {
            if (!this.IsEnabled(LogLevel.Debug))
                return;

            var entry = new SchemaPipelineRegisteredLogEntry<TSchema>(pipleine);
            this.LogEvent(LogLevel.Debug, entry);
        }

        /// <inheritdoc />
        public virtual void SchemaUrlRouteRegistered<TSchema>(string routePath)
            where TSchema : class, ISchema
        {
            if (!this.IsEnabled(LogLevel.Debug))
                return;

            var entry = new SchemaRouteRegisteredLogEntry<TSchema>(routePath);
            this.LogEvent(LogLevel.Debug, entry);
        }

        /// <inheritdoc />
        public virtual void RequestReceived(QueryExecutionContext queryContext)
        {
            if (!this.IsEnabled(LogLevel.Debug))
                return;

            var entry = new RequestReceivedLogEntry(queryContext);
            this.LogEvent(LogLevel.Trace, entry);
        }

        /// <inheritdoc />
        public virtual void QueryPlanCacheFetchHit<TSchema>(string queryHash)
            where TSchema : class, ISchema
        {
            if (!this.IsEnabled(LogLevel.Trace))
                return;

            var entry = new QueryExecutionPlanCacheHitLogEntry<TSchema>(queryHash);
            this.LogEvent(LogLevel.Trace, entry);
        }

        /// <inheritdoc />
        public virtual void QueryPlanCacheFetchMiss<TSchema>(string queryHash)
            where TSchema : class, ISchema
        {
            if (!this.IsEnabled(LogLevel.Trace))
                return;

            var entry = new QueryExecutionPlanCacheMissLogEntry<TSchema>(queryHash);
            this.LogEvent(LogLevel.Trace, entry);
        }

        /// <inheritdoc />
        public virtual void QueryPlanCached(string queryHash, IQueryExecutionPlan queryPlan)
        {
            if (!this.IsEnabled(LogLevel.Debug))
                return;

            var entry = new QueryExecutionPlanCacheAddLogEntry(queryHash, queryPlan);
            this.LogEvent(LogLevel.Debug, entry);
        }

        /// <inheritdoc />
        public virtual void QueryPlanGenerated(IQueryExecutionPlan queryPlan)
        {
            if (!this.IsEnabled(LogLevel.Trace))
                return;

            var entry = new QueryExecutionPlanGeneratedLogEntry(queryPlan);
            this.LogEvent(LogLevel.Trace, entry);
        }

        /// <inheritdoc />
        public virtual void FieldResolutionStarted(FieldResolutionContext context)
        {
            if (!this.IsEnabled(LogLevel.Trace))
                return;

            var entry = new FieldResolutionStartedLogEntry(context);
            this.LogEvent(LogLevel.Trace, entry);
        }

        /// <inheritdoc />
        public virtual void SchemaItemAuthorizationChallenge(SchemaItemSecurityChallengeContext context)
        {
            if (!this.IsEnabled(LogLevel.Trace))
                return;

            var entry = new SchemaItemAuthorizationStartedLogEntry(context);
            this.LogEvent(LogLevel.Trace, entry);
        }

        /// <inheritdoc />
        public virtual void SchemaItemAuthorizationChallengeResult(SchemaItemSecurityChallengeContext context)
        {
            var logLevel = context?.Result == null || context.Result.Status == SchemaItemSecurityChallengeStatus.Unauthorized
                ? LogLevel.Warning
                : LogLevel.Trace;

            if (!this.IsEnabled(logLevel))
                return;

            var entry = new SchemaItemAuthorizationCompletedLogEntry(context);
            this.LogEvent(logLevel, entry);
        }

        /// <inheritdoc />
        public virtual void SchemaItemAuthenticationChallenge(SchemaItemSecurityChallengeContext context)
        {
            if (!this.IsEnabled(LogLevel.Trace))
                return;

            var entry = new SchemaItemAuthenticationStartedLogEntry(context);
            this.LogEvent(LogLevel.Trace, entry);
        }

        /// <inheritdoc />
        public virtual void SchemaItemAuthenticationChallengeResult(SchemaItemSecurityChallengeContext context, IAuthenticationResult authResult)
        {
            LogLevel logLevel;
            if (context?.Result != null)
            {
                logLevel = context.Result.Status == SchemaItemSecurityChallengeStatus.Failed
                ? LogLevel.Warning
                : LogLevel.Trace;
            }
            else
            {
                logLevel = authResult == null || !authResult.Suceeded ? LogLevel.Warning : LogLevel.Trace;
            }

            if (!this.IsEnabled(logLevel))
                return;

            var entry = new SchemaItemAuthenticationCompletedLogEntry(context, authResult);
            this.LogEvent(logLevel, entry);
        }

        /// <inheritdoc />
        public virtual void FieldResolutionCompleted(FieldResolutionContext context)
        {
            if (!this.IsEnabled(LogLevel.Trace))
                return;

            var entry = new FieldResolutionCompletedLogEntry(context);
            this.LogEvent(LogLevel.Trace, entry);
        }

        /// <inheritdoc />
        public virtual void ActionMethodInvocationRequestStarted(IGraphFieldResolverMetaData action, IDataRequest request)
        {
            if (!this.IsEnabled(LogLevel.Trace))
                return;

            var entry = new ActionMethodInvocationStartedLogEntry(action, request);
            this.LogEvent(LogLevel.Trace, entry);
        }

        /// <inheritdoc />
        public virtual void ActionMethodModelStateValidated(IGraphFieldResolverMetaData action, IDataRequest request, InputModelStateDictionary modelState)
        {
            if (!this.IsEnabled(LogLevel.Trace))
                return;

            var entry = new ActionMethodModelStateValidatedLogEntry(action, request, modelState);
            this.LogEvent(LogLevel.Trace, entry);
        }

        /// <inheritdoc />
        public virtual void ActionMethodInvocationException(IGraphFieldResolverMetaData action, IDataRequest request, Exception exception)
        {
            if (!this.IsEnabled(LogLevel.Error))
                return;

            var entry = new ActionMethodInvocationExceptionLogEntry(action, request, exception);
            this.LogEvent(LogLevel.Error, entry);
        }

        /// <inheritdoc />
        public virtual void ActionMethodUnhandledException(IGraphFieldResolverMetaData action, IDataRequest request, Exception exception)
        {
            if (!this.IsEnabled(LogLevel.Error))
                return;

            var entry = new ActionMethodUnhandledExceptionLogEntry(action, request, exception);
            this.LogEvent(LogLevel.Error, entry);
        }

        /// <inheritdoc />
        public virtual void ActionMethodInvocationCompleted(IGraphFieldResolverMetaData action, IDataRequest request, object result)
        {
            if (!this.IsEnabled(LogLevel.Trace))
                return;

            var entry = new ActionMethodInvocationCompletedLogEntry(action, request, result);
            this.LogEvent(LogLevel.Trace, entry);
        }

        /// <inheritdoc />
        public virtual void RequestCompleted(QueryExecutionContext queryContext)
        {
            if (!this.IsEnabled(LogLevel.Trace))
                return;

            var entry = new RequestCompletedLogEntry(queryContext);
            this.LogEvent(LogLevel.Trace, entry);
        }

        /// <inheritdoc />
        public virtual void RequestTimedOut(QueryExecutionContext queryContext)
        {
            if (!this.IsEnabled(LogLevel.Warning))
                return;

            var entry = new RequestTimedOutLogEntry(queryContext);
            this.LogEvent(LogLevel.Warning, entry);
        }

        /// <inheritdoc />
        public virtual void RequestCancelled(QueryExecutionContext queryContext)
        {
            if (!this.IsEnabled(LogLevel.Information))
                return;

            var entry = new RequestCancelledLogEntry(queryContext);
            this.LogEvent(LogLevel.Information, entry);
        }

        /// <inheritdoc />
        public virtual void TypeSystemDirectiveApplied<TSchema>(IDirective appliedDirective, ISchemaItem appliedTo)
            where TSchema : class, ISchema
        {
            if (!this.IsEnabled(LogLevel.Debug))
                return;

            var entry = new TypeSystemDirectiveAppliedLogEntry<TSchema>(appliedDirective, appliedTo);
            this.Log(LogLevel.Debug, entry);
        }

        /// <inheritdoc />
        public virtual void ExecutionDirectiveApplied<TSchema>(IDirective appliedDirective, IDocumentPart appliedTo)
            where TSchema : class, ISchema
        {
            if (!this.IsEnabled(LogLevel.Trace))
                return;

            var entry = new ExecutionDirectiveAppliedLogEntry<TSchema>(appliedDirective, appliedTo);
            this.Log(LogLevel.Trace, entry);
        }

        /// <inheritdoc />
        public virtual bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        /// <inheritdoc />
        public virtual void Log(LogLevel logLevel, IGraphLogEntry logEntry)
        {
            if (logEntry == null || !this.IsEnabled(logLevel))
                return;

            this.Log(logLevel, logEntry.EventId, logEntry, null, (state, _) => state.ToString());
        }

        /// <inheritdoc />
        public virtual void Log(LogLevel logLevel, Func<IGraphLogEntry> entryMaker)
        {
            if (!this.IsEnabled(logLevel))
                return;

            var logEntry = entryMaker?.Invoke();
            if (logEntry != null)
                this.Log(logLevel, logEntry.EventId, logEntry, null, (state, _) => state.ToString());
        }

        /// <inheritdoc />
        public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }

        /// <inheritdoc />
        public virtual IDisposable BeginScope<TState>(TState state)
        {
            return _logger.BeginScope(state);
        }
    }
}