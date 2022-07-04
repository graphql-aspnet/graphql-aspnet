// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.QueryInputArgumentSteps
{
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// A rule that ensures that for any given input node, the name of the node exists as a valid named argument
    /// on the target field.
    /// </summary>
    internal class Rule_5_4_1_ArgumentMustBeDefinedOnTheField
        : DocumentPartValidationRuleStep<IInputArgumentDocumentPart>
    {

        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            // rule 5.4.1 does not cover "arguments" as fields on complex values
            return base.ShouldExecute(context)
                && (context.ParentPart is IDirectiveDocumentPart
                   || context.ParentPart is IFieldDocumentPart);
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var docPart = (IInputArgumentDocumentPart)context.ActivePart;

            var argExists = false;
            var ownerType = "unknown type";
            var ownerName = "~unknown~";
            if (context.ParentPart is IDirectiveDocumentPart ddp && ddp.GraphType is IDirective directive)
            {
                ownerType = "directive";
                ownerName = ddp.DirectiveName;
                argExists = directive.Arguments.FindArgument(docPart.Name) != null;
            }
            else if (context.ParentPart is IFieldDocumentPart fdp && fdp.Field != null)
            {
                ownerType = "field";
                ownerName = fdp.Name.ToString();
                argExists = fdp.Field.Arguments.FindArgument(docPart.Name) != null;
            }

            if (!argExists)
            {
                this.ValidationError(
                    context,
                    $"The {ownerType} '{ownerName}' does not define an input argument named '{docPart.Name}'. Input arguments " +
                    "must be defined on their ownering container in the target schema.");
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.4.1";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Argument-Names";
    }
}