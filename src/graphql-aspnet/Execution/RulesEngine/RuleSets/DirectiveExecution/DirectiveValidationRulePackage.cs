// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DirectiveExecution
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine.Interfaces;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DirectiveExecution.DirectiveValidation;

    /// <summary>
    /// A rule package for validating a request to execute a directive against
    /// a target.
    /// </summary>
    internal class DirectiveValidationRulePackage : IRulePackage<GraphDirectiveExecutionContext>
    {
        /// <summary>
        /// Gets the singleton instance of this rule package.
        /// </summary>
        /// <value>The instance.</value>
        public static DirectiveValidationRulePackage Instance { get; } = new DirectiveValidationRulePackage();

        private readonly IDictionary<object, IList<IRuleStep<GraphDirectiveExecutionContext>>> _stepCollection;
        private readonly IList<IRuleStep<GraphDirectiveExecutionContext>> _ruleSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveValidationRulePackage" /> class.
        /// </summary>
        private DirectiveValidationRulePackage()
        {
            _stepCollection = new Dictionary<object, IList<IRuleStep<GraphDirectiveExecutionContext>>>();
            _ruleSet = new List<IRuleStep<GraphDirectiveExecutionContext>>();

            _ruleSet.Add(new DirectiveContext_ContentsExist());
            _ruleSet.Add(new Rule_5_7_1_DirectivesMustExist());
            _ruleSet.Add(new Rule_5_7_2_DirectiveValidForLocation());
            _ruleSet.Add(new Rule_5_7_ArgumentsMustbeValid());
        }

        /// <inheritdoc />
        public IEnumerable<IRuleStep<GraphDirectiveExecutionContext>> FetchRules(GraphDirectiveExecutionContext context)
        {
            return _ruleSet;
        }
    }
}