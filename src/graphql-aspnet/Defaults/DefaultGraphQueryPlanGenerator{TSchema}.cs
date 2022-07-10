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
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.PlanGeneration;

    /// <summary>
    /// A plan generator capable of converting an operation document part into an
    /// executable operatiopn by a the query pipeline.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this plan generator is registered for.</typeparam>
    public class DefaultGraphQueryPlanGenerator<TSchema> : IGraphQueryPlanGenerator<TSchema>
        where TSchema : class, ISchema
    {
        private readonly ISchema _schema;
        private readonly IQueryOperationComplexityCalculator<TSchema> _complexityCalculator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGraphQueryPlanGenerator{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="complexityCalculator">The complexity calculator for this plan generator to use
        /// when computing complexity scores.</param>
        public DefaultGraphQueryPlanGenerator(
            TSchema schema,
            IQueryOperationComplexityCalculator<TSchema> complexityCalculator)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _complexityCalculator = Validation.ThrowIfNullOrReturn(complexityCalculator, nameof(complexityCalculator));
        }

        /// <inheritdoc />
        public async Task<IGraphQueryPlan> CreatePlan(
            IOperationDocumentPart operation)
        {
            Validation.ThrowIfNull(operation, nameof(operation));

            var queryPlan = this.CreatePlanInstance();

            // Validate that the document meets the depth requirements for plan generation
            this.InspectSyntaxDepth(queryPlan, operation);
            queryPlan.MaxDepth = operation.MaxDepth;
            if (!queryPlan.IsValid)
                return queryPlan;

            var generator = new ExecutableOperationGenerator(_schema);

            var executableOperation = await generator.Create(operation).ConfigureAwait(false);
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
        /// <param name="queryPlan">The query plan being generated.</param>
        /// <param name="operation">The operation to be included.</param>
        protected virtual void InspectSyntaxDepth(
            IGraphQueryPlan queryPlan,
            IOperationDocumentPart operation)
        {
            var maxAllowedDepth = _schema.Configuration?.ExecutionOptions?.MaxQueryDepth;
            if (maxAllowedDepth.HasValue && operation.MaxDepth > maxAllowedDepth.Value)
            {
                queryPlan.Messages.Critical(
                    $"The query operation has a max field depth of {operation.MaxDepth} but " +
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
        protected virtual void InspectQueryPlanComplexity(IGraphQueryPlan plan)
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