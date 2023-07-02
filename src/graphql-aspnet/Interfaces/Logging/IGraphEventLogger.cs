﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Logging
{
    using System;
    using System.Security.Claims;
    using GraphQL.AspNet.Controllers.InputModel;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Security;

    /// <summary>
    /// A logging interface describing specific logged events in the completion of a graphql request.
    /// </summary>
    public interface IGraphEventLogger : IGraphLogger
    {
        /// <summary>
        /// Recorded when the startup services generates a new schema instance.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema that was generated.</typeparam>
        /// <param name="schema">The schema instance.</param>
        void SchemaInstanceCreated<TSchema>(TSchema schema)
            where TSchema : class, ISchema;

        /// <summary>
        /// Recorded when the startup services generate a new pipeline for the target schema.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema for which the pipeline was generated.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        void SchemaPipelineRegistered<TSchema>(ISchemaPipeline pipeline)
            where TSchema : class, ISchema;

        /// <summary>
        /// Recorded when the startup services registers a publically available ASP.NET route to which
        /// end users can submit graphql queries.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema the route was registered for.</typeparam>
        /// <param name="routePath">The relative route path (e.g. '/graphql').</param>
        void SchemaRouteRegistered<TSchema>(string routePath)
            where TSchema : class, ISchema;

        /// <summary>
        /// Recorded when a new request is generated by a query controller and passed to an
        /// executor for processing. This event is recorded before any action is taken.
        /// </summary>
        /// <param name="queryContext">The query context.</param>
        void RequestReceived(QueryExecutionContext queryContext);

        /// <summary>
        /// Recorded when an executor attempts to fetch a query plan from its local cache but failed
        /// either because the the plan is not cached or a retrieval operation failed.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema for which the query hash was generated.</typeparam>
        /// <param name="key">The key that was searched for in the cache.</param>
        void QueryPlanCacheFetchMiss<TSchema>(string key)
            where TSchema : class, ISchema;

        /// <summary>
        /// Recorded when the security middleware invokes an authentication challenge
        /// against an <see cref="IUserSecurityContext"/> to produce a <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="context">The field security context that contains the request to be authenticated.</param>
        void SchemaItemAuthenticationChallenge(SchemaItemSecurityChallengeContext context);

        /// <summary>
        /// Recorded when the security middleware completes an authentication challenge
        /// against an <see cref="IUserSecurityContext" /> to produce a <see cref="ClaimsPrincipal" />.
        /// </summary>
        /// <param name="context">The field security context that contains the request to be authenticated.</param>
        /// <param name="authResult">The authentication result that was created.</param>
        void SchemaItemAuthenticationChallengeResult(SchemaItemSecurityChallengeContext context, IAuthenticationResult authResult);

        /// <summary>
        /// Recorded when the security middleware invokes an authorization challenge
        /// against a <see cref="ClaimsPrincipal"/> to determine access to a field of data.
        /// </summary>
        /// <param name="context">The field security context that contains the <see cref="ClaimsPrincipal"/> to be authorized.</param>
        void SchemaItemAuthorizationChallenge(SchemaItemSecurityChallengeContext context);

        /// <summary>
        /// Recorded when the security middleware completes an authorization challenge
        /// against a <see cref="ClaimsPrincipal"/> to determine access to a field of data.
        /// </summary>
        /// <param name="context">The field security context that contains the <see cref="ClaimsPrincipal"/> to be authorized.</param>
        void SchemaItemAuthorizationChallengeResult(SchemaItemSecurityChallengeContext context);

        /// <summary>
        /// Recorded when an executor attempts, and succeeds, to retrieve a query plan from its local cache.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema for which the query hash was generated.</typeparam>
        /// <param name="key">The key that was searched for in the cache.</param>
        void QueryPlanCacheFetchHit<TSchema>(string key)
            where TSchema : class, ISchema;

        /// <summary>
        /// Recorded when an executor successfully caches a newly created query plan to its
        /// local cache for future use.
        /// </summary>
        /// <param name="key">The key the plan is to be cached under.</param>
        /// <param name="queryPlan">The completed plan that was cached.</param>
        void QueryPlanCached(string key, IQueryExecutionPlan queryPlan);

        /// <summary>
        /// Recorded when an executor finishes creating a query plan and is ready to
        /// cache and execute against it.
        /// </summary>
        /// <param name="queryPlan">The generated query plan.</param>
        void QueryPlanGenerated(IQueryExecutionPlan queryPlan);

        /// <summary>
        /// Recorded by a field resolver when it starts resolving a field context and
        /// set of source items given to it. This occurs prior to the middleware pipeline being executed.
        /// </summary>
        /// <param name="context">The field resolution context that is being completed.</param>
        void FieldResolutionStarted(FieldResolutionContext context);

        /// <summary>
        /// Recorded by a field resolver when it completes resolving a field context (and its children).
        /// This occurs after the middleware pipeline is executed.
        /// </summary>
        /// <param name="context">The context of the field resolution that was completed.</param>
        void FieldResolutionCompleted(FieldResolutionContext context);

        /// <summary>
        /// Recorded when a controller begins the invocation of an action method to resolve
        /// a field request.
        /// </summary>
        /// <param name="action">The action method on the controller being invoked.</param>
        /// <param name="request">The request being completed by the action method.</param>
        void ActionMethodInvocationRequestStarted(IGraphFieldResolverMetaData action, IDataRequest request);

        /// <summary>
        /// Recorded when a controller completes validation of the model data that will be passed
        /// to the action method.
        /// </summary>
        /// <param name="action">The action method on the controller being invoked.</param>
        /// <param name="request">The request being completed by the action method.</param>
        /// <param name="modelState">The model data that was validated.</param>
        void ActionMethodModelStateValidated(IGraphFieldResolverMetaData action, IDataRequest request, InputModelStateDictionary modelState);

        /// <summary>
        /// Recorded after a controller invokes and receives a result from an action method.
        /// </summary>
        /// <param name="action">The action method on the controller being invoked.</param>
        /// <param name="request">The request being completed by the action method.</param>
        /// <param name="result">The result object that was returned from the action method.</param>
        void ActionMethodInvocationCompleted(IGraphFieldResolverMetaData action, IDataRequest request, object result);

        /// <summary>
        /// Recorded when the invocation of action method generated a known exception; generally
        /// related to target invocation errors.
        /// </summary>
        /// <param name="action">The action method on the controller being invoked.</param>
        /// <param name="request">The request being completed by the action method.</param>
        /// <param name="exception">The exception that was generated.</param>
        void ActionMethodInvocationException(IGraphFieldResolverMetaData action, IDataRequest request, Exception exception);

        /// <summary>
        /// Recorded when the invocation of action method generated an unknown exception. This
        /// event is called when custom resolver code throws an unhandled exception.
        /// </summary>
        /// <param name="action">The action method on the controller being invoked.</param>
        /// <param name="request">The request being completed by the action method.</param>
        /// <param name="exception">The exception that was generated.</param>
        void ActionMethodUnhandledException(IGraphFieldResolverMetaData action, IDataRequest request, Exception exception);

        /// <summary>
        /// Recorded by an executor after the entire graphql operation has been completed
        /// and final results have been generated.
        /// </summary>
        /// <param name="queryContext">The query context.</param>
        void RequestCompleted(QueryExecutionContext queryContext);

        /// <summary>
        /// Recorded by an executor after the query failed to complete within the expected
        /// amount of time.
        /// </summary>
        /// <param name="queryContext">The query context.</param>
        void RequestTimedOut(QueryExecutionContext queryContext);

        /// <summary>
        /// Recorded by an executor after the request was cancelled by an external actor.
        /// </summary>
        /// <param name="queryContext">The query context.</param>
        void RequestCancelled(QueryExecutionContext queryContext);

        /// <summary>
        /// Recorded when, during schema generation, a type system directive is successfully applied
        /// to a targeted schema item.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema the on which the directive was applied.</typeparam>
        /// <param name="appliedDirective">The directive that has been applied.</param>
        /// <param name="appliedTo">The schema item the directive was applied to.</param>
        void TypeSystemDirectiveApplied<TSchema>(IDirective appliedDirective, ISchemaItem appliedTo)
            where TSchema : class, ISchema;

        /// <summary>
        /// Recorded whem, during a query execution, an execution directive is successfully applied
        /// to its target documnet part.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema the under which the directive was applied.</typeparam>
        /// <param name="appliedDirective">The applied directive.</param>
        /// <param name="appliedTo">The part of the query document the directive was applied to.</param>
        void ExecutionDirectiveApplied<TSchema>(IDirective appliedDirective, IDocumentPart appliedTo)
            where TSchema : class, ISchema;
    }
}