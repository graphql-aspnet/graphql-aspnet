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
    /// For a field argument that matches the provided predicate
    /// this rule will ensure that the root object of the type expression cannot be null.
    /// </summary>
    public class ApplyArgumentNonNullItemTypeExpressionFormatRule : NullabilityFormatRuleBase, ISchemaItemFormatRule
    {
        private readonly Func<IGraphArgument, bool> _predicate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyArgumentNonNullItemTypeExpressionFormatRule"/> class.
        /// </summary>
        /// <param name="predicate">The predicate function that will be used to match an argument.</param>
        public ApplyArgumentNonNullItemTypeExpressionFormatRule(Func<IGraphArgument, bool> predicate)
        {
            _predicate = Validation.ThrowIfNullOrReturn(predicate, nameof(predicate));
        }

        /// <inheritdoc />
        public TSchemaItemType Execute<TSchemaItemType>(TSchemaItemType schemaItem)
            where TSchemaItemType : ISchemaItem
        {
            if (schemaItem is IGraphArgument argument && _predicate(argument))
            {
                var newTypeExpression = this.ConvertItemReferenceToNonNull(argument.TypeExpression);
                argument = argument.Clone(typeExpression: newTypeExpression);

                if (!newTypeExpression.IsNullable && argument.HasDefaultValue && argument.DefaultValue is null)
                {
                    // when the input argument, as a whole, becomes non-nullable and has a default value of null
                    // the field must become "required" without a default value because of the rules
                    // of the schema
                    argument = argument.Clone(defaultValueOptions: DefaultValueCloneOptions.MakeRequired);
                }

                schemaItem = (TSchemaItemType)argument;
            }

            return schemaItem;
        }
    }
}
