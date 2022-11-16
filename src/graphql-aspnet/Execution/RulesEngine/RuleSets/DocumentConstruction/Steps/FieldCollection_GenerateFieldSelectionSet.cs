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
    using GraphQL.AspNet.Parsing2.DocumentGeneration.DocumentConstruction.Common;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;

    /// <summary>
    /// For top level operations fragments and fields that have a set of requested child fields,
    /// generates this step generates a <see cref="IFieldSelectionSetDocumentPart"/> and sets
    /// it as active on the current context.
    /// </summary>
    internal class FieldCollection_GenerateFieldSelectionSet : DocumentConstructionStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldCollection_GenerateFieldSelectionSet"/> class.
        /// </summary>
        public FieldCollection_GenerateFieldSelectionSet()
            : base(SyntaxNodeType.FieldCollection)
        {
        }

        /// <inheritdoc />
        public override bool Execute(ref DocumentConstructionContext context)
        {
            // skip the field collection node
            // carry the parent part (a field or an operation) to the next set of children
            // (the child fields).
            var docPart = new DocumentFieldSelectionSet(
                context.ParentPart,
                context.ActiveNode.Location);

            context = context.AssignPart(docPart);
            return true;
        }
    }
}