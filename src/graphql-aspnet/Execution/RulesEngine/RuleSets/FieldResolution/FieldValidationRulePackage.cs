// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.FieldResolution
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.FieldResolution.FieldValidation;
    using GraphQL.AspNet.Interfaces.Execution.RulesEngine;

    /// <summary>
    /// A rule package defining the rules to perform final validation checks of a field value and its children.
    /// </summary>
    public sealed class FieldValidationRulePackage : IRulePackage<FieldValidationContext>
    {
        /// <summary>
        /// Gets the singleton instance of this rule package.
        /// </summary>
        /// <value>The instance.</value>
        public static FieldValidationRulePackage Instance { get; } = new FieldValidationRulePackage();

        private readonly List<IRuleStep<FieldValidationContext>> _ruleSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldValidationRulePackage" /> class.
        /// </summary>
        private FieldValidationRulePackage()
        {
            _ruleSet = new List<IRuleStep<FieldValidationContext>>();
            _ruleSet.Add(new Rule_6_4_4_ChildErrorsAndNonNullability());
            _ruleSet.Add(new GraphDataItem_FinalizeDataItem());
        }

        /// <summary>
        /// Allow for addition of custom validation rules for a given document part. Rules added via this method
        /// will be executed against the document part in the order they are supplied and AFTER all baseline rules
        /// are executed.
        /// </summary>
        /// <param name="rule">The rule to be invoked.</param>
        public void AddCustomRule(IRuleStep<FieldValidationContext> rule)
        {
            Validation.ThrowIfNull(rule, nameof(rule));

            // insert the rule just before finalize data item (which must be last)
            _ruleSet.Insert(_ruleSet.Count - 2, rule);
        }

        /// <inheritdoc/>
        public IEnumerable<IRuleStep<FieldValidationContext>> FetchRules(FieldValidationContext context)
        {
            return _ruleSet;
        }
    }
}