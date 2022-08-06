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

    /// <summary>
    /// Creats and assigns a <see cref="IInputArgumentDocumentPart"/> to the current node context for the active node.
    /// </summary>
    internal class InputArgument_C_AssignFieldForInputObject
        : DocumentConstructionStep<InputItemNode>
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentConstructionContext context)
        {
            return base.ShouldExecute(context) && context.ParentPart is IComplexSuppliedValueDocumentPart;
        }

        /// <inheritdoc />
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (InputItemNode)context.ActiveNode;
            var csv = (IComplexSuppliedValueDocumentPart)context.ParentPart;

            IGraphType inputItemGraphType = null;
            IGraphField inputField = null;

            if (csv.GraphType is IInputObjectGraphType iio)
            {
                inputField = iio.Fields.FindField(node.InputName.ToString());
                inputItemGraphType = context.Schema.KnownTypes.FindGraphType(inputField?.TypeExpression?.TypeName);
            }

            var docPart = new DocumentInputObjectField(
                context.ParentPart,
                node,
                inputField);

            docPart.AssignGraphType(inputItemGraphType);

            context.AssignPart(docPart);
            return true;
        }
    }
}