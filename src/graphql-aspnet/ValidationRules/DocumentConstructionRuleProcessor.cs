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
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction;

    /// <summary>
    /// A rule processor that handles rules related to constructing a query document from source text
    /// submitted by a user. I.E. these rules collectively build a usable query document for a target schema
    /// using the document syntax provided by the end user.
    /// </summary>
    internal sealed class DocumentConstructionRuleProcessor : RuleProcessor<DocumentConstructionContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentConstructionRuleProcessor"/> class.
        /// </summary>
        public DocumentConstructionRuleProcessor()
            : base(DocumentConstructionRulePackage.Instance)
        {
        }
    }
}