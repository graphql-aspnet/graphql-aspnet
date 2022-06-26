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
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.SuppliedValues;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Creats and assigns a <see cref="IInputArgumentDocumentPart"/> to the current node context for the active node.
    /// </summary>
    internal class InputArgument_C_AssignContextQueryInputArgumentForInputObject
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
            GraphTypeExpression expectedTypeExpression = null;

            if (csv.GraphType is IInputObjectGraphType iio)
            {
                var inputField = iio.Fields.FindField(node.InputName.ToString());
                expectedTypeExpression = inputField?.TypeExpression;
                inputItemGraphType = context.Schema.KnownTypes.FindGraphType(inputField.TypeExpression.TypeName);
            }

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