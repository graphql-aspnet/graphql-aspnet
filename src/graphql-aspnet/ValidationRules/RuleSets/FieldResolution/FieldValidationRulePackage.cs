// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.FieldResolution
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.ValidationRules.Interfaces;
    using GraphQL.AspNet.ValidationRules.RuleSets.FieldResolution.FieldValidation;

    /// <summary>
    /// A rule package for performing final validation checks of a rule and all its children.
    /// </summary>
    internal sealed class FieldValidationRulePackage : IRulePackage<FieldValidationContext>
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
        /// Fetches the rules that should be executed, in order, for the given context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>IEnumerable&lt;IRuleStep&lt;TContext&gt;&gt;.</returns>
        public IEnumerable<IRuleStep<FieldValidationContext>> FetchRules(FieldValidationContext context)
        {
            return _ruleSet;
        }
    }
}