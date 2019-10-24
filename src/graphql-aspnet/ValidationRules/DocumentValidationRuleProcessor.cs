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
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation;

    /// <summary>
    /// A rule processor that handles rules related to ensuring a completed query document
    /// (parsed from the text submitted by a user) is internally consistant and valid against its target schema.
    /// e.g. things such as "are variables used when delcared", "are type expressions matched against the schema" etc.
    /// </summary>
    internal class DocumentValidationRuleProcessor : RuleProcessor<DocumentValidationContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentValidationRuleProcessor"/> class.
        /// </summary>
        public DocumentValidationRuleProcessor()
            : base(DocumentValidationRulePackage.Instance)
        {
        }
    }
}