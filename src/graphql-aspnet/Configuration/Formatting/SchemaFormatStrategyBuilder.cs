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
    using GraphQL.AspNet.Interfaces.Configuration;

    /// <summary>
    /// A builder class to formulate a <see cref="SchemaFormatStrategy"/>
    /// with various options.
    /// </summary>
    public class SchemaFormatStrategyBuilder
    {
        /// <summary>
        /// Starts a new strategy builder instance.
        /// </summary>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public static SchemaFormatStrategyBuilder Create()
        {
            return new SchemaFormatStrategyBuilder();
        }

        private readonly SchemaFormatStrategy _format;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaFormatStrategyBuilder"/> class.
        /// </summary>
        public SchemaFormatStrategyBuilder()
        {
            _format = new SchemaFormatStrategy();
        }

        /// <summary>
        /// Sets a rule such that all string scalars are marked as "not null" by default
        /// when accepted as an argument to a field or returned by a field. This can be overriden on a per field or
        /// per argument basis.
        /// </summary>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public SchemaFormatStrategyBuilder WithRequiredStrings()
        {
            return this.WithRequiredInputStrings()
                        .WithRequiredOutputStrings();
        }

        /// <summary>
        /// Sets a rule such that all string scalar on inbound data items (e.g. field arguments and INPUT_OBJECT types)
        /// will be declared as "not null" by default for all generated graph types.This can be overriden on a per field or
        /// per argument basis.
        /// </summary>
        /// <returns>GraphSchemaFormatStrategyBuilder.</returns>
        public SchemaFormatStrategyBuilder WithRequiredInputStrings()
        {
            if (!_format.NullabilityStrategy.HasFlag(TypeExpressionNullabilityFormatRules.NonNullInputStrings))
                _format.NullabilityStrategy = _format.NullabilityStrategy | TypeExpressionNullabilityFormatRules.NonNullInputStrings;

            return this;
        }

        /// <summary>
        /// Sets a rule such that all string scalar fields on outbound data items (e.g. INTERFACE and OBJECT types)
        /// will be declared as "not null" by default for all generated graph types.This can be overriden on a per field or
        /// per argument basis.
        /// </summary>
        /// <returns>GraphSchemaFormatStrategyBuilder.</returns>
        public SchemaFormatStrategyBuilder WithRequiredOutputStrings()
        {
            if (!_format.NullabilityStrategy.HasFlag(TypeExpressionNullabilityFormatRules.NonNullOutputStrings))
                _format.NullabilityStrategy = _format.NullabilityStrategy | TypeExpressionNullabilityFormatRules.NonNullOutputStrings;

            return this;
        }

        /// <summary>
        /// Sets a rule such that all List types (e.g. List, Array, IEnumerable etc.) are marked as "not null" by default
        /// when accepted as an argument to a field or returned by a field. This can be overriden on a per field or
        /// per argument basis.
        /// </summary>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public SchemaFormatStrategyBuilder WithRequiredLists()
        {
            if (!_format.NullabilityStrategy.HasFlag(TypeExpressionNullabilityFormatRules.NonNullLists))
                _format.NullabilityStrategy = _format.NullabilityStrategy | TypeExpressionNullabilityFormatRules.NonNullLists;

            return this;
        }

        /// <summary>
        /// Sets a rule such that all reference types (e.g. classes and interfaces) are marked as "not null" by default
        /// when accepted as an argument to a field or returned by a field. This can be overriden on a per field or
        /// per argument basis.
        /// </summary>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public SchemaFormatStrategyBuilder WithRequiredObjects()
        {
            if (!_format.NullabilityStrategy.HasFlag(TypeExpressionNullabilityFormatRules.NonNullReferenceTypes))
                _format.NullabilityStrategy = _format.NullabilityStrategy | TypeExpressionNullabilityFormatRules.NonNullReferenceTypes;

            return this;
        }

        /// <summary>
        /// Clears all nullability rules, including the default non-null template rule, such that
        /// no rules are applied to any objects. All intermediate graph types, strings, lists are treated
        /// as nullable.
        /// </summary>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public SchemaFormatStrategyBuilder ClearNullabilityRules()
        {
            _format.NullabilityStrategy = TypeExpressionNullabilityFormatRules.None;
            return this;
        }

        /// <summary>
        /// Sets the formating of graph type names (object, interfaces etc.) to the supplied built in strategy.
        /// </summary>
        /// <remarks>
        /// DEFAULT: ProperCase
        /// </remarks>
        /// <param name="strategy">The strategy to employ for graph type names.</param>
        /// <returns>GraphSchemaFormatStrategyBuilder.</returns>
        public SchemaFormatStrategyBuilder WithGraphTypeNameFormat(SchemaItemNameFormatOptions strategy)
        {
            _format.GraphTypeNameStrategy = strategy;
            return this;
        }

        /// <summary>
        /// Sets the formating of field names to the supplied built in strategy.
        /// </summary>
        /// <remarks>
        /// DEFAULT: camelCase
        /// </remarks>
        /// <param name="strategy">The strategy to employ for field names.</param>
        /// <returns>GraphSchemaFormatStrategyBuilder.</returns>
        public SchemaFormatStrategyBuilder WithFieldNameFormat(SchemaItemNameFormatOptions strategy)
        {
            _format.FieldNameStrategy = strategy;
            return this;
        }

        /// <summary>
        /// Sets the formating of enum values to the supplied built in strategy.
        /// </summary>
        /// <remarks>
        /// Default: UPPER CASE
        /// </remarks>
        /// <param name="strategy">The strategy to employ for graph type names.</param>
        /// <returns>GraphSchemaFormatStrategyBuilder.</returns>
        public SchemaFormatStrategyBuilder WithEnumValueFormat(SchemaItemNameFormatOptions strategy)
        {
            _format.EnumValueStrategy = strategy;
            return this;
        }

        /// <summary>
        /// Returns the format strategy instance being built.
        /// </summary>
        /// <returns>GraphSchemaFormatStrategy.</returns>
        public ISchemaFormatStrategy Build()
        {
            return _format;
        }
    }
}