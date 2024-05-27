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
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// For a given input field (on an INPUT_OBJECT) will force all encountered lists
    /// and nested lists on the type expression to be non-null.
    /// </summary>
    internal class ApplyInputFieldNonNullListTypeExpressionFormatRule : NullabilityFormatRuleBase, ISchemaItemFormatRule
    {
        private readonly Func<IInputGraphField, bool> _predicate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyInputFieldNonNullListTypeExpressionFormatRule"/> class.
        /// </summary>
        /// <param name="predicate">The predicate function that will be used to match a graph field.</param>
        public ApplyInputFieldNonNullListTypeExpressionFormatRule(Func<IInputGraphField, bool> predicate)
        {
            _predicate = Validation.ThrowIfNullOrReturn(predicate, nameof(predicate));
        }

        /// <inheritdoc />
        public TSchemaItemType Execute<TSchemaItemType>(TSchemaItemType schemaItem)
            where TSchemaItemType : ISchemaItem
        {
            if (schemaItem is IInputGraphField field && _predicate(field))
            {
                var newTypeExpression = this.ConvertAllListsToNonNull(field.TypeExpression);
                field = field.Clone(typeExpression: newTypeExpression);

                if (!newTypeExpression.IsNullable && field.HasDefaultValue && field.DefaultValue is null)
                {
                    // when the field, as a whole, becomes non-nullable and has a default value of null
                    // the field must become "required" without a default value because of the rules
                    // of the schema
                    field = field.Clone(defaultValueOptions: DefaultValueCloneOptions.MakeRequired);
                }

                schemaItem = (TSchemaItemType)field;
            }

            return schemaItem;
        }
    }
}
