// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine
{
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.VariableDataValidation;

    /// <summary>
    /// A validation context processor that executes to allow for custom rule processing of the actual resolved variable
    /// data (received on the request) against the query document and chosen operation just before its executed. This rule processor
    /// has no "built in" rules but serves as a hook point for user code where they may deem that such validation is necessary
    /// </summary>
    /// <remarks>
    /// One example where this processor is helpful is in use of the @oneOf directive, where supplied variable data
    /// must conform to the @oneOf rule set. This can only be determined after the document has been fully parsed and validated.
    /// </remarks>
    public class VariableDataValidationRuleProcessor : RuleProcessor<VariableDataValidationContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableDataValidationRuleProcessor" /> class.
        /// </summary>
        public VariableDataValidationRuleProcessor()
            : base(VariableDataValidationRulePackage.Instance)
        {
        }
    }
}