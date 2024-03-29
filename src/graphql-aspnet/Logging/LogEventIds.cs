﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging
{
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Schema;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A collection of log events related to graph ql.
    /// </summary>
    public static class LogEventIds
    {
        /// <summary>
        /// Gets or sets the base event number indicating an event is of a general or misc. variety.
        /// <remarks>
        /// The value is global for all schemas and should be set prior to calling any graphql
        /// setup functions.
        /// </remarks>
        /// </summary>
        /// <value>The root general event identifier.</value>
        public static int ROOT_GENERAL_EVENT_ID { get; set; } = 85000;

        /// <summary>
        /// Gets or sets the base number indicating an event that specifically relates to the
        /// execution/completion of a graphql query.
        /// <remarks>
        /// The value is global for all schemas and should be set prior to calling any graphql
        /// setup functions.
        /// </remarks>
        /// </summary>
        /// <value>The root execution event identifier.</value>
        public static int ROOT_EXECUTION_EVENT_ID { get; set; } = 86000;

        /// <summary>
        /// An general, untyped log event not relating to any specific execution event. Typically
        /// raised by developer code.
        /// </summary>
        public static EventId General = new EventId(ROOT_GENERAL_EVENT_ID, "GraphQL General Event");

        /// <summary>
        /// A log event indicating a DI container generated a new instance of a graph schema.
        /// </summary>
        public static EventId SchemaInstanceCreated = new EventId(ROOT_GENERAL_EVENT_ID + 100, "GraphQL Schema Instance Created");

        /// <summary>
        /// A log event indicating a DI container generated a new instance of a graph schema's execution pipeline.
        /// </summary>
        public static EventId SchemaPipelineInstanceCreated = new EventId(ROOT_GENERAL_EVENT_ID + 110, "GraphQL Schema Pipeline Instance Created");

        /// <summary>
        /// A log event indicating the runtime successfully registered an ASP.NET route to serve requests for a graph schema.
        /// </summary>
        public static EventId SchemaRouteRegistered = new EventId(ROOT_GENERAL_EVENT_ID + 120, "GraphQL Schema Route Registered");

        /// <summary>
        /// A log event indicating an unhandled exception was logged out of context of any other
        /// event or execution phase.
        /// </summary>
        public static EventId UnhandledException = new EventId(ROOT_GENERAL_EVENT_ID + 200, "GraphQL Unhandled Exception");

        /// <summary>
        /// A log event indicating a new graphql request was recieved and needs to be processed.
        /// </summary>
        public static EventId RequestReceived = new EventId(ROOT_EXECUTION_EVENT_ID, "GraphQL Request Received");

        /// <summary>
        /// A log event indicating that a query plan was successfully retrieved from the cache.
        /// </summary>
        public static EventId QueryCacheHit = new EventId(ROOT_EXECUTION_EVENT_ID + 300, "GraphQL Query Cache Hit");

        /// <summary>
        /// A log event indicating that a query plan was not found in the cache and needs
        /// to be generated.
        /// </summary>
        public static EventId QueryCacheMiss = new EventId(ROOT_EXECUTION_EVENT_ID + 310, "GraphQL Query Cache Miss");

        /// <summary>
        /// A log event indicating that a query plan was successfulled added to the cache.
        /// </summary>
        public static EventId QueryCacheAdd = new EventId(ROOT_EXECUTION_EVENT_ID + 350, "GraphQL Query Cache Add");

        /// <summary>
        /// A log event indicating that a query plan and has completed its generation process.
        /// </summary>
        public static EventId QueryPlanGenerationCompleted = new EventId(ROOT_EXECUTION_EVENT_ID + 400, "GraphQL Query Plan Generated");

        /// <summary>
        /// A log event indicating that a single field of data for one or more items
        /// has started and is going to execute its resolver and validation routines.
        /// </summary>
        public static EventId FieldResolutionStarted = new EventId(ROOT_EXECUTION_EVENT_ID + 500, "GraphQL Field Resolution Started");

        /// <summary>
        /// A log event that occurs when the security middleware initiates an authentication challenge to evaluate
        /// security policies against a schema item (commonly a field or directive).
        /// </summary>
        public static EventId SchemaItemAuthenticationStarted = new EventId(ROOT_EXECUTION_EVENT_ID + 510, "GraphQL Schema Item Authentication Started");

        /// <summary>
        /// A log event that occurs when the security middleware completes an authentication challenge
        /// against a schema item (commonly a field or directive).
        /// </summary>
        public static EventId SchemaItemAuthenticationCompleted = new EventId(ROOT_EXECUTION_EVENT_ID + 515, "GraphQL Schema Item Authentication Completed");

        /// <summary>
        /// A log event that occurs when the security middleware initiates an authorization challenge to evaluate
        /// security policies against a schema item (commonly a field or directive).
        /// </summary>
        public static EventId SchemaItemAuthorizationStarted = new EventId(ROOT_EXECUTION_EVENT_ID + 520, "GraphQL Schema Item Authorization Started");

        /// <summary>
        /// A log event that occurs when the security middleware completes an authorization challenge
        /// against a schema item (commonly a field or directive).
        /// </summary>
        public static EventId SchemaItemAuthorizationCompleted = new EventId(ROOT_EXECUTION_EVENT_ID + 530, "GraphQL SchemaItem Authorization Completed");

        /// <summary>
        /// A log event indicating that a single field of data in a query has completed
        /// its resolver invocation and data validation routines.
        /// </summary>
        public static EventId FieldResolutionCompleted = new EventId(ROOT_EXECUTION_EVENT_ID + 599, "GraphQL Field Resolution Completed");

        /// <summary>
        /// A log event indicating that a controller has accepted a request to invoke an action
        /// method and is preparing a data package for invocation.
        /// </summary>
        public static EventId ControllerInvocationStarted = new EventId(ROOT_EXECUTION_EVENT_ID + 600, "GraphQL Field Controller Action Started");

        /// <summary>
        /// A log event indicating that a controller has validated the model data for the target
        /// action method and produced a result.
        /// </summary>
        public static EventId ControllerModelValidated = new EventId(ROOT_EXECUTION_EVENT_ID + 610, "GraphQL Field Controller Action Model Validated");

        /// <summary>
        /// A log event indicating that a controller has encountered an error attempting to invoke
        /// the requested action method.
        /// </summary>
        public static EventId ControllerInvocationException = new EventId(ROOT_EXECUTION_EVENT_ID + 620, "GraphQL Field Controller Action Invocation Exception");

        /// <summary>
        /// A log event indicating that a controller, while having successfully invoked the action
        /// method, encountered an exception in the resolver's code and cannot continue.
        /// </summary>
        public static EventId ControllerUnhandledException = new EventId(ROOT_EXECUTION_EVENT_ID + 630, "GraphQL Field Controller Action Unhandled Exception");

        /// <summary>
        /// A log envet indicating that a controller invoked an action method
        /// and generated a reslt.
        /// </summary>
        public static EventId ControllerInvocationCompleted = new EventId(ROOT_EXECUTION_EVENT_ID + 640, "GraphQL Field Controller Action Completed");

        /// <summary>
        /// A log event indicating that a request has completed and a data package has been
        /// generated to be sent to a client.
        /// </summary>
        public static EventId RequestCompleted = new EventId(ROOT_EXECUTION_EVENT_ID + 700, "GraphQL Request Completed");

        /// <summary>
        /// A log evnet indicating that a given request took longer than the allowed query time
        /// for the schema was dropped.
        /// </summary>
        public static EventId RequestTimeout = new EventId(ROOT_EXECUTION_EVENT_ID + 710, "GraphQL Request Timed Out");

        /// <summary>
        /// A log event indicating that a given request was cancelled or dropped by an external actor
        /// (such as an HTTP request).
        /// </summary>
        public static EventId RequestCancelled = new EventId(ROOT_EXECUTION_EVENT_ID + 720, "GraphQL Request Cancelled");

        /// <summary>
        /// A log entry indicating that a directive was applied to a specific <see cref="ISchemaItem"/>
        /// during schema generation.
        /// </summary>
        public static EventId TypeSystemDirectiveApplied = new EventId(ROOT_EXECUTION_EVENT_ID + 810, "GraphQL Type System Directive Applied");

        /// <summary>
        /// A log entry indicating that a directive was applied to a specific <see cref="IDocumentPart"/>
        /// during query execution.
        /// </summary>
        public static EventId ExecutionDirectiveApplied = new EventId(ROOT_EXECUTION_EVENT_ID + 820, "GraphQL Execution Directive Applied");
    }
}