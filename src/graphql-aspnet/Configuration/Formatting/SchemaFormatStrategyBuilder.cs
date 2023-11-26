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
    /// <summary>
    /// A builder class to formulate a <see cref="GraphSchemaFormatStrategy"/>
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

        private readonly GraphSchemaFormatStrategy _format;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaFormatStrategyBuilder"/> class.
        /// </summary>
        public SchemaFormatStrategyBuilder()
        {
            _format = new GraphSchemaFormatStrategy();
        }

        /// <summary>
        /// Sets a rule such that all string scalars are marked as "not null" by default
        /// when accepted as an argument to a field or returned by a field. This can be overriden on a per field or
        /// per argument basis.
        /// </summary>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public SchemaFormatStrategyBuilder WithRequiredStrings()
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
        public SchemaFormatStrategyBuilder WithRequiredLists()
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
        public SchemaFormatStrategyBuilder WithRequiredObjects()
        {
            if (!_format.NullabilityStrategy.HasFlag(NullabilityFormatStrategy.NonNullReferenceTypes))
                _format.NullabilityStrategy = _format.NullabilityStrategy | NullabilityFormatStrategy.NonNullReferenceTypes;

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