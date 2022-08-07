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
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Creates a new <see cref="IFieldDocumentPart"/> on the active <see cref="IFieldSelectionSetDocumentPart"/> and sets it as the
    /// active <see cref="IFieldDocumentPart"/> on the current context.
    /// </summary>
    internal class FieldSelection_CreateFieldOnContext
        : DocumentConstructionStep<FieldNode>
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentConstructionContext context)
        {
            // this step does not handle `__typename` field requests
            return base.ShouldExecute(context) && ((FieldNode)context.ActiveNode)
                   .FieldName
                   .Span
                   .SequenceNotEqual(Constants.ReservedNames.TYPENAME_FIELD.AsSpan());
        }

        /// <inheritdoc />
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (FieldNode)context.ActiveNode;
            var sourceGraphType = context.ParentPart.GraphType as IGraphFieldContainer;

            var graphField = sourceGraphType?.Fields.FindField(node.FieldName.ToString());
            var fieldReturnGraphType = context.Schema.KnownTypes.FindGraphType(graphField);

            var docPart = new DocumentField(
                context.ParentPart,
                node,
                graphField,
                fieldReturnGraphType);

            context.AssignPart(docPart);
            return true;
        }
    }
}