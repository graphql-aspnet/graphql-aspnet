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
    using System.Linq;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;

    /// <summary>
    /// <para>(5.2.3.1) Validate that when an anon operation is included it exists by itself.</para>
    /// <para>Reference: <see href="https://graphql.github.io/graphql-spec/September2025/#sec-Lone-Anonymous-Operation" /> .</para>
    /// </summary>
    internal class Rule_5_2_3_1_LoneAnonymousOperation
        : DocumentPartValidationRuleStep<IQueryDocument>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            // anonymous operations will all present as ReadOnlyMemory<char>.Empty
            var document = (IQueryDocument)context.ActivePart;

            var allFoundOperations = document.Children[DocumentPartType.Operation]
                .OfType<IOperationDocumentPart>()
                .ToList();

            var anonymousOperations = allFoundOperations
                .Where(x => string.IsNullOrWhiteSpace(x.Name))
                .ToList();

            if (anonymousOperations.Count >= 1 && allFoundOperations.Count > 1)
            {
                var location = anonymousOperations.Count > 1 ? anonymousOperations[1].SourceLocation : anonymousOperations[0].SourceLocation;
                this.ValidationError(
                    context,
                    location,
                    "A query document may declare an anonymous operation only if it exists by itself in a document. This document " +
                    $"contains {document.Operations.Count} total operation(s). Remove the other operations or " +
                    "provide a name for every operation.");

                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.2.3.1";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Lone-Anonymous-Operation";
    }
}