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
        /// Creates a format strategy builder and applies the given format option for all name format rules.
        /// </summary>
        /// <param name="formatOption">The single format option to use for all generated names.</param>
        /// <param name="applyDefaultRules">if set to <c>true</c> default format rules
        /// are applied to the builder. When false, no rules are applied and the builder is created
        /// in an empty state.</param>
        /// <returns>SchemaFormatStrategy.</returns>
        public static SchemaFormatStrategyBuilder Create(
            TextFormatOptions formatOption,
            bool applyDefaultRules = true)
        {
            return CreateInternal(
                 false,
                 formatOption,
                 formatOption,
                 formatOption,
                 formatOption,
                 formatOption);
        }

        /// <summary>
        /// Creates a format strategy builder with the provided text format overrides
        /// will be applied as defined.
        /// </summary>
        /// <param name="typeNameFormat">The name format to use for graph type names.</param>
        /// <param name="fieldNameFormat">The name format to use for fields.</param>
        /// <param name="enumValueNameFormat">The name format to use for enum values.</param>
        /// <param name="argumentNameFormat">The name format to use for arguments on fields and directives.</param>
        /// <param name="directiveNameFormat">The name format to use for directive type names.</param>
        /// <param name="applyDefaultRules">if set to <c>true</c> default format rules
        /// are applied to the builder. When false, no rules are applied and the builder is created
        /// in an empty state.</param>
        /// <returns>SchemaFormatStrategy.</returns>
        public static SchemaFormatStrategyBuilder Create(
            TextFormatOptions? typeNameFormat = TextFormatOptions.ProperCase,
            TextFormatOptions? fieldNameFormat = TextFormatOptions.CamelCase,
            TextFormatOptions? enumValueNameFormat = TextFormatOptions.UpperCase,
            TextFormatOptions? argumentNameFormat = TextFormatOptions.CamelCase,
            TextFormatOptions? directiveNameFormat = TextFormatOptions.CamelCase,
            bool applyDefaultRules = true)
        {
            return CreateInternal(
                applyDefaultRules,
                typeNameFormat,
                fieldNameFormat,
                enumValueNameFormat,
                argumentNameFormat,
                directiveNameFormat);
        }

        private static SchemaFormatStrategyBuilder CreateInternal(
            bool applyDefaultRules,
            TextFormatOptions? typeNameFormat,
            TextFormatOptions? fieldNameFormat,
            TextFormatOptions? enumValueNameFormat,
            TextFormatOptions? argumentNameFormat,
            TextFormatOptions? directiveNameFormat)
        {
            var builder = new SchemaFormatStrategyBuilder();

            if (!applyDefaultRules)
                builder = builder.Clear();

            if (typeNameFormat.HasValue)
                builder = builder.WithGraphTypeNameFormat(typeNameFormat.Value);
            if (fieldNameFormat.HasValue)
                builder = builder.WithFieldNameFormat(fieldNameFormat.Value);
            if (enumValueNameFormat.HasValue)
                builder = builder.WithEnumValueFormat(enumValueNameFormat.Value);
            if (directiveNameFormat.HasValue)
                builder = builder.WithDirectiveNameFormat(directiveNameFormat.Value);
            if (argumentNameFormat.HasValue)
                builder = builder.WithFieldArgumentNameFormat(argumentNameFormat.Value);

            return builder;
        }

        private TextFormatOptions? _typeNameFormat = null;
        private TextFormatOptions? _fieldNameFormat = null;
        private TextFormatOptions? _enumValueFormat = null;
        private TextFormatOptions? _argumentNameFormat = null;
        private TextFormatOptions? _directiveNameFormat = null;
        private List<ISchemaItemFormatRule> _customRules = null;

        private bool _includeIntermediateTypeNonNullRules;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaFormatStrategyBuilder"/> class.
        /// </summary>
        protected SchemaFormatStrategyBuilder()
        {
            _customRules = new List<ISchemaItemFormatRule>();

            // set all aspects of virtual fields as "non null" by default
            _includeIntermediateTypeNonNullRules = true;
        }

        /// <summary>
        /// Sets a rule such that intermediate graph type fields are declared as "non null".
        /// These are guaranteed to exist by the runtime to always be present and this declaration is made
        /// by default.
        /// </summary>
        /// <remarks>
        /// Use the <see cref="Clear"/> method to unset this rule. This can be useful for backwards
        /// compatiability with v1.x of this library.
        /// </remarks>
        /// <returns>SchemaFormatStrategyBuilder.</returns>
        public virtual SchemaFormatStrategyBuilder DeclareIntermediateTypesAsNonNull()
        {
            _includeIntermediateTypeNonNullRules = true;
            return this;
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
        public virtual SchemaFormatStrategyBuilder Clear()
        {
            _customRules.Clear();

            _typeNameFormat = null;
            _fieldNameFormat = null;
            _enumValueFormat = null;
            _argumentNameFormat = null;
            _directiveNameFormat = null;

            _includeIntermediateTypeNonNullRules = false;

            return this;
        }

        /// <summary>
        /// Creates the format strategy from all the rules and options set on this builder.
        /// </summary>
        /// <returns>ISchemaFormatStrategy.</returns>
        public virtual ISchemaFormatStrategy Build()
        {
            return new SchemaFormatStrategy(this.GatherRules());
        }

        /// <summary>
        /// Gathers the rules set on this builder into a single array.
        /// </summary>
        /// <returns>ISchemaItemFormatRule[].</returns>
        protected virtual ISchemaItemFormatRule[] GatherRules()
        {
            var ruleSet = new List<ISchemaItemFormatRule>();

            // name rules first
            ruleSet.AddRange(this.CreateNameFormatRules());

            // intermediate type rules second
            if (_includeIntermediateTypeNonNullRules)
            {
                ruleSet.Add(new ApplyFieldNonNullItemTypeExpressionFormatRule(x => x.IsVirtual));
                ruleSet.Add(new ApplyFieldNonNullListTypeExpressionFormatRule(x => x.IsVirtual));
            }

            // used defined rules last
            ruleSet.AddRange(_customRules);

            return ruleSet.ToArray();
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
    }
}