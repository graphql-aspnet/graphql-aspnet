// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.VariableDataValidation
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.RulesEngine;

    /// <summary>
    /// A rule package for doing a holistic validation pass at a parsed query document, in context of a chosen operation,
    /// after all variables have been coerced and resolved. Performs deeper validations (such as variable values conforming to
    /// extended, non-specification rules) across the fully validated operation.
    /// </summary>
    public class VariableDataValidationRulePackage : IRulePackage<VariableDataValidationContext>
    {
        /// <summary>
        /// Gets the singleton instance of this rule package.
        /// </summary>
        /// <value>The instance.</value>
        public static VariableDataValidationRulePackage Instance { get; } = new VariableDataValidationRulePackage();

        private readonly IDictionary<DocumentPartType, IList<IRuleStep<VariableDataValidationContext>>> _stepCollection;

        /// <summary>
        /// Prevents a default instance of the <see cref="VariableDataValidationRulePackage" /> class from being created.
        /// </summary>
        private VariableDataValidationRulePackage()
        {
            _stepCollection = new Dictionary<DocumentPartType, IList<IRuleStep<VariableDataValidationContext>>>();
        }

        /// <summary>
        /// Allow for addition of custom variable validation rules for a given document part. Rules added via this method
        /// will be executed against the document part in the order they are supplied and AFTER all baseline rules
        /// are executed.
        /// </summary>
        /// <param name="documentPart">The document part targetd by the rule.</param>
        /// <param name="rule">The rule to be invoked.</param>
        public void AddCustomRule(DocumentPartType documentPart, IRuleStep<VariableDataValidationContext> rule)
        {
            Validation.ThrowIfNull(rule, nameof(rule));

            if (!_stepCollection.TryGetValue(documentPart, out var steps))
            {
                steps = [];
                _stepCollection.Add(documentPart, steps);
            }

            steps.Add(rule);
        }

        /// <inheritdoc />
        public IEnumerable<IRuleStep<VariableDataValidationContext>> FetchRules(VariableDataValidationContext context)
        {
            if (_stepCollection.TryGetValue(context.DocumentPartType, out var steps))
                return steps;

            // nothing else applicable right now
            return [];
        }

        /// <inheritdoc />
        public bool HasAnyRules => _stepCollection.Count > 0;
    }
}