// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.DocumentLevelSteps
{
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.QueryPlans.Document;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;

    /// <summary>
    /// <para>(5.1.1) Verify that all top level definitions in the document are either an operation definition
    /// or a fragment definition.</para>
    /// <para>Reference: <see href="https://graphql.github.io/graphql-spec/October2021/#sec-Executable-Definitions" />.</para>
    /// </summary>
    internal class Rule_5_1_1_OnlyExecutableDefinition
        : DocumentPartValidationRuleStep<IGraphQueryDocument>
    {
        /// <inheritdoc/>
        public override bool Execute(DocumentValidationContext context)
        {
            // the parser can't fail this rule, but an injected child by a directive
            // could, so it must exist
            var docPart = context.ActivePart as IGraphQueryDocument;
            var invalidPartFound = false;
            foreach (var part in docPart.Children)
            {
                switch (part)
                {
                    case INamedFragmentDocumentPart _:
                    case IOperationDocumentPart _:
                        break;

                    default:
                        this.ValidationError(
                           context,
                           $"Unexpected Definition. Invalid Top level definition of '{docPart.PartType}'. " +
                           $"Expected one of: {DocumentPartType.NamedFragment}, {DocumentPartType.Operation}");
                        invalidPartFound = true;
                        break;
                }
            }

            return !invalidPartFound;
        }

        /// <inheritdoc/>
        public override string RuleNumber => "5.1.1";

        /// <inheritdoc/>
        protected override string RuleAnchorTag => "#sec-Executable-Definitions";
    }
}