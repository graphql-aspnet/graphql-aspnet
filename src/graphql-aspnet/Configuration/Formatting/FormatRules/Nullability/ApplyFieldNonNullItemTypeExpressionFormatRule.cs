// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Configuration.Formatting.FormatRules
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration.Formatting.FormatRules.Nullability;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// For a given graph field (OBJECT or INTERFACE) that matches the provided predicate
    /// this rule will ensure that the root object of the type expression
    /// cannot be null.
    /// </summary>
    public class ApplyFieldNonNullItemTypeExpressionFormatRule : NullabilityFormatRuleBase, ISchemaItemFormatRule
    {
        private readonly Func<IGraphField, bool> _predicate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyFieldNonNullItemTypeExpressionFormatRule"/> class.
        /// </summary>
        /// <param name="predicate">The predicate function that will be used to match a graph field.</param>
        public ApplyFieldNonNullItemTypeExpressionFormatRule(Func<IGraphField, bool> predicate)
        {
            _predicate = Validation.ThrowIfNullOrReturn(predicate, nameof(predicate));
        }

        /// <inheritdoc />
        public TSchemaItemType Execute<TSchemaItemType>(TSchemaItemType schemaItem)
            where TSchemaItemType : ISchemaItem
        {
            if (schemaItem is IGraphField field && _predicate(field))
            {
                var newTypeExpression = this.ConvertItemReferenceToNonNull(field.TypeExpression);
                schemaItem = (TSchemaItemType)field.Clone(typeExpression: newTypeExpression);
            }

            return schemaItem;
        }
    }
}
