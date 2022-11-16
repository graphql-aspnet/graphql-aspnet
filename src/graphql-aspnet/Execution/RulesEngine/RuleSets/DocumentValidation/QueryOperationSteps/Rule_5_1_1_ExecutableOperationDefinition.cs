// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.QueryOperationSteps
{
    using System.Linq;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// <para>(5.1.1) Verify that all top level definitions in the document are either an operation definition
    /// or a fragment definition.</para>
    /// <para>Reference: https://graphql.github.io/graphql-spec/October2021/#sec-Executable-Definitions .</para>
    /// </summary>
    internal class Rule_5_1_1_ExecutableOperationDefinition
        : DocumentPartValidationRuleStep<IOperationDocumentPart>
    {
        /// <inheritdoc/>
        public override bool Execute(DocumentValidationContext context)
        {
            var docPart = context.ActivePart as IOperationDocumentPart;
            if (docPart.OperationType == GraphOperationType.Unknown)
            {
                var names = string.Join(", ", Constants.ReservedNames.GRAPH_OPERATION_NAMES.Select(x => $"'{x}'"));

                this.ValidationError(
                    context,
                    $"Invalid Executable Definition. Unexpected operation type '{docPart.OperationTypeName}' not " +
                    $"allowed. Only {names} are allowed.");

                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override string RuleNumber => "5.1.1";

        /// <inheritdoc/>
        protected override string RuleAnchorTag => "#sec-Executable-Definitions";
    }
}