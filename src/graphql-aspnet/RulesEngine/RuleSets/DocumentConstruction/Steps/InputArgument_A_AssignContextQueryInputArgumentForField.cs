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
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Creates and assigns a <see cref="IInputArgumentDocumentPart"/> to the current node context for the active node.
    /// </summary>
    internal class InputArgument_A_AssignContextQueryInputArgumentForField
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
            IGraphType inputItemGraphType = null;
            GraphTypeExpression expectedTypeExpression = null;

            var argument = fdp.Field?.Arguments.FindArgument(node.InputName.ToString());
            inputItemGraphType = context.Schema.KnownTypes.FindGraphType(argument?.TypeExpression.TypeName);
            expectedTypeExpression = argument?.TypeExpression;

            var docPart = new DocumentInputArgument(
                context.ParentPart,
                node,
                expectedTypeExpression);

            docPart.AssignGraphType(inputItemGraphType);

            context.AssignPart(docPart);

            return true;
        }
    }
}