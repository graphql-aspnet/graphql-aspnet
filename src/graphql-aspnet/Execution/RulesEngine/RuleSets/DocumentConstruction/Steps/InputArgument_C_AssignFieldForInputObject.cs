// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentConstruction.Steps
{
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentConstruction.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// Creats and assigns a <see cref="IInputArgumentDocumentPart"/> to the current node context for the active node.
    /// </summary>
    internal class InputArgument_C_AssignFieldForInputObject : DocumentConstructionStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputArgument_C_AssignFieldForInputObject"/> class.
        /// </summary>
        public InputArgument_C_AssignFieldForInputObject()
            : base(SyntaxNodeType.InputItem)
        {
        }

        /// <inheritdoc />
        public override bool ShouldExecute(DocumentConstructionContext context)
        {
            return base.ShouldExecute(context) && context.ParentPart is IComplexSuppliedValueDocumentPart;
        }

        /// <inheritdoc />
        public override bool Execute(ref DocumentConstructionContext context)
        {
            var node = context.ActiveNode;
            var csv = (IComplexSuppliedValueDocumentPart)context.ParentPart;

            IGraphType inputItemGraphType = null;
            IInputGraphField inputField = null;

            var inputName = context.SourceText.Slice(node.PrimaryValue.TextBlock).ToString();
            if (csv.GraphType is IInputObjectGraphType iio)
            {
                inputField = iio.Fields.FindField(inputName);
                inputItemGraphType = context.Schema.KnownTypes.FindGraphType(inputField?.TypeExpression?.TypeName);
            }

            var docPart = new DocumentInputObjectField(
                context.ParentPart,
                inputName,
                inputField,
                node.Location);

            docPart.AssignGraphType(inputItemGraphType);

            context = context.AssignPart(docPart);
            return true;
        }
    }
}