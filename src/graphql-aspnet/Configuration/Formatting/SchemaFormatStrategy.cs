// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration.Formatting
{
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A strategy, employed by a schema, to make programatic alterations
    /// to schema items as they are being constructed.
    /// </summary>
    public class SchemaFormatStrategy : ISchemaFormatStrategy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaFormatStrategy"/> class.
        /// </summary>
        /// <param name="nameFormatStrategy">A single format strategy to use for all naming formats.</param>
        public SchemaFormatStrategy(SchemaItemNameFormatOptions nameFormatStrategy)
            : this(
                  TypeExpressionNullabilityFormatRules.Default,
                  nameFormatStrategy,
                  nameFormatStrategy,
                  nameFormatStrategy)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaFormatStrategy" /> class.
        /// </summary>
        /// <param name="nullabilityStrategy">The strategy used to augment nullability
        /// checks on type expressions for fields and arguments.</param>
        /// <param name="typeNameStrategy">The format strategy to use for graph type names.</param>
        /// <param name="fieldNameStrategy">The format strategy to use for field names.</param>
        /// <param name="enumValueStrategy">The format strategy to use for enum values.</param>
        public SchemaFormatStrategy(
           TypeExpressionNullabilityFormatRules nullabilityStrategy = TypeExpressionNullabilityFormatRules.Default,
           SchemaItemNameFormatOptions typeNameStrategy = SchemaItemNameFormatOptions.ProperCase,
           SchemaItemNameFormatOptions fieldNameStrategy = SchemaItemNameFormatOptions.CamelCase,
           SchemaItemNameFormatOptions enumValueStrategy = SchemaItemNameFormatOptions.UpperCase)
        {
            this.NullabilityStrategy = nullabilityStrategy;
            this.GraphTypeNameStrategy = typeNameStrategy;
            this.FieldNameStrategy = fieldNameStrategy;
            this.EnumValueStrategy = enumValueStrategy;
        }

        /// <inheritdoc />
        public T ApplySchemaItemRules<T>(ISchemaConfiguration configuration, T schemaItem)
            where T : ISchemaItem
        {
            switch (schemaItem)
            {
                case IDirective directive:
                    return (T)this.FormatDirective(configuration, directive);

                case IUnionGraphType unionGraphType:
                    return (T)this.FormatUnionType(configuration, unionGraphType);

                case IGraphType graphType:
                    return (T)this.FormatGraphType(configuration, graphType);

                case IGraphField graphField:
                    return (T)this.FormatGraphField(configuration, graphField);

                case IEnumValue enumValue:
                    return (T)this.FormatEnumValue(configuration, enumValue);

                case IInputGraphField inputGraphField:
                    return (T)this.FormatInputGraphField(configuration, inputGraphField);

                case IGraphArgument argument:
                    return (T)this.FormatArgument(configuration, argument);
            }

            return schemaItem;
        }

        /// <summary>
        /// Formats the directive name to be how it will appear in a query, excluding the '@'
        /// symbol.
        /// </summary>
        /// <param name="name">The directive name to format.</param>
        /// <returns>System.String.</returns>
        protected virtual string FormatDirectiveName(string name)
        {
            return this.FormatName(name, this.FieldNameStrategy);
        }

        /// <summary>
        /// Formats a field or argument name according to the rules declared on this strategy.
        /// </summary>
        /// <param name="name">The field or argument name to format.</param>
        /// <returns>System.String.</returns>
        protected virtual string FormatFieldName(string name)
        {
            return this.FormatName(name, this.FieldNameStrategy);
        }

        /// <summary>
        /// Formats the enum value name according to the rules declared on this strategy.
        /// </summary>
        /// <param name="name">The enum value name to format.</param>
        /// <returns>System.String.</returns>
        protected virtual string FormatEnumValueName(string name)
        {
            return this.FormatName(name, this.EnumValueStrategy);
        }

        /// <summary>
        /// Formats the graph type name according to the rules declared on this strategy.
        /// </summary>
        /// <param name="name">The type name to format.</param>
        /// <returns>System.String.</returns>
        protected virtual string FormatGraphTypeName(string name)
        {
            if (!GlobalTypes.CanBeRenamed(name))
                return name;

            return this.FormatName(name, this.GraphTypeNameStrategy);
        }

        /// <summary>
        /// Applies the schema specific formats identified by <paramref name="configuration"/>
        /// to the target <paramref name="unionGraphType"/>.
        /// </summary>
        /// <param name="configuration">The configuration to read formatting settings
        /// from.</param>
        /// <param name="unionGraphType">The union graph type to update.</param>
        /// <returns>IDirective.</returns>
        protected virtual IUnionGraphType FormatUnionType(ISchemaConfiguration configuration, IUnionGraphType unionGraphType)
        {
            unionGraphType.Clone(possibleGraphTypeNameFormatter: this.FormatGraphTypeName);
            return (IUnionGraphType)this.FormatGraphType(configuration, unionGraphType);
        }

        /// <summary>
        /// Applies the schema specific formats identified by <paramref name="configuration"/>
        /// to the target <paramref name="enumValue"/>.
        /// </summary>
        /// <param name="configuration">The configuration to read formatting settings
        /// from.</param>
        /// <param name="enumValue">The enum value to update.</param>
        /// <returns>IDirective.</returns>
        protected virtual IEnumValue FormatEnumValue(ISchemaConfiguration configuration, IEnumValue enumValue)
        {
            var formattedName = this.FormatEnumValueName(enumValue.Name);
            return enumValue.Clone(valueName: formattedName);
        }

        /// <summary>
        /// Applies the schema specific formats identified by <paramref name="configuration"/>
        /// to the target <paramref name="directive"/>.
        /// </summary>
        /// <param name="configuration">The configuration to read formatting settings
        /// from.</param>
        /// <param name="directive">The directive to update.</param>
        /// <returns>IDirective.</returns>
        protected virtual IDirective FormatDirective(ISchemaConfiguration configuration, IDirective directive)
        {
            // directives are referenced as fields
            var formattedName = this.FormatFieldName(directive.Name);
            return (IDirective)directive.Clone(formattedName);
        }

        /// <summary>
        /// Applies the schema specific formats identified by <paramref name="configuration" />
        /// to the target <paramref name="graphType" />.
        /// </summary>
        /// <param name="configuration">The configuration to read formatting settings
        /// from.</param>
        /// <param name="graphType">The graph type to update.</param>
        /// <returns>IGraphType.</returns>
        protected virtual IGraphType FormatGraphType(ISchemaConfiguration configuration, IGraphType graphType)
        {
            // ensure all path segments of the virtual type are
            // named according to the rules of this schema
            if (graphType is VirtualObjectGraphType virtualType)
            {
                var newName = VirtualObjectGraphType.MakeSafeTypeNameFromItemPath(
                    virtualType.ItemPathTemplate,
                    this.FormatGraphTypeName);

                return virtualType.Clone(newName);
            }

            var formattedName = this.FormatGraphTypeName(graphType.Name);
            return graphType.Clone(formattedName);
        }

        /// <summary>
        /// Applies the schema specific formats identified by <paramref name="configuration"/>
        /// to the target <paramref name="inputGraphField"/>.
        /// </summary>
        /// <param name="configuration">The configuration to read formatting settings
        /// from.</param>
        /// <param name="inputGraphField">The field to update.</param>
        /// <returns>IInputGraphField.</returns>
        protected virtual IInputGraphField FormatInputGraphField(ISchemaConfiguration configuration, IInputGraphField inputGraphField)
        {
            if (!inputGraphField.TypeExpression.IsFixed)
                inputGraphField = this.ApplyTypeExpressionNullabilityRules(inputGraphField);

            var formattedName = this.FormatFieldName(inputGraphField.Name);
            var typeExpression = inputGraphField.TypeExpression;
            typeExpression = typeExpression.Clone(this.FormatGraphTypeName(typeExpression.TypeName));

            return inputGraphField.Clone(fieldName: formattedName, typeExpression: typeExpression);
        }

        /// <summary>
        /// Applies the schema specific formats identified by <paramref name="configuration"/>
        /// to the target <paramref name="graphField"/>.
        /// </summary>
        /// <param name="configuration">The configuration to read formatting settings
        /// from.</param>
        /// <param name="graphField">The field to update.</param>
        /// <returns>IGraphField.</returns>
        protected virtual IGraphField FormatGraphField(ISchemaConfiguration configuration, IGraphField graphField)
        {
            if (!graphField.TypeExpression.IsFixed)
                graphField = this.ApplyTypeExpressionNullabilityRules(graphField);

            var formattedName = this.FormatFieldName(graphField.Name);
            var typeExpression = graphField.TypeExpression;
            typeExpression = typeExpression.Clone(this.FormatGraphTypeName(typeExpression.TypeName));

            return graphField.Clone(fieldName: formattedName, typeExpression: typeExpression);
        }

        /// <summary>
        /// Applies the schema specific formats identified by <paramref name="configuration"/>
        /// to the target <paramref name="argument"/>.
        /// </summary>
        /// <param name="configuration">The configuration to read formatting settings
        /// from.</param>
        /// <param name="argument">The argument to update.</param>
        /// <returns>IGraphField.</returns>
        protected virtual IGraphArgument FormatArgument(ISchemaConfiguration configuration, IGraphArgument argument)
        {
            if (!argument.TypeExpression.IsFixed)
                argument = this.ApplyTypeExpressionNullabilityRules(argument);

            var formattedName = this.FormatFieldName(argument.Name);
            var typeExpression = argument.TypeExpression;
            typeExpression = typeExpression.Clone(this.FormatGraphTypeName(typeExpression.TypeName));

            return argument.Clone(argumentName: formattedName, typeExpression: typeExpression);
        }

        /// <summary>
        /// For the given field applies an appropriate nullability strategy
        /// according to the rules of this instance and returns a new instance
        /// of the field.
        /// </summary>
        /// <param name="graphField">The graph field to update.</param>
        /// <returns>IGraphField.</returns>
        protected virtual IGraphField ApplyTypeExpressionNullabilityRules(IGraphField graphField)
        {
            GraphTypeExpressionNullabilityStrategies strat = GraphTypeExpressionNullabilityStrategies.None;
            var shouldBeNonNullType = this.NullabilityStrategy
                        .HasFlag(TypeExpressionNullabilityFormatRules.NonNullIntermediateTypes)
                && graphField.IsVirtual;

            shouldBeNonNullType = shouldBeNonNullType ||
                (this.NullabilityStrategy
                        .HasFlag(TypeExpressionNullabilityFormatRules.NonNullOutputStrings)
                && graphField.ObjectType == typeof(string));

            shouldBeNonNullType = shouldBeNonNullType ||
                (this.NullabilityStrategy
                        .HasFlag(TypeExpressionNullabilityFormatRules.NonNullReferenceTypes)
                && !graphField.IsVirtual
                && graphField.ObjectType != typeof(string)
                && !graphField.ObjectType.IsValueType);

            if (shouldBeNonNullType)
                strat = strat | GraphTypeExpressionNullabilityStrategies.NonNullType;

            if (this.NullabilityStrategy.HasFlag(TypeExpressionNullabilityFormatRules.NonNullLists))
                strat = strat | GraphTypeExpressionNullabilityStrategies.NonNullLists;

            var newTypeExpression = graphField.TypeExpression.Clone(strat);
            return graphField.Clone(typeExpression: newTypeExpression);
        }

        /// <summary>
        /// For the given field, applies an appropriate nullability strategy
        /// according to the rules of this instance and returns a new instance
        /// of the field.
        /// </summary>
        /// <param name="inputGraphField">The graph field to update.</param>
        /// <returns>IGraphField.</returns>
        protected virtual IInputGraphField ApplyTypeExpressionNullabilityRules(IInputGraphField inputGraphField)
        {
            GraphTypeExpressionNullabilityStrategies strat = GraphTypeExpressionNullabilityStrategies.None;

            var shouldTypeBeNonNull = this.NullabilityStrategy
                        .HasFlag(TypeExpressionNullabilityFormatRules.NonNullInputStrings)
                && inputGraphField.ObjectType == typeof(string);

            shouldTypeBeNonNull = shouldTypeBeNonNull ||
                (this.NullabilityStrategy.HasFlag(TypeExpressionNullabilityFormatRules.NonNullReferenceTypes)
                && inputGraphField.ObjectType != typeof(string)
                && !inputGraphField.ObjectType.IsValueType);

            if (shouldTypeBeNonNull)
                strat = strat | GraphTypeExpressionNullabilityStrategies.NonNullType;

            if (this.NullabilityStrategy.HasFlag(TypeExpressionNullabilityFormatRules.NonNullLists))
                strat = strat | GraphTypeExpressionNullabilityStrategies.NonNullLists;

            var newTypeExpression = inputGraphField.TypeExpression.Clone(strat);
            inputGraphField = inputGraphField.Clone(typeExpression: newTypeExpression);

            if (!newTypeExpression.IsNullable && inputGraphField.HasDefaultValue && inputGraphField.DefaultValue is null)
            {
                // when the field, as a whole, becomes non-nullable and has a default value of null
                // the field must become "required" without a default value because of the rules
                // of the schema
                inputGraphField = inputGraphField.Clone(defaultValueOptions: DefaultValueCloneOptions.MakeRequired);
            }

            return inputGraphField;
        }

        /// <summary>
        /// For the given argument applies an appropriate nullability strategy
        /// according to the rules of this instance and returns a new instance
        /// of the argument.
        /// </summary>
        /// <param name="argument">The argument to update.</param>
        /// <returns>IGraphField.</returns>
        protected virtual IGraphArgument ApplyTypeExpressionNullabilityRules(IGraphArgument argument)
        {
            GraphTypeExpressionNullabilityStrategies strat = GraphTypeExpressionNullabilityStrategies.None;

            var shouldBeNonNullType = this.NullabilityStrategy
                        .HasFlag(TypeExpressionNullabilityFormatRules.NonNullInputStrings)
                && argument.ObjectType == typeof(string);

            shouldBeNonNullType = shouldBeNonNullType ||
                (this.NullabilityStrategy
                        .HasFlag(TypeExpressionNullabilityFormatRules.NonNullReferenceTypes)
                && argument.ObjectType != typeof(string)
                && !argument.ObjectType.IsValueType);

            if (shouldBeNonNullType)
                strat = strat | GraphTypeExpressionNullabilityStrategies.NonNullType;

            if (this.NullabilityStrategy.HasFlag(TypeExpressionNullabilityFormatRules.NonNullLists))
                strat = strat | GraphTypeExpressionNullabilityStrategies.NonNullLists;

            var newTypeExpression = argument.TypeExpression.Clone(strat);
            argument = argument.Clone(typeExpression: newTypeExpression);

            if (!newTypeExpression.IsNullable && argument.HasDefaultValue && argument.DefaultValue is null)
            {
                // when the input argument, as a whole, becomes non-nullable and has a default value of null
                // the field must become "required" without a default value because of the rules
                // of the schema
                argument = argument.Clone(defaultValueOptions: DefaultValueCloneOptions.MakeRequired);
            }

            return argument;
        }

        /// <summary>
        /// Formats or reformats the name according to the rules of this formatter.
        /// </summary>
        /// <param name="name">The name value being formatted.</param>
        /// <param name="strategy">The selected strategy to format with.</param>
        /// <returns>System.String.</returns>
        protected virtual string FormatName(string name, SchemaItemNameFormatOptions strategy)
        {
            if (name == null)
                return null;

            switch (strategy)
            {
                case SchemaItemNameFormatOptions.ProperCase:
                    return name.FirstCharacterToUpperInvariant();

                case SchemaItemNameFormatOptions.CamelCase:
                    return name.FirstCharacterToLowerInvariant();

                case SchemaItemNameFormatOptions.UpperCase:
                    return name.ToUpperInvariant();

                case SchemaItemNameFormatOptions.LowerCase:
                    return name.ToLowerInvariant();

                // ReSharper disable once RedundantCaseLabel
                case SchemaItemNameFormatOptions.NoChanges:
                default:
                    return name;
            }
        }

        /// <summary>
        /// Gets or sets the bitwise flags that make up the nullability strategy
        /// to apply to field and argument type expressions.
        /// </summary>
        /// <value>The nullability strategy.</value>
        public TypeExpressionNullabilityFormatRules NullabilityStrategy { get; set; }

        /// <summary>
        /// Gets or sets the name format strategy to use for graph type names.
        /// </summary>
        /// <value>The type name strategy.</value>
        public SchemaItemNameFormatOptions GraphTypeNameStrategy { get; set; }

        /// <summary>
        /// Gets or sets the name format strategy to use for field names.
        /// </summary>
        /// <value>The field strategy.</value>
        public SchemaItemNameFormatOptions FieldNameStrategy { get; set; }

        /// <summary>
        /// Gets or sets the name format strategy to use for enum values.
        /// </summary>
        /// <remarks>
        /// The parent enum graph type, which owns an enum value, is named via
        /// the <see cref="GraphTypeNameStrategy"/>.
        /// </remarks>
        /// <value>The enum value name strategy.</value>
        public SchemaItemNameFormatOptions EnumValueStrategy { get; set; }
    }
}