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
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration.Formatting.FormatRules;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A builder class to formulate a <see cref="SchemaFormatStrategy"/>
    /// by applying a varied set of rules according to the devleoper's configuration requirements.
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

        private TextFormatOptions? _typeNameFormat;
        private TextFormatOptions? _fieldNameFormat;
        private TextFormatOptions? _enumValueFormat;
        private TextFormatOptions? _argumentNameFormat;
        private TextFormatOptions? _directiveNameFormat;
        private List<ISchemaItemFormatRule> _customRules;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaFormatStrategyBuilder"/> class.
        /// </summary>
        public SchemaFormatStrategyBuilder()
        {
            _customRules = new List<ISchemaItemFormatRule>();

            _typeNameFormat = TextFormatOptions.ProperCase;
            _enumValueFormat = TextFormatOptions.UpperCase;

            _fieldNameFormat = TextFormatOptions.CamelCase;
            _argumentNameFormat = TextFormatOptions.CamelCase;
            _directiveNameFormat = TextFormatOptions.CamelCase;

            // set all aspects of virtual fields as "non null" by default
            _customRules.Add(new ApplyFieldNonNullItemTypeExpressionFormatRule(x => x.IsVirtual));
            _customRules.Add(new ApplyFieldNonNullListTypeExpressionFormatRule(x => x.IsVirtual));
        }

        /// <summary>
        /// Creates a set of rules to apply the name formats requested on this builder.
        /// </summary>
        /// <returns>IEnumerable&lt;ISchemaItemFormatRule&gt;.</returns>
        protected virtual IEnumerable<ISchemaItemFormatRule> CreateNameFormatRules()
        {
            // formal name formatting
            if (_typeNameFormat.HasValue)
            {
                yield return new ApplyGraphTypeNameFormatRule(_typeNameFormat.Value);
                yield return new ApplyGraphTypeNameFormatToUnionTypeMembersRule(_typeNameFormat.Value);
                yield return new ApplyFieldTypeExpressionNameFormatRule(_typeNameFormat.Value);
                yield return new ApplyArgumentTypeExpressionNameFormatRule(_typeNameFormat.Value);
                yield return new ApplyInputFieldTypeExpressionNameFormatRule(_typeNameFormat.Value);
            }

            if (_fieldNameFormat.HasValue)
            {
                yield return new ApplyFieldNameFormatRule(_fieldNameFormat.Value);
                yield return new ApplyInputFieldNameFormatRule(_fieldNameFormat.Value);
            }

            if (_directiveNameFormat.HasValue)
            {
                yield return new ApplyDirectiveNameFormatRule(_directiveNameFormat.Value);
            }

            if (_argumentNameFormat.HasValue)
            {
                yield return new ApplyArgumentNameFormatRule(_argumentNameFormat.Value);
            }

            if (_enumValueFormat.HasValue)
            {
                yield return new ApplyEnumNameFormatRule(_enumValueFormat.Value);
            }
        }

        /// <summary>
        /// Sets a rule such fields on INPUT_OBJECT types that match the given predicate
        /// will be declared as "not null" by default for all generated graph types.
        /// This can be overriden on a per field basis using <see cref="GraphFieldAttribute" />.
        /// </summary>
        /// <remarks>Example: <c>[String] => [String!]</c></remarks>
        /// <param name="predicate">The predicate function that a field must match for the rule to be invoked.</param>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public virtual SchemaFormatStrategyBuilder DeclareInputFieldValuesAsNonNull(Func<IInputGraphField, bool> predicate)
        {
            Validation.ThrowIfNull(predicate, nameof(predicate));

            return this.DeclareCustomRule(new ApplyInputFieldNonNullItemTypeExpressionFormatRule(predicate));
        }

        /// <summary>
        /// Sets a rule such fields on INPUT_OBJECT types that match the given predicate and return a list
        /// will be declared as "not null" by default for all generated graph types. Nested lists will also
        /// be made non-nullable. Matching fields that do not declare a list will be quietly skipped.
        /// This can be overriden on a per field basis using <see cref="GraphFieldAttribute" />.
        /// </summary>
        /// <remarks>Example: <c>[String] => [String]!</c></remarks>
        /// <param name="predicate">The predicate function that a field must match for the rule to be invoked.</param>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public virtual SchemaFormatStrategyBuilder DeclareInputFieldListsAsNonNull(Func<IInputGraphField, bool> predicate)
        {
            return this.DeclareCustomRule(new ApplyInputFieldNonNullListTypeExpressionFormatRule(predicate));
        }

        /// <summary>
        /// Sets a rule such fields on OBJECT and INTERFACE types that match the given predicate
        /// will be declared as "not null" by default for all generated graph types.
        /// This can be overriden on a per field basis using <see cref="GraphFieldAttribute" />.
        /// </summary>
        /// <remarks>Example: <c>[String] => [String!]</c></remarks>
        /// <param name="predicate">The predicate function that a field must match for the rule to be invoked.</param>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public virtual SchemaFormatStrategyBuilder DeclareFieldValuesAsNonNull(Func<IGraphField, bool> predicate)
        {
            Validation.ThrowIfNull(predicate, nameof(predicate));
            return this.DeclareCustomRule(new ApplyFieldNonNullItemTypeExpressionFormatRule(predicate));
        }

        /// <summary>
        /// Sets a rule such that fields on OBJECT and INTERFACE types which match the given predicate and
        /// return a list of items will be declared as "not null" by default for all generated graph types. Nested lists will also
        /// be made non-nullable. Matching fields that do not declare a list will be quietly skipped.
        /// This can be overriden on a per field basis using <see cref = "GraphFieldAttribute" />.
        /// </summary>
        /// <remarks>Example: <c>[String] => [String]!</c></remarks>
        /// <param name="predicate">The predicate function that a field must match for the rule to be invoked.</param>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public virtual SchemaFormatStrategyBuilder DeclareFieldListsAsNonNull(Func<IGraphField, bool> predicate)
        {
            Validation.ThrowIfNull(predicate, nameof(predicate));
            return this.DeclareCustomRule(new ApplyFieldNonNullListTypeExpressionFormatRule(predicate));
        }

        /// <summary>
        /// Sets a rule such that arguments on OBJECT and INTERFACE fields that match the given predicate
        /// will have their object value be declared as "not null" by default for all generated graph types.
        /// This can be overriden on a per field basis using <see cref="FromGraphQLAttribute" />.
        /// </summary>
        /// <remarks>Example: <c>[String] => [String!]</c></remarks>
        /// <param name="predicate">The predicate function that a field must match for the rule to be invoked.</param>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public virtual SchemaFormatStrategyBuilder DeclareArgumentValuesAsNonNull(Func<IGraphArgument, bool> predicate)
        {
            Validation.ThrowIfNull(predicate, nameof(predicate));
            return this.DeclareCustomRule(new ApplyArgumentNonNullItemTypeExpressionFormatRule(predicate));
        }

        /// <summary>
        /// Sets a rule such that arguments on OBJECT and INTERFACE fields which match the given predicate and
        /// return a list of items will be declared as "not null" by default for all generated graph types. Nested lists will also
        /// be made non-nullable. Matching arguments that do not declare a list will be quietly skipped.
        /// This can be overriden on a per field basis using <see cref = "FromGraphQLAttribute" />.
        /// </summary>
        /// /// <remarks>Example: <c>[String] => [String]!</c></remarks>
        /// <param name="predicate">The predicate function that a field must match for the rule to be invoked.</param>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public virtual SchemaFormatStrategyBuilder DeclareArgumentListsAsNonNull(Func<IGraphArgument, bool> predicate)
        {
            Validation.ThrowIfNull(predicate, nameof(predicate));
            return this.DeclareCustomRule(new ApplyArgumentNonNullListTypeExpressionFormatRule(predicate));
        }

        /// <summary>
        /// Declares a custom built rule on the strategy. This rule will be executed against each
        /// <see cref="ISchemaItem"/> just before its added to the target schema.
        /// </summary>
        /// <param name="rule">The rule to declare.</param>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public virtual SchemaFormatStrategyBuilder DeclareCustomRule(ISchemaItemFormatRule rule)
        {
            Validation.ThrowIfNull(rule, nameof(rule));
            _customRules.Add(rule);
            return this;
        }

        /// <summary>
        /// Sets the formating of graph type names (object, interface etc.) to the supplied built in strategy.
        /// </summary>
        /// <remarks>
        /// DEFAULT: ProperCase
        /// </remarks>
        /// <param name="format">The strategy to use for graph type names.</param>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public virtual SchemaFormatStrategyBuilder WithGraphTypeNameFormat(TextFormatOptions format)
        {
            _typeNameFormat = format;
            return this;
        }

        /// <summary>
        /// Sets the formating of field names on INPUT_OBJECT, OBJECT and INTERFACE types
        /// to the supplied built in strategy.
        /// </summary>
        /// <remarks>
        /// DEFAULT: camelCase
        /// </remarks>
        /// <param name="format">The format to use for field names.</param>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public virtual SchemaFormatStrategyBuilder WithFieldNameFormat(TextFormatOptions format)
        {
            _fieldNameFormat = format;
            return this;
        }

        /// <summary>
        /// Sets the formating of enum values to the supplied built in strategy.
        /// </summary>
        /// <remarks>
        /// Default: UPPER CASE
        /// </remarks>
        /// <param name="format">The strategy to use for graph type names.</param>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public virtual SchemaFormatStrategyBuilder WithEnumValueFormat(TextFormatOptions format)
        {
            _enumValueFormat = format;
            return this;
        }

        /// <summary>
        /// Sets the formating of argument names defined on fields in INTERFACE and OBJECT
        /// graph types as well as arguments on DIRECTIVEs.
        /// </summary>
        /// <remarks>
        /// Default: camelCase
        /// </remarks>
        /// <param name="format">The format to use for all field argument names.</param>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public virtual SchemaFormatStrategyBuilder WithFieldArgumentNameFormat(TextFormatOptions format)
        {
            _argumentNameFormat = format;
            return this;
        }

        /// <summary>
        /// Sets the formating of directive names for the target schema (e.g. <c>@camelCasedDirectiveName</c>)
        /// </summary>
        /// <remarks>
        /// Default: camelCase
        /// </remarks>
        /// <param name="format">The format to use for all directive names.</param>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public virtual SchemaFormatStrategyBuilder WithDirectiveNameFormat(TextFormatOptions format)
        {
            _directiveNameFormat = format;
            return this;
        }

        /// <summary>
        /// Clears all custom rules and name format options.
        /// </summary>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public SchemaFormatStrategyBuilder Clear()
        {
            _customRules.Clear();

            _typeNameFormat = null;
            _fieldNameFormat = null;
            _enumValueFormat = null;
            _argumentNameFormat = null;
            _directiveNameFormat = null;

            return this;
        }

        /// <summary>
        /// Creates the format strategy from all the rules and options set on this builder.
        /// </summary>
        /// <returns>ISchemaFormatStrategy.</returns>
        public virtual ISchemaFormatStrategy Build()
        {
            var ruleSet = new List<ISchemaItemFormatRule>();
            ruleSet.AddRange(this.CreateNameFormatRules());
            ruleSet.AddRange(_customRules);

            return new SchemaFormatStrategy(ruleSet.ToArray());
        }
    }
}