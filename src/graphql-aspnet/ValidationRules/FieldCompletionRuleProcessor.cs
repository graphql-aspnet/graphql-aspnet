// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules
{
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.ValidationRules.RuleSets.FieldResolution;

    /// <summary>
    /// A rule processor that handles a set of rules relating to the immediate completion of a field
    /// resolution from user code. This rule set is executed directly following a resolver assigning a value
    /// to a <see cref="GraphDataItem"/> during a query plan execution. The rules of this processor must pass
    /// for the field to allow child context execution.
    /// </summary>
    internal sealed class FieldCompletionRuleProcessor : RuleProcessor<FieldValidationContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldCompletionRuleProcessor"/> class.
        /// </summary>
        public FieldCompletionRuleProcessor()
            : base(FieldCompletionRulePackage.Instance)
        {
        }
    }
}