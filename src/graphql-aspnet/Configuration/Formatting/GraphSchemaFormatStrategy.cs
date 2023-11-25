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
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A strategy, employed by a schema, to make programatic alterations
    /// to schema items as they are being constructed.
    /// </summary>
    public class GraphSchemaFormatStrategy
    {
        private readonly GraphNameFormatStrategy _typeNameStrategy;
        private readonly GraphNameFormatStrategy _fieldNameStrategy;
        private readonly GraphNameFormatStrategy _enumValueStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSchemaFormatStrategy"/> class.
        /// </summary>
        /// <param name="singleStrategy">A single format strategy to use for all naming formats.</param>
        public GraphSchemaFormatStrategy(GraphNameFormatStrategy singleStrategy)
            : this(singleStrategy, singleStrategy, singleStrategy)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSchemaFormatStrategy"/> class.
        /// </summary>
        /// <param name="typeNameStrategy">The format strategy to use for graph type names.</param>
        /// <param name="fieldNameStrategy">The format strategy to use for field names.</param>
        /// <param name="enumValueStrategy">The format strategy to use for enum values.</param>
        public GraphSchemaFormatStrategy(
           GraphNameFormatStrategy typeNameStrategy = GraphNameFormatStrategy.ProperCase,
           GraphNameFormatStrategy fieldNameStrategy = GraphNameFormatStrategy.CamelCase,
           GraphNameFormatStrategy enumValueStrategy = GraphNameFormatStrategy.UpperCase)
        {
            _typeNameStrategy = typeNameStrategy;
            _fieldNameStrategy = fieldNameStrategy;
            _enumValueStrategy = enumValueStrategy;
        }

        /// <summary>
        /// Applies custom, programatic level formatting to the
        /// target schema item.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="schemaItem"/> is not yet added to the schema
        /// and may not be be fully populated.
        /// For example, a field will not yet have a parent assignment and graph types
        /// will not have any rendered fields when this method is processed.
        /// </para>
        /// <para>
        /// Schema items are immutable for any essential data values. Use the various
        /// implementations of <c>.Clone()</c> to create copies of a schema item with
        /// updated values.
        /// </para>
        /// <para>
        /// Applied changes are accepted as is with no further validation, ensure your
        /// formats are consistant with the target schema's expectations.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of schema item being formatted.</typeparam>
        /// <param name="configuration">The complete configuration settings setup
        /// at application start for the target schema.</param>
        /// <param name="schemaItem">The schema item to apply formatting rules to.</param>
        /// <returns>The updated or altered schema item instance that was formatted.</returns>
        public virtual T ApplyFormatting<T>(ISchemaConfiguration configuration, T schemaItem)
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
        /// Formats a field or argument name according to the strategy declared on this formatter.
        /// </summary>
        /// <param name="name">The field or argument name to format.</param>
        /// <returns>System.String.</returns>
        public virtual string FormatFieldName(string name)
        {
            return this.FormatName(name, _fieldNameStrategy);
        }

        /// <summary>
        /// Formats the enum value name according to the strategy declared on this formatter.
        /// </summary>
        /// <param name="name">The enum value name to format.</param>
        /// <returns>System.String.</returns>
        public virtual string FormatEnumValueName(string name)
        {
            return this.FormatName(name, _enumValueStrategy);
        }

        /// <summary>
        /// Formats the graph type name according to the strategy declared on this formatter.
        /// </summary>
        /// <param name="name">The type name to format.</param>
        /// <returns>System.String.</returns>
        public virtual string FormatGraphTypeName(string name)
        {
            // enforce non-renaming standards in the maker since the
            // directly controls the formatter
            if (!GlobalTypes.CanBeRenamed(name))
                return name;

            return this.FormatName(name, _typeNameStrategy);
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
        /// Applies the schema specific formats identified by <paramref name="configuration"/>
        /// to the target <paramref name="graphType"/>.
        /// </summary>
        /// <param name="configuration">The configuration to read formatting settings
        /// from.</param>
        /// <param name="graphType">The graph type to update.</param>
        /// <returns>IGraphType.</returns>
        protected virtual IGraphType FormatGraphType(ISchemaConfiguration configuration, IGraphType graphType)
        {
            if (graphType is IScalarGraphType scalarType)
            {
                if (!GlobalTypes.CanBeRenamed(scalarType.Name))
                    return graphType;
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
            var formattedName = this.FormatFieldName(argument.Name);

            var typeExpression = argument.TypeExpression;
            typeExpression = typeExpression.Clone(this.FormatGraphTypeName(typeExpression.TypeName));

            return argument.Clone(argumentName: formattedName, typeExpression: typeExpression);
        }

        /// <summary>
        /// Formats or reformats the name according to the rules of this formatter.
        /// </summary>
        /// <param name="name">The name value being formatted.</param>
        /// <param name="strategy">The selected strategy to format with.</param>
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