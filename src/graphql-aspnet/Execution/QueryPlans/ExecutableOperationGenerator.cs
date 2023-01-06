// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A generator capable of converting a single <see cref="IOperationDocumentPart"/> into
    /// an actionable execution context containing the necessary data, steps,
    /// resolvers, analyzers etc. to fulfill a request made from it.
    /// </summary>
    internal class ExecutableOperationGenerator
    {
        private readonly ISchema _schema;
        private readonly ArgumentGenerator _argumentGenerator;
        private IGraphMessageCollection _messages;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutableOperationGenerator" /> class.
        /// </summary>
        /// <param name="schema">The schema used by this generator to find resolvers
        /// and look up various data items.</param>
        public ExecutableOperationGenerator(ISchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _argumentGenerator = new ArgumentGenerator(schema);
        }

        /// <summary>
        /// Creates an executable operation context from the given query document operation. This method
        /// marries the parsed requirements from a schema to the concrete method and property implementations
        /// in the user's sourcecode. All abstract graph types are materialized and only concrete, actionable methods
        /// will be be operationalized.
        /// </summary>
        /// <param name="operation">The query operation to generate an execution context for.</param>
        /// <returns>Task&lt;IGraphFieldExecutableOperation&gt;.</returns>
        public async Task<IExecutableOperation> CreateAsync(IOperationDocumentPart operation)
        {
            Validation.ThrowIfNull(operation, nameof(operation));
            _messages = new GraphMessageCollection();

            var topLevelFields = await this.CreateContextsForFieldSelectionSetAsync(
                operation.GraphType as IObjectGraphType,
                operation.FieldSelectionSet)
                .ConfigureAwait(false);

            var result = new ExecutableOperation(operation);
            foreach (var field in topLevelFields)
                result.FieldContexts.Add(field);

            result.Messages.AddRange(_messages);
            return result;
        }

        /// <summary>
        /// Walks the selection set, extract the necessary resolvers for any field requests and converting input data into
        /// their .NET types for use by said resolvers (when able) variabl data will be deferred to execution time.
        /// </summary>
        /// <param name="sourceGraphType">The source type for which fields requests should be generated.</param>
        /// <param name="fieldsToReturn">The set of fields to return from the source type.</param>
        /// <returns>Task.</returns>
        private async Task<List<IGraphFieldInvocationContext>> CreateContextsForFieldSelectionSetAsync(
            IObjectGraphType sourceGraphType,
            IFieldSelectionSetDocumentPart fieldsToReturn)
        {
            var tasks = new List<Task<IGraphFieldInvocationContext>>();
            if (sourceGraphType != null && fieldsToReturn?.ExecutableFields != null)
            {
                foreach (var fieldPart in fieldsToReturn.ExecutableFields.IncludedOnly)
                {
                    // not all fields in a selection set will target all known source types
                    // like when a fragment is spread into a selection set, the fragment target type will
                    // restrict those fields to a given graph type (or types in the case of a union or interface)
                    if (fieldPart.Field.CanResolveForGraphType(sourceGraphType))
                    {
                        var task = this.CreateFieldContextAsync(sourceGraphType, fieldPart);
                        tasks.Add(task);
                    }
                }
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            var results = new List<IGraphFieldInvocationContext>(tasks.Count);
            foreach (var task in tasks)
            {
                // reawait failed tasks so they can throw in context.
                if (task.IsFaulted)
                    await task.ConfigureAwait(false);

                if (task.Result != null)
                    results.Add(task.Result);
            }

            return results;
        }

        /// <summary>
        /// Creates the appropriate field context from the selection parsed from the user's query document.
        /// </summary>
        /// <param name="sourceGraphType">The graph type from which to extract the data.</param>
        /// <param name="fieldPart">The part of the query document.</param>
        /// <returns>IGraphFieldExecutionContext.</returns>
        private async Task<IGraphFieldInvocationContext> CreateFieldContextAsync(
            IObjectGraphType sourceGraphType,
            IFieldDocumentPart fieldPart)
        {
            var concreteType = _schema.KnownTypes.FindConcreteType(sourceGraphType);

            // the fieldSelection could have been declared and carried in context of an interface
            // translate the field reference to that of the target source type (a resolvable object graph type)
            var targetField = sourceGraphType.Fields[fieldPart.Field.Name];

            var fieldContext = new FieldInvocationContext(
                _schema,
                concreteType,
                fieldPart.Alias.ToString(),
                targetField,
                fieldPart);

            var arguments = this.CreateArgumentList(targetField, fieldPart.Arguments);
            foreach (var argument in arguments)
                fieldContext.Arguments.Add(argument);

            // if the field declares itself as being specifically for a single concrete type
            // enforce that restriction (all user created POCO data types will, most virtual controller based types will not)
            if (sourceGraphType is ITypedSchemaItem typedType)
            {
                if (!(typedType is IInternalSchemaItem))
                    fieldContext.Restrict(typedType.ObjectType);
            }

            if (fieldPart.FieldSelectionSet != null)
            {
                // resolve the child fields for each possible known return type
                // since we don't know what the resultant query may produce at runtime we need to account
                // for all possibilities known to the target schema
                var allKnownTypes = _schema.KnownTypes.ExpandAbstractType(fieldPart.GraphType);

                // maintain the order of completion to ensure that child contexts are added to this context
                // in the order they were declared on the query
                var orderedFieldTasks = new List<Task<List<IGraphFieldInvocationContext>>>();
                foreach (var childGraphType in allKnownTypes)
                {
                    var childrenTask = this.CreateContextsForFieldSelectionSetAsync(childGraphType, fieldPart.FieldSelectionSet);
                    orderedFieldTasks.Add(childrenTask);
                }

                await Task.WhenAll(orderedFieldTasks).ConfigureAwait(false);
                foreach (var task in orderedFieldTasks)
                {
                    // reawait and give the task a chance to throw from the wrapper above
                    if (task.IsFaulted)
                        await task.ConfigureAwait(false);

                    if (task.Result == null)
                        continue;

                    foreach (var childContext in task.Result)
                        fieldContext.ChildContexts.Add(childContext);
                }
            }

            return fieldContext;
        }

        /// <summary>
        /// Inspects the arguments supplied on the user's query document, merges them with those defaulted from the graph field itself
        /// and applies variable data as necessary to generate a single unified set of input arguments to be used when resolving a field of data.
        /// </summary>
        /// <param name="argumentDefinitions">The set of arguments defined on a field.</param>
        /// <param name="querySuppliedArguments">The supplied argument collection parsed from the user query document.</param>
        /// <returns>Task&lt;IInputArgumentCollection&gt;.</returns>
        private IInputArgumentCollection CreateArgumentList(
            IGraphArgumentContainer argumentDefinitions,
            IInputArgumentCollectionDocumentPart querySuppliedArguments)
        {
            var collection = new InputArgumentCollection(argumentDefinitions.Arguments.Count);

            foreach (var argumentDefinition in argumentDefinitions.Arguments)
            {
                var argResult = _argumentGenerator.CreateInputArgument(querySuppliedArguments, argumentDefinition);
                if (argResult.IsValid)
                    collection.Add(argResult.Argument);
                else
                    _messages.Add(argResult.Message);
            }

            return collection;
        }
    }
}