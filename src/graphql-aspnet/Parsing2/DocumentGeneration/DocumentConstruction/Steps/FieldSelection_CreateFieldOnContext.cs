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
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing2.DocumentGeneration.DocumentConstruction.Common;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;

    /// <summary>
    /// Creates a new <see cref="IFieldDocumentPart"/> on the active <see cref="IFieldSelectionSetDocumentPart"/> and sets it as the
    /// active <see cref="IFieldDocumentPart"/> on the current context.
    /// </summary>
    internal class FieldSelection_CreateFieldOnContext : DocumentConstructionStep
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldSelection_CreateFieldOnContext"/> class.
        /// </summary>
        public FieldSelection_CreateFieldOnContext()
            : base(SynNodeType.Field)
        {
        }

        /// <inheritdoc />
        public override bool ShouldExecute(DocumentConstructionContext context)
        {
            // this step does not handle `__typename` field requests
            if (!base.ShouldExecute(context))
                return false;

            var fieldName = context.SourceText.Slice(context.ActiveNode.PrimaryValue.TextBlock);
            return fieldName
                   .SequenceNotEqual(Constants.ReservedNames.TYPENAME_FIELD.AsSpan());
        }

        /// <inheritdoc />
        public override bool Execute(ref DocumentConstructionContext context)
        {
            var node = context.ActiveNode;
            var sourceGraphType = context.ParentPart.GraphType as IGraphFieldContainer;

            var fieldName = context.SourceText.Slice(node.PrimaryValue.TextBlock).ToString();
            var aliasName = context.SourceText.Slice(node.SecondaryValue.TextBlock).ToString();

            var graphField = sourceGraphType?.Fields.FindField(fieldName);
            var fieldReturnGraphType = context.Schema.KnownTypes.FindGraphType(graphField);

            var docPart = new DocumentField(
                context.ParentPart,
                graphField,
                fieldReturnGraphType,
                node.Location,
                aliasName);

            context = context.AssignPart(docPart);
            return true;
        }
    }
}