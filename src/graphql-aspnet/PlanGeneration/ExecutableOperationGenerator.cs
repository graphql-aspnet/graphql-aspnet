// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.PlanGeneration.InputArguments;

    /// <summary>
    /// A generator capable of converting a single operation from a query document into an actionable
    /// "execution context" containing the necessary data, steps, resolvers, analyzers etc.  to fulfill a
    /// request made from it.
    /// </summary>
    public class ExecutableOperationGenerator
    {
        private readonly ISchema _schema;
        private IGraphMessageCollection _messages;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutableOperationGenerator" /> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public ExecutableOperationGenerator(ISchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <summary>
        /// Creates an executable operation context from the given query document operation. This method
        /// marries the parsed requirements from a schema to the concrete method and property implementations
        /// in the user's sourcecode. All abstract graph types are materialized and only concrete, actionable methods
        /// will be be operationalized.
        /// </summary>
        /// <param name="operation">The query operation to generate an execution context for.</param>
        /// <returns>Task&lt;IGraphFieldExecutableOperation&gt;.</returns>
        public async Task<IGraphFieldExecutableOperation> Create(QueryOperation operation)
        {
            Validation.ThrowIfNull(operation, nameof(operation));
            _messages = new GraphMessageCollection();

            var topLevelFields = await this.CreateContextsForFieldSelectionSet(
                operation.GraphType,
                operation.FieldSelectionSet).ConfigureAwait(false);

            var result = new GraphFieldExecutableOperation(operation);
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
        private async Task<IEnumerable<IGraphFieldInvocationContext>> CreateContextsForFieldSelectionSet(
            IObjectGraphType sourceGraphType,
            FieldSelectionSet fieldsToReturn)
        {
            var tasks = new List<Task<IGraphFieldInvocationContext>>();
            if (sourceGraphType != null && fieldsToReturn != null && fieldsToReturn.Count > 0)
            {
                foreach (var field in fieldsToReturn)
                {
                    // not all fields in a selection set will target all known source types
                    // like when a fragment is spread into a selection set, the fragment target type will
                    // restrict those fields to a given graph type (or types in the case of a union or interface)
                    if (field.ShouldResolveForGraphType(sourceGraphType))
                    {
                        var task = this.CreateFieldContext(sourceGraphType, field);
                        tasks.Add(task);
                    }
                }
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
            foreach (var task in tasks.Where(x => x.IsFaulted))
            {
                // reawait failed tasks so they can throw in context.
                if (task.IsFaulted)
                    await task.ConfigureAwait(false);
            }

            return tasks.Where(x => x.Result != null).Select(x => x.Result);
        }

        /// <summary>
        /// Creates the appropriate field context from the selection parsed from the user's query document.
        /// </summary>
        /// <param name="sourceGraphType">The graph type from which to extract the data.</param>
        /// <param name="fieldSelection">The requested field of data from teh graph type.</param>
        /// <returns>IGraphFieldExecutionContext.</returns>
        private async Task<IGraphFieldInvocationContext> CreateFieldContext(
            IObjectGraphType sourceGraphType,
            FieldSelection fieldSelection)
        {
            var concreteType = _schema.KnownTypes.FindConcreteType(sourceGraphType);

            // the fieldSelection could have been declared and carried in context of an interface
            // translate the field reference to that of the target source type (a resolvable object graph type)
            var targetField = sourceGraphType.Fields[fieldSelection.Field.Name];

            var fieldContext = new FieldInvocationContext(
                concreteType,
                fieldSelection.Alias.ToString(),
                targetField,
                new SourceOrigin(fieldSelection.Node.Location, fieldSelection.Path));

            var arguments = this.CreateArgumentList(targetField, fieldSelection.Arguments);
            foreach (var argument in arguments)
                fieldContext.Arguments.Add(argument);

            var directives = this.CreateDirectiveContexts(fieldSelection.Directives);
            foreach (var directive in directives)
                fieldContext.Directives.Add(directive);

            // if the field declares itself as being specifically for a single concrete type
            // enforce that restriction (all user created POCO data types will, most virtual controller based types will not)
            if (sourceGraphType is ITypedItem typedType)
                fieldContext.Restrict(typedType.ObjectType);

            if (fieldSelection.FieldSelectionSet != null && fieldSelection.FieldSelectionSet.Count > 0)
            {
                // resolve the child fields for each possible known return type
                // since we don't know what the resultant query may produce at runtime we need to account
                // for all possibilities known to the target schema
                var allKnownTypes = _schema.KnownTypes.ExpandAbstractType(fieldSelection.GraphType);

                // maintain the order of completion to ensure that child contexts are added to this context
                // in the order they were declared on the query
                var orderedFieldTasks = new List<Task<IEnumerable<IGraphFieldInvocationContext>>>();
                foreach (var childGraphType in allKnownTypes)
                {
                    var childrenTask = this.CreateContextsForFieldSelectionSet(childGraphType, fieldSelection.FieldSelectionSet);
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
        /// <param name="argumentContainer">The field selection.</param>
        /// <param name="querySuppliedArguments">The supplied argument collection parsed from the user query document.</param>
        /// <returns>Task&lt;IInputArgumentCollection&gt;.</returns>
        private IInputArgumentCollection CreateArgumentList(
            IGraphFieldArgumentContainer argumentContainer,
            IQueryInputArgumentCollection querySuppliedArguments)
        {
            var collection = new InputArgumentCollection();
            var argGenerator = new ArgumentGenerator(_schema, querySuppliedArguments);

            foreach (var argument in argumentContainer.Arguments)
            {
                var argResult = argGenerator.CreateInputArgument(argument);
                if (argResult.IsValid)
                    collection.Add(new InputArgument(argument, argResult.Argument));
                else
                    _messages.Add(argResult.Message);
            }

            return collection;
        }

        /// <summary>
        /// Convert the referenced directives into executable contexts.
        /// </summary>
        /// <param name="queryDirectives">The directives parsed from the user supplied query document..</param>
        private IEnumerable<IDirectiveInvocationContext> CreateDirectiveContexts(IEnumerable<QueryDirective> queryDirectives)
        {
            var list = new List<IDirectiveInvocationContext>();

            foreach (var directive in queryDirectives)
            {
                var directiveContext = new GraphDirectiveExecutionContext(
                    directive.Location,
                    directive.Directive,
                    directive.Node.Location.AsOrigin());

                // gather arguments
                var arguments = this.CreateArgumentList(directive.Directive, directive.Arguments);
                foreach (var arg in arguments)
                    directiveContext.Arguments.Add(arg);

                list.Add(directiveContext);
            }

            return list;
        }
    }
}