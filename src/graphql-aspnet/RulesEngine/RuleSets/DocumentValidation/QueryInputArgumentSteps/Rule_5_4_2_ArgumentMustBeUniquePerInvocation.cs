// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.InputItemNodeSteps
{
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// A rule that ensures that for any given input node, the name of the node is unique among the defined set of inputs
    /// on the target field in the user's query document.
    /// </summary>
    internal class Rule_5_4_2_ArgumentMustBeUniquePerInvocation
        : DocumentPartValidationRuleStep<IInputArgumentDocumentPart>
    {

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var docPart = (IInputArgumentDocumentPart)context.ActivePart;

            // short cut. if an arugment IS declared this rule would be
            // run twice duplicating the error message for the same argument name
            var key = $"5.4.2|{docPart.Parent.Path.DotString()}|argument:{docPart.Name}";
            if (context.GlobalKeys.ContainsKey(key))
                return true;

            context.GlobalKeys.Add(key, true);

            var parentType = "unknown type";
            var parentName = "~unknown~";
            var isDuplicate = false;
            if (context.ParentPart is IDirectiveDocumentPart ddp)
            {
                isDuplicate = !ddp.GatherArguments().IsUnique(docPart.Name);
                parentType = "directive";
                parentName = ddp.DirectiveName;
            }
            else if (context.ParentPart is IFieldDocumentPart fdp)
            {
                isDuplicate = !fdp.GatherArguments().IsUnique(docPart.Name);
                parentType = "field";
                parentName = fdp.Name.ToString();
            }

            if (isDuplicate)
            {
                this.ValidationError(
                    context,
                    $"The {parentType} '{parentName}' already contains an input argument named '{docPart.Name}'. Input arguments " +
                    $"must be unique per {parentType} invocation.");
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.4.2";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Argument-Uniqueness";
    }
}