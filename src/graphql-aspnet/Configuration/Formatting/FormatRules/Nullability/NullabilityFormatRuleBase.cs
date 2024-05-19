// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Configuration.Formatting.FormatRules.Nullability
{
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// A common class for nullability format rules to centralize common logic
    /// </summary>
    public abstract class NullabilityFormatRuleBase
    {
        /// <summary>
        /// For the given type expression converts all lists (nested or otherwise) to be
        /// non-nullabe.
        /// </summary>
        /// <param name="typeExpression">The type expression.</param>
        /// <returns>GraphTypeExpression.</returns>
        protected virtual GraphTypeExpression ConvertAllListsToNonNull(GraphTypeExpression typeExpression)
        {
            if (typeExpression.IsFixed)
                return typeExpression;

            typeExpression = typeExpression.Clone(GraphTypeExpressionNullabilityStrategies.NonNullLists);
            return typeExpression;
        }

        /// <summary>
        /// For the given type expression converts the core item reference to be non-nullable.
        /// </summary>
        /// <param name="typeExpression">The type expression.</param>
        /// <returns>GraphTypeExpression.</returns>
        protected virtual GraphTypeExpression ConvertItemReferenceToNonNull(GraphTypeExpression typeExpression)
        {
            if (!typeExpression.IsFixed)
                typeExpression = typeExpression.Clone(GraphTypeExpressionNullabilityStrategies.NonNullType);

            return typeExpression;
        }
    }
}
