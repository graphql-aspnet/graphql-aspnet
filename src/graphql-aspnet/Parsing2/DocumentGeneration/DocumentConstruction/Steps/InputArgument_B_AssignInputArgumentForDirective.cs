// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing2.DocumentGeneration.DocumentConstruction
{
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing2.DocumentGeneration.DocumentConstruction.Common;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;

    /// <summary>
    /// Creates and assigns a <see cref="IInputArgumentDocumentPart"/> to the current node context for the active node.
    /// </summary>
    internal class InputArgument_B_AssignInputArgumentForDirective : DocumentConstructionStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputArgument_B_AssignInputArgumentForDirective"/> class.
        /// </summary>
        public InputArgument_B_AssignInputArgumentForDirective()
            : base(SynNodeType.InputItem)

        {
        }

        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the node if it cannot
        /// process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified node; otherwise, <c>false</c>.</returns>
        public override bool ShouldExecute(DocumentConstructionContext context)
        {
            return base.ShouldExecute(context) && context.ParentPart is IDirectiveDocumentPart;
        }

        /// <summary>
        /// Executes the construction step the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(ref DocumentConstructionContext context)
        {
            var node = context.ActiveNode;
            var directive = context.ParentPart.GraphType as IDirective;

            var inputName = context.SourceText.Slice(node.PrimaryValue.TextBlock).ToString();

            var argument = directive?.Arguments.FindArgument(inputName);
            var argumentGraphType = context.Schema.KnownTypes.FindGraphType(argument?.TypeExpression?.TypeName);

            var docpart = new DocumentInputArgument(
                context.ParentPart,
                argument,
                inputName,
                node.Location);

            docpart.AssignGraphType(argumentGraphType);

            context = context.AssignPart(docpart);
            return true;
        }
    }
}