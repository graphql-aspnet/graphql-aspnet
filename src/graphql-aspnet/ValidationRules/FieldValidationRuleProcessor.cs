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
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.ValidationRules.RuleSets.FieldResolution;

    /// <summary>
    /// A child first rule processor that starts at the bottom of a set of executed fields
    /// and validates in a "bottom up" order that they are valid in contents, conform to non-nullability
    /// requirements for their target fields in the schema and as necessary rule in eliminating field values
    /// or invalidating hte the whole stack as necessary.
    /// </summary>
    internal sealed class FieldValidationRuleProcessor : RuleProcessor<FieldValidationContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldValidationRuleProcessor"/> class.
        /// </summary>
        public FieldValidationRuleProcessor()
            : base(FieldValidationRulePackage.Instance, true)
        {
        }
    }
}