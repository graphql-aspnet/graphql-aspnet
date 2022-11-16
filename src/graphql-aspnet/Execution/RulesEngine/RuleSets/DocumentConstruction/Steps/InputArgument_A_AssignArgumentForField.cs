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
    using GraphQL.AspNet.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentConstruction.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;

    /// <summary>
    /// Creates and assigns a <see cref="IInputArgumentDocumentPart"/> to the current node context for the active node.
    /// </summary>
    internal class InputArgument_A_AssignArgumentForField : DocumentConstructionStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputArgument_A_AssignArgumentForField"/> class.
        /// </summary>
        public InputArgument_A_AssignArgumentForField()
            : base(SyntaxNodeType.InputItem)
        {
        }

        /// <inheritdoc />
        public override bool ShouldExecute(DocumentConstructionContext context)
        {
            return base.ShouldExecute(context) && context.ParentPart is IFieldDocumentPart;
        }

        /// <inheritdoc />
        public override bool Execute(ref DocumentConstructionContext context)
        {
            var node = context.ActiveNode;
            var fdp = context.ParentPart as IFieldDocumentPart;

            var inputName = context.SourceText.Slice(node.PrimaryValue.TextBlock).ToString();

            var argument = fdp.Field?.Arguments.FindArgument(inputName);
            var inputItemGraphType = context.Schema.KnownTypes.FindGraphType(argument?.TypeExpression.TypeName);

            var docPart = new DocumentInputArgument(
                context.ParentPart,
                argument,
                inputName,
                node.Location);

            docPart.AssignGraphType(inputItemGraphType);

            context = context.AssignPart(docPart);

            return true;
        }
    }
}