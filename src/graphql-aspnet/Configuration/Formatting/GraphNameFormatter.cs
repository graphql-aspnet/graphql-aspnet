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
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Extensions;

    /// <summary>
    /// A formatter class capable of altering a graph item name before its added to a schema.
    /// </summary>
    [DebuggerDisplay("Type = {_typeNameStrategy}, Field = {_fieldNameStrategy}, Enum = {_enumValueStrategy}")]
    public class GraphNameFormatter
    {
        private readonly GraphNameFormatStrategy _typeNameStrategy;
        private readonly GraphNameFormatStrategy _fieldNameStrategy;
        private readonly GraphNameFormatStrategy _enumValueStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphNameFormatter"/> class.
        /// </summary>
        protected GraphNameFormatter()
            : this(
                typeNameStrategy: GraphNameFormatStrategy.ProperCase,
                fieldNameStrategy: GraphNameFormatStrategy.CamelCase,
                enumValueStrategy: GraphNameFormatStrategy.UpperCase)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphNameFormatter"/> class.
        /// </summary>
        /// <param name="singleStrategy">The single strategy to use for all naming options.</param>
        public GraphNameFormatter(GraphNameFormatStrategy singleStrategy)
        {
            _typeNameStrategy = singleStrategy;
            _fieldNameStrategy = singleStrategy;
            _enumValueStrategy = singleStrategy;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphNameFormatter" /> class.
        /// </summary>
        /// <param name="typeNameStrategy">The strategy to use for naming graph types.</param>
        /// <param name="fieldNameStrategy">The strategy to use for naming fields.</param>
        /// <param name="enumValueStrategy">The strategy to use for naming individual enum values.</param>
        public GraphNameFormatter(
            GraphNameFormatStrategy typeNameStrategy = GraphNameFormatStrategy.ProperCase,
            GraphNameFormatStrategy fieldNameStrategy = GraphNameFormatStrategy.CamelCase,
            GraphNameFormatStrategy enumValueStrategy = GraphNameFormatStrategy.UpperCase)
        {
            _typeNameStrategy = typeNameStrategy;
            _fieldNameStrategy = fieldNameStrategy;
            _enumValueStrategy = enumValueStrategy;
        }

        /// <summary>
        /// Formats the field name according to the strategy declared on this formatter.
        /// </summary>
        /// <param name="name">The name to format.</param>
        /// <returns>System.String.</returns>
        public virtual string FormatFieldName(string name)
        {
            return this.FormatName(name, _fieldNameStrategy);
        }

        /// <summary>
        /// Formats the enum value name according to the strategy declared on this formatter.
        /// </summary>
        /// <param name="name">The name to format.</param>
        /// <returns>System.String.</returns>
        public virtual string FormatEnumValueName(string name)
        {
            return this.FormatName(name, _enumValueStrategy);
        }

        /// <summary>
        /// Formats the graph type name according to the strategy declared on this formatter.
        /// </summary>
        /// <param name="name">The name to format.</param>
        /// <returns>System.String.</returns>
        public virtual string FormatGraphTypeName(string name)
        {
            // scalar names are considered fixed constants and can't be changed
            if (name == null || GraphQLProviders.ScalarProvider.IsScalar(name))
                return name;

            return this.FormatName(name, _typeNameStrategy);
        }

        /// <summary>
        /// Formats or reformats the name according to the rules of this formatter.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="strategy">The strategy to invoke.</param>
        /// <returns>System.String.</returns>
        protected virtual string FormatName(string name, GraphNameFormatStrategy strategy)
        {
            if (name == null)
                return null;

            switch (strategy)
            {
                case GraphNameFormatStrategy.ProperCase:
                    return name.FirstCharacterToUpperInvariant();

                case GraphNameFormatStrategy.CamelCase:
                    return name.FirstCharacterToLowerInvariant();

                case GraphNameFormatStrategy.UpperCase:
                    return name.ToUpperInvariant();

                case GraphNameFormatStrategy.LowerCase:
                    return name.ToLowerInvariant();

                // ReSharper disable once RedundantCaseLabel
                case GraphNameFormatStrategy.NoChanges:
                default:
                    return name;
            }
        }
    }
}