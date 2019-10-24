// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Defaults
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.PlanGeneration;

    /// <summary>
    /// A plan generator capable of converting a syntax tree into an actionable
    /// query plan executable by a the query pipeline.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this plan generator is registered for.</typeparam>
    public class DefaultGraphQueryPlanGenerator<TSchema> : IGraphQueryPlanGenerator<TSchema>
        where TSchema : class, ISchema
    {
        private readonly ISchema _schema;
        private readonly IQueryOperationComplexityCalculator<TSchema> _complexityCalculator;
        private readonly IGraphQueryDocumentGenerator<TSchema> _documentGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGraphQueryPlanGenerator{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="documentGenerator">The document generator that will generate query documents, a precursor
        /// to a query plan.</param>
        /// <param name="complexityCalculator">The complexity calculator for this plan generator to use
        /// when computing complexity scores.</param>
        public DefaultGraphQueryPlanGenerator(
            TSchema schema,
            IGraphQueryDocumentGenerator<TSchema> documentGenerator,
            IQueryOperationComplexityCalculator<TSchema> complexityCalculator)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _complexityCalculator = Validation.ThrowIfNullOrReturn(complexityCalculator, nameof(complexityCalculator));
            _documentGenerator = Validation.ThrowIfNullOrReturn(documentGenerator, nameof(documentGenerator));
        }

        /// <summary>
        /// Creates a qualified exeuction plan that can be executed to fulfill a user's request.
        /// </summary>
        /// <param name="syntaxTree">The syntax tree that was lexed from the original source supplied by a user.</param>
        /// <returns>Task&lt;IGraphQueryPlan&gt;.</returns>
        public async Task<IGraphQueryPlan> CreatePlan(ISyntaxTree syntaxTree)
        {
            Validation.ThrowIfNull(syntaxTree, nameof(syntaxTree));
            var queryPlan = this.CreatePlanInstance();

            // ------------------------------------------
            // Step 1:  Parse the document.
            // ------------------------------------------
            // Validate that the lexed syntax tree is internally consistant and
            // is valid in context to the schema it is to be processed against
            // at the same time build up the query document in anticipation that it will be correct
            // as in production this should mostly be the case. Should it fail return the messages
            // on an empty query plan.
            // ------------------------------------------
            var document = _documentGenerator.CreateDocument(syntaxTree);
            this.InspectSyntaxDepth(document);
            queryPlan.MaxDepth = document.MaxDepth;
            queryPlan.Messages.AddRange(document.Messages);
            if (!queryPlan.IsValid)
                return queryPlan;

            // ------------------------------------------
            // Step 2:  Plan Construction
            // ------------------------------------------
            // The document is garunteed to be syntactically correct and, barring anything user related (variable data, custom code etc.),
            // will resolve to produce a result.  Using the correct operation (or the anon operation), extract the resolvers for all possible concrete
            // types needed to fulfill the user's request and generate a query plan that can be executed to fulfill the request.
            // ------------------------------------------
            var generator = new ExecutableOperationGenerator(_schema);
            foreach (var operation in document.Operations.Values)
            {
                var executableOperation = await generator.Create(operation).ConfigureAwait(false);
                queryPlan.AddOperation(executableOperation);

                // estimate the complexity for any successfully parsed operations
                if (executableOperation.Messages.IsSucessful)
                {
                    var complexity = _complexityCalculator.Calculate(executableOperation);

                    if (complexity > queryPlan.EstimatedComplexity)
                        queryPlan.EstimatedComplexity = complexity;
                }
            }

            this.InspectQueryPlanComplexity(queryPlan);
            return queryPlan;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IGraphQueryPlan"/>
        /// this generator manages. Overload this method in a child class to inject
        /// your own plan type.
        /// </summary>
        /// <returns>IGraphQueryPlan.</returns>
        protected virtual IGraphQueryPlan CreatePlanInstance()
        {
             return new GraphQueryExecutionPlan<TSchema>();
        }

        /// <summary>
        /// Inspects the syntax tree against the required metric values for the target schema and should a violation occur a
        /// <see cref="GraphQLSyntaxException" /> is thrown aborting the query.
        /// </summary>
        /// <param name="document">The document to inspect.</param>
        protected void InspectSyntaxDepth(IGraphQueryDocument document)
        {
            var maxAllowedDepth = _schema.Configuration?.ExecutionOptions?.MaxQueryDepth;
            if (maxAllowedDepth.HasValue && document.MaxDepth > maxAllowedDepth.Value)
            {
                document.Messages.Critical(
                    $"The query has a max field depth of {document.MaxDepth} but " +
                    $"this schema has been configured to only accept queries with a max depth of {maxAllowedDepth.Value}. " +
                    "Adjust your query and try again.",
                    Constants.ErrorCodes.REQUEST_ABORTED);
            }
        }

        /// <summary>
        /// Inspects the query plan's estimated execution metrics against the configured values for the target schema and should a violation
        /// occur, a message is recorded to the plan and it is abandoned.
        /// </summary>
        /// <param name="plan">The plan.</param>
        protected void InspectQueryPlanComplexity(IGraphQueryPlan plan)
        {
            var maxComplexity = _schema.Configuration?.ExecutionOptions?.MaxQueryComplexity;
            if (maxComplexity.HasValue && plan.EstimatedComplexity > maxComplexity.Value)
            {
                plan.Messages.Critical(
                    $"The generated query plan has an estimated complexity score of {plan.EstimatedComplexity} but this schema has been configured to only accept " +
                    $"queries with a maximum estimated complexity of {maxComplexity.Value}. A high complexity value " +
                    "usually indicates a large number of data store operations and compounding field executions such as fields that yield lists with child fields that also yield lists. " +
                    "Adjust your query and try again.",
                    Constants.ErrorCodes.REQUEST_ABORTED);
            }
        }
    }
}