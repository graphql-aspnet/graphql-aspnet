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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.QueryPlans;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A plan generator capable of converting an operation document part into an
    /// executable operatiopn by a the query pipeline.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this plan generator is registered for.</typeparam>
    public class DefaultQueryExecutionPlanGenerator<TSchema> : IQueryExecutionPlanGenerator<TSchema>
        where TSchema : class, ISchema
    {
        private readonly ISchema _schema;
        private readonly IQueryOperationComplexityCalculator<TSchema> _complexityCalculator;
        private readonly IQueryOperationDepthCalculator<TSchema> _depthCalculator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultQueryExecutionPlanGenerator{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="depthCalculator">The depth calculator for this plan generator to use when
        /// detereming node depths in generated query documents.</param>
        /// <param name="complexityCalculator">The complexity calculator for this plan generator to use
        /// when computing complexity scores for query documents.</param>
        public DefaultQueryExecutionPlanGenerator(
            TSchema schema,
            IQueryOperationDepthCalculator<TSchema> depthCalculator,
            IQueryOperationComplexityCalculator<TSchema> complexityCalculator)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _complexityCalculator = Validation.ThrowIfNullOrReturn(complexityCalculator, nameof(complexityCalculator));
            _depthCalculator = Validation.ThrowIfNullOrReturn(depthCalculator, nameof(depthCalculator));
        }

        /// <inheritdoc />
        public async Task<IQueryExecutionPlan> CreatePlanAsync(IOperationDocumentPart operation)
        {
            Validation.ThrowIfNull(operation, nameof(operation));

            var queryPlan = this.CreatePlanInstance();

            // Validate that the document meets the depth requirements for plan generation
            this.InspectSyntaxDepth(queryPlan, operation);
            if (!queryPlan.IsValid)
                return queryPlan;

            var generator = new ExecutableOperationGenerator(_schema);

            var executableOperation = await generator.CreateAsync(operation).ConfigureAwait(false);
            queryPlan.Operation = executableOperation;
            queryPlan.Messages.AddRange(executableOperation.Messages);

            // estimate the complexity for any successfully parsed operations
            if (executableOperation.Messages.IsSucessful)
            {
                var complexity = _complexityCalculator.Calculate(executableOperation);
                if (complexity > queryPlan.EstimatedComplexity)
                    queryPlan.EstimatedComplexity = complexity;
            }

            this.InspectQueryPlanComplexity(queryPlan);
            return queryPlan;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IQueryExecutionPlan"/>
        /// this generator manages. Overload this method in a child class to inject
        /// your own plan type.
        /// </summary>
        /// <returns>IGraphQueryPlan.</returns>
        protected virtual IQueryExecutionPlan CreatePlanInstance()
        {
            return new QueryExecutionPlan<TSchema>();
        }

        /// <summary>
        /// Inspects ano operation against the required metric values for the target schema and should a violation occur a
        /// an error message is added to the query plan.
        /// </summary>
        /// <param name="queryPlan">The query plan being generated.</param>
        /// <param name="operation">The operation to be included.</param>
        protected virtual void InspectSyntaxDepth(IQueryExecutionPlan queryPlan, IOperationDocumentPart operation)
        {
            var maxAllowedDepth = _schema.Configuration?.ExecutionOptions?.MaxQueryDepth;
            if (maxAllowedDepth == null)
                return;

            var computedDepth = _depthCalculator.Calculate(operation);

            if (maxAllowedDepth.HasValue && computedDepth > maxAllowedDepth.Value)
            {
                var operationName = operation.Name;
                if (string.IsNullOrWhiteSpace(operationName))
                    operationName = "~anonymous~";

                queryPlan.Messages.Critical(
                    $"The query operation {operationName} has a max field depth of {computedDepth} but " +
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
        protected virtual void InspectQueryPlanComplexity(IQueryExecutionPlan plan)
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