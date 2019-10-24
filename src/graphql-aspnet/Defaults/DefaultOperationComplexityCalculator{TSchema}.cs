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
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.TypeTemplates;

    /// <summary>
    /// A calculator that can determine the estimated complexity for a given operation.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema for this calculator exists.</typeparam>
    public class DefaultOperationComplexityCalculator<TSchema> : IQueryOperationComplexityCalculator<TSchema>
        where TSchema : class, ISchema
    {
        private const float QUERY_WEIGHT = 1;
        private const float MUTATION_WEIGHT = 2;
        private const float SUBSCIRPTION_WEIGHT = 2;

        private const float CONTROLLER_ACTION_WEIGHT = 1.5f;
        private const float OBJECT_METHOD_FIELD_WEIGHT = 1.2f;
        private const float OBJECT_PROPERTY_FIELD_WEIGHT = 1;

        private const float EXECUTION_MODE_PER_SOURCE_WEIGHT = 1.2f;
        private const float EXECUTION_MODE_BATCH_WEIGHT = 1;

        private const float RETURN_TYPE_SINGLE_VALUE_WEIGHT = 0.95f;
        private const float RETURN_TYPE_LIST_WEIGHT = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultOperationComplexityCalculator{TSchema}" /> class.
        /// </summary>
        public DefaultOperationComplexityCalculator()
        {
        }

        /// <summary>
        /// Inspects the operation and determines a final complexity score.
        /// </summary>
        /// <param name="operation">The complexity score for the given operations.</param>
        /// <returns>System.Single.</returns>
        public float Calculate(IGraphFieldExecutableOperation operation)
        {
            float totalWeight = 0;

            foreach (var field in operation.FieldContexts)
                totalWeight += this.CalculateFieldWeight(field);

            switch (operation.OperationType)
            {
                case GraphCollection.Query:
                    totalWeight *= QUERY_WEIGHT;
                    break;

                case GraphCollection.Mutation:
                    totalWeight *= MUTATION_WEIGHT;
                    break;

                case GraphCollection.Subscription:
                    totalWeight *= SUBSCIRPTION_WEIGHT;
                    break;
            }

            return totalWeight;
        }

        private float CalculateFieldWeight(IGraphFieldInvocationContext field)
        {
            var fieldWeight = field.Field.Complexity ?? 1;

            // Weight of how the field will be processed
            switch (field.Field.Mode)
            {
                case FieldResolutionMode.PerSourceItem:
                    fieldWeight *= EXECUTION_MODE_PER_SOURCE_WEIGHT;
                    break;

                case FieldResolutionMode.Batch:
                    fieldWeight *= EXECUTION_MODE_BATCH_WEIGHT;
                    break;
            }

            // what of how the field data is sourced
            switch (field.Field.FieldSource)
            {
                case GraphFieldTemplateSource.Action:
                    fieldWeight *= CONTROLLER_ACTION_WEIGHT;
                    break;

                case GraphFieldTemplateSource.Method:
                    fieldWeight *= OBJECT_METHOD_FIELD_WEIGHT;
                    break;

                case GraphFieldTemplateSource.Property:
                    fieldWeight *= OBJECT_PROPERTY_FIELD_WEIGHT;
                    break;
            }

            // the total weight of the child fields (per source item)
            float childFieldsWeight = 1;
            foreach (var childField in field.ChildContexts)
            {
                childFieldsWeight += this.CalculateFieldWeight(childField);
            }

            // the combined weight of the child items against how those children
            // are executed (once for a single object orm perhaps many times for a list?)
            if (field.Field.TypeExpression.IsListOfItems)
                childFieldsWeight *= RETURN_TYPE_LIST_WEIGHT;
            else
                childFieldsWeight *= RETURN_TYPE_SINGLE_VALUE_WEIGHT;

            // the combined weight
            return childFieldsWeight * fieldWeight;
        }
    }
}