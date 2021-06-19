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
    using GraphQL.AspNet.ValidationRules.RuleSets.FieldResolution.FieldCompletion;

    /// <summary>
    /// A rule package with the necessary rules to completion the execution of a single field context.
    /// </summary>
    internal sealed class FieldCompletionRulePackage : IRulePackage<FieldValidationContext>
    {
        /// <summary>
        /// Gets the singleton instance of this rule package.
        /// </summary>
        /// <value>The instance.</value>
        public static FieldCompletionRulePackage Instance { get; } = new FieldCompletionRulePackage();

        private readonly List<IRuleStep<FieldValidationContext>> _ruleSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldCompletionRulePackage" /> class.
        /// </summary>
        private FieldCompletionRulePackage()
        {
            _ruleSet = new List<IRuleStep<FieldValidationContext>>();
            _ruleSet.Add(new Rule_6_4_3_ServerValueCompletion());
            _ruleSet.Add(new Rule_6_4_3_SchemaValueCompletion());
            _ruleSet.Add(new GraphDataItem_ResolveFieldStatus());
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