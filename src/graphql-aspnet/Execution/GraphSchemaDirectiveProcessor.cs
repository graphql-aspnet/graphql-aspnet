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
    using System.Linq;
    using System.Threading;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.PlanGeneration.InputArguments;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Performs post processing on a built schema applying any assigned directives to their
    /// respective schema items.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema to work with.</typeparam>
    public class GraphSchemaDirectiveProcessor<TSchema>
        where TSchema : class, ISchema
    {
        private readonly SchemaOptions _options;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSchemaDirectiveProcessor{TSchema}" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="serviceProvider">The service provider used to instantiate
        /// and apply type system directives.</param>
        public GraphSchemaDirectiveProcessor(SchemaOptions options, IServiceProvider serviceProvider)
        {
            _options = Validation.ThrowIfNullOrReturn(options, nameof(options));
            _serviceProvider = Validation.ThrowIfNullOrReturn(serviceProvider, nameof(serviceProvider));
        }

        /// <summary>
        /// Scans all types, fields and arguments and applies any
        /// found directives in their declared order.
        /// </summary>
        /// <param name="schema">The schema to scan.</param>
        public void ApplyDirectives(TSchema schema)
        {
            // Process the schema
            this.ApplyDirectivesToItem(schema, schema);

            // process each graph type (includes top level operations)
            var graphTypesToProcess = schema.KnownTypes.Where(x => x.Kind != TypeKind.DIRECTIVE);
            foreach (var graphType in graphTypesToProcess)
            {
                this.ApplyDirectivesToItem(schema, graphType);

                if (graphType is IEnumGraphType enumType)
                {
                    // each option on each enum
                    foreach (var option in enumType.Values)
                        this.ApplyDirectivesToItem(schema, option.Value);
                }
                else if (graphType is IGraphFieldContainer fieldContainer)
                {
                    // each field in each graph type
                    foreach (var field in fieldContainer.Fields)
                    {
                        this.ApplyDirectivesToItem(schema, field);

                        // each argument on each field
                        foreach (var argument in field.Arguments)
                            this.ApplyDirectivesToItem(schema, argument);
                    }
                }
            }
        }

        private void ApplyDirectivesToItem(TSchema schema, ISchemaItem item)
        {
            var fullRouteName = item.Name;
            foreach (var appliedDirective in item.AppliedDirectives)
            {
                var scopedProvider = _serviceProvider.CreateScope();

                var directivePipeline = scopedProvider.ServiceProvider.GetService<ISchemaPipeline<TSchema, GraphDirectiveExecutionContext>>();
                if (directivePipeline == null)
                {
                    throw new GraphExecutionException($"Unable to construct the schema '{schema.Name}'. " +
                          $"No valid directive processing pipeline was found.");
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
                        $"The supplied directive type '{directiveName}' " +
                        $"does not represent a valid directive on the target schema. (Target: '{item.Route.Path}', Schema: {schema.Name})";

                    throw new GraphTypeDeclarationException(failureMessage);
                }

                var inputArgs = this.GatherInputArguments(targetDirective, appliedDirective.Arguments);

                var parentRequest = new GraphOperationRequest(GraphQueryData.Empty);

                var invocationContext = new DirectiveInvocationContext(
                    targetDirective,
                    item.AsDirectiveLocation(),
                    SourceOrigin.None,
                    inputArgs);

                var request = new GraphDirectiveRequest(
                    invocationContext,
                    DirectiveInvocationPhase.SchemaGeneration,
                    item);

                var logger = scopedProvider.ServiceProvider.GetService<IGraphEventLogger>();
                var context = new GraphDirectiveExecutionContext(
                    schema,
                    scopedProvider.ServiceProvider,
                    parentRequest,
                    request,
                    null as IGraphQueryExecutionMetrics,
                    logger,
                    items: request.Items);

                directivePipeline.InvokeAsync(context, CancellationToken.None).Wait();
            }
        }

        private IInputArgumentCollection GatherInputArguments(IDirective targetDirective, object[] arguments)
        {
            var argCollection = new InputArgumentCollection();
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