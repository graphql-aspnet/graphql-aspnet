// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Configuration.Exceptions;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Performs post processing on a built schema applying any assigned directives to their
    /// respective schema items.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema to work with.</typeparam>
    internal sealed class DirectiveProcessorTypeSystem<TSchema>
        where TSchema : class, ISchema
    {
        private readonly IQuerySession _querySession;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveProcessorTypeSystem{TSchema}" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to instantiate
        /// and apply type system directives.</param>
        /// <param name="querySession">The query session to be shared by
        /// all applied directives.</param>
        public DirectiveProcessorTypeSystem(IServiceProvider serviceProvider, IQuerySession querySession)
        {
            _querySession = Validation.ThrowIfNullOrReturn(querySession, nameof(querySession));
            _serviceProvider = Validation.ThrowIfNullOrReturn(serviceProvider, nameof(serviceProvider));
        }

        /// <summary>
        /// Scans the target schema looking for any type system directives that should
        /// be applied to any schema items. Those directives are executed against their targets
        /// using standard directive pipeline.
        /// </summary>
        /// <param name="schema">The schema to apply directives too.</param>
        public void ApplyDirectives(TSchema schema)
        {
            // all schema items
            var anyDirectivesApplied = false;
            foreach (var item in schema.AllSchemaItems())
                anyDirectivesApplied = this.ApplyDirectivesToItem(schema, item) || anyDirectivesApplied;
        }

        /// <summary>
        /// Applies the directives to item.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="item">The item.</param>
        private bool ApplyDirectivesToItem(TSchema schema, ISchemaItem item)
        {
            var invokedDirectives = new HashSet<IDirective>();
            foreach (var appliedDirective in item.AppliedDirectives)
            {
                var scopedProvider = _serviceProvider.CreateScope();

                var directivePipeline = scopedProvider.ServiceProvider.GetService<ISchemaPipeline<TSchema, GraphDirectiveExecutionContext>>();
                if (directivePipeline == null)
                {
                    throw new SchemaConfigurationException($"Unable to construct the schema '{schema.Name}'. " +
                          $"No valid directive processing pipeline was found in the service provider.");
                }

                IDirective targetDirective = null;
                if (appliedDirective.DirectiveType != null)
                    targetDirective = schema.KnownTypes.FindDirective(appliedDirective.DirectiveType);
                else if (!string.IsNullOrWhiteSpace(appliedDirective.DirectiveName))
                    targetDirective = schema.KnownTypes.FindDirective(appliedDirective.DirectiveName);

                if (targetDirective == null)
                {
                    var directiveName = appliedDirective.DirectiveType?.FriendlyName() ?? appliedDirective.DirectiveName ?? "-unknown-";
                    var failureMessage =
                        $"Type System Directive Invocation Failure. " +
                        $"The directive type named '{directiveName}' " +
                        $"does not represent a valid directive on the target schema. (Target: '{item.Route.Path}', Schema: {schema.Name})";

                    throw new SchemaConfigurationException(failureMessage);
                }

                // ensure that repeated directives on the type system
                // are in fact repeatable
                if (invokedDirectives.Contains(targetDirective))
                {
                    if (!targetDirective.IsRepeatable)
                    {
                        throw new SchemaConfigurationException(
                            $"Unable to construct the schema '{schema.Name}'. " +
                            $"The non-repeatable directive @{targetDirective.Name} is repeated on the schema item '{item.Name}'. (Target: '{item.Route.Path}', Schema: {schema.Name})");
                    }
                }

                invokedDirectives.Add(targetDirective);

                var inputArgs = this.GatherInputArguments(targetDirective, appliedDirective.ArgumentValues);

                var operationRequest = new QueryExecutionRequest(GraphQueryData.Empty);

                var invocationContext = new DirectiveInvocationContext(
                    targetDirective,
                    item.AsDirectiveLocation(),
                    SourceOrigin.None,
                    inputArgs);

                var directiveRequest = new GraphDirectiveRequest(
                    invocationContext,
                    DirectiveInvocationPhase.SchemaGeneration,
                    item);

                var logger = scopedProvider.ServiceProvider.GetService<IGraphEventLogger>();
                var context = new GraphDirectiveExecutionContext(
                    schema,
                    directiveRequest,
                    operationRequest,
                    scopedProvider.ServiceProvider,
                    _querySession,
                    logger: logger,
                    metrics: null,
                    userSecurityContext: null);

                Exception causalException = null;

                try
                {
                    directivePipeline.InvokeAsync(context, CancellationToken.None).Wait();
                }
                catch (Exception ex)
                {
                    // SUPER FAIL!!!
                    context.Cancel();
                    causalException = ex;
                }

                if (context.IsCancelled || !context.Messages.IsSucessful)
                {
                    Type failedType = null;
                    if (item is ITypedSchemaItem tsi)
                        failedType = tsi.ObjectType;

                    // attempt to discover the reason for the failure if its contained within the
                    // executed context
                    if (causalException == null)
                    {
                        // when lots of failures are indicated
                        // nest them into each other before throwing
                        foreach (var message in context.Messages.Where(x => x.Severity.IsCritical()))
                        {
                            // when an actual exception was encountered
                            // use it as the causal exception
                            if (message.Exception != null)
                            {
                                causalException = message.Exception;
                                break;
                            }

                            // otherwise chain together any failure messages
                            var errorMessage = message.Message;
                            if (!string.IsNullOrWhiteSpace(message.Code))
                                errorMessage = message.Code + " : " + errorMessage;

                            causalException = new GraphTypeDeclarationException(
                                errorMessage,
                                failedType,
                                causalException);
                        }

                        if (causalException == null)
                        {
                            // out of options, cant figure out the issue
                            // just declare a general failure   ¯\_(ツ)_/¯
                            causalException = new GraphTypeDeclarationException(
                                $"An unknown error occured while applying a directive " +
                                    $"to graph type '{item.Name}' (Directive: '{targetDirective.Name}')",
                                failedType);
                        }
                    }

                    throw new SchemaConfigurationException(
                        $"An exception occured applying the type system directive '{targetDirective.Name}' to schema item '{item.Name}'. " +
                        $"See inner exception(s) for details.",
                        innerException: causalException);
                }
            }

            return item.AppliedDirectives.Count > 0;
        }

        private IInputArgumentCollection GatherInputArguments(IDirective targetDirective, object[] arguments)
        {
            var argCollection = new InputArgumentCollection(targetDirective.Arguments.Count);
            for (var i = 0; i < targetDirective.Arguments.Count; i++)
            {
                if (arguments.Length <= i)
                    break;

                var directiveArg = targetDirective.Arguments[i];
                var resolvedValue = arguments[i];

                var argValue = new ResolvedInputArgumentValue(directiveArg.Name, resolvedValue);
                var inputArg = new InputArgument(directiveArg, argValue);
                argCollection.Add(inputArg);
            }

            return argCollection;
        }
    }
}