// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.RulesEngine.RuleSets.DocumentConstruction.Steps
{
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentConstruction.Common;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// Creates and assigns a <see cref="IInputArgumentDocumentPart"/> to the current node context for the active node.
    /// </summary>
    internal class InputArgument_A_AssignArgumentForField
        : DocumentConstructionStep<InputItemNode>
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentConstructionContext context)
        {
            return base.ShouldExecute(context) && context.ParentPart is IFieldDocumentPart;
        }

        /// <inheritdoc />
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (InputItemNode)context.ActiveNode;
            var fdp = context.ParentPart as IFieldDocumentPart;

            var argument = fdp.Field?.Arguments.FindArgument(node.InputName.ToString());
            var inputItemGraphType = context.Schema.KnownTypes.FindGraphType(argument?.TypeExpression.TypeName);

            var docPart = new DocumentInputArgument(
                context.ParentPart,
                node,
                argument);

            docPart.AssignGraphType(inputItemGraphType);

            context.AssignPart(docPart);

            return true;
        }
    }
}