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
    using GraphQL.AspNet.Execution.QueryPlans.Document;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;

    /// <summary>
    /// <para>(5.2.2.1) Validate that when an anon operation is included it exists by itself.</para>
    /// <para>Reference: https://graphql.github.io/graphql-spec/October2021/#sec-Operation-Name-Uniqueness" .</para>
    /// </summary>
    internal class Rule_5_2_2_1_LoneAnonymousOperation
        : DocumentPartValidationRuleStep<IGraphQueryDocument>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            // anonymous operations will all present as ReadOnlyMemory<char>.Empty
            var document = (IGraphQueryDocument)context.ActivePart;

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

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z").
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.2.2.1";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Lone-Anonymous-Operation";
    }
}