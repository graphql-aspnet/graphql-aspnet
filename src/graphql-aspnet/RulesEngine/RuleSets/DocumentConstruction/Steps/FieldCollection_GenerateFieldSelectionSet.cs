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
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// For <see cref="OperationNode"/> and <see cref="FieldNode"/> that have a set of requested child fields, generates
    /// a new <see cref="IFieldSelectionSetDocumentPart"/> and sets it as active on the current context.
    /// </summary>
    internal class FieldCollection_GenerateFieldSelectionSet
        : DocumentConstructionStep<FieldCollectionNode>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentConstructionContext context)
        {
            // skip the field collection node
            // carry the parent part (a field or an operation) to the next set of children
            // (the child fields).
            var docPart = new DocumentFieldSelectionSet(context.ParentPart);
            context.AssignPart(docPart);
            return true;
        }
    }
}