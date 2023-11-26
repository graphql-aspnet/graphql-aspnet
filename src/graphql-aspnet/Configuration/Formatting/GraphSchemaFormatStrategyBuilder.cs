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
    using System;

    /// <summary>
    /// A builder class to formulate a <see cref="GraphSchemaFormatStrategy"/>
    /// with various options.
    /// </summary>
    public class GraphSchemaFormatStrategyBuilder
    {
        /// <summary>
        /// Starts a new strategy builder instance.
        /// </summary>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public static GraphSchemaFormatStrategyBuilder Create()
        {
            return new GraphSchemaFormatStrategyBuilder();
        }

        private readonly GraphSchemaFormatStrategy _format;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSchemaFormatStrategyBuilder"/> class.
        /// </summary>
        public GraphSchemaFormatStrategyBuilder()
        {
            _format = new GraphSchemaFormatStrategy();
        }

        /// <summary>
        /// Sets a rule such that all string scalars are marked as "not null" by default
        /// when accepted as an argument to a field or returned by a field. This can be overriden on a per field or
        /// per argument basis.
        /// </summary>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public GraphSchemaFormatStrategyBuilder WithRequiredStrings()
        {
            if (!_format.NullabilityStrategy.HasFlag(NullabilityFormatStrategy.NonNullStrings))
                _format.NullabilityStrategy = _format.NullabilityStrategy | NullabilityFormatStrategy.NonNullStrings;

            return this;
        }

        /// <summary>
        /// Sets a rule such that all List types (e.g. List, Array, IEnumerable etc.) are marked as "not null" by default
        /// when accepted as an argument to a field or returned by a field. This can be overriden on a per field or
        /// per argument basis.
        /// </summary>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public GraphSchemaFormatStrategyBuilder WithRequiredLists()
        {
            if (!_format.NullabilityStrategy.HasFlag(NullabilityFormatStrategy.NonNullLists))
                _format.NullabilityStrategy = _format.NullabilityStrategy | NullabilityFormatStrategy.NonNullLists;

            return this;
        }

        /// <summary>
        /// Sets a rule such that all reference types (e.g. classes and interfaces) are marked as "not null" by default
        /// when accepted as an argument to a field or returned by a field. This can be overriden on a per field or
        /// per argument basis.
        /// </summary>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public GraphSchemaFormatStrategyBuilder WithRequiredObjects()
        {
            if (!_format.NullabilityStrategy.HasFlag(NullabilityFormatStrategy.NonNullReferenceTypes))
                _format.NullabilityStrategy = _format.NullabilityStrategy | NullabilityFormatStrategy.NonNullReferenceTypes;

            return this;
        }

        /// <summary>
        /// Clears all nullability rules, including the default non-null template rule, such that
        /// no rules are applied to any objects. All intermediate graph types, strings, lists are treated
        /// as nullable.
        /// </summary>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public GraphSchemaFormatStrategyBuilder ClearNullabilityRules()
        {
            _format.NullabilityStrategy = NullabilityFormatStrategy.None;
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
        public GraphSchemaFormatStrategyBuilder WithGraphTypeNameFormat(GraphNameFormatStrategy strategy)
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
        public GraphSchemaFormatStrategyBuilder WithFieldNameFormat(GraphNameFormatStrategy strategy)
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
        public GraphSchemaFormatStrategyBuilder WithEnumValueFormat(GraphNameFormatStrategy strategy)
        {
            _format.EnumValueStrategy = strategy;
            return this;
        }

        /// <summary>
        /// Returns the format strategy instance being built.
        /// </summary>
        /// <returns>GraphSchemaFormatStrategy.</returns>
        public GraphSchemaFormatStrategy Build()
        {
            return _format;
        }
    }
}