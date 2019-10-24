// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.FieldNodeSteps
{
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Creates a new <see cref="FieldSelection"/> on the active <see cref="FieldSelectionSet"/> and sets it as the
    /// active <see cref="FieldSelection"/> on the current context.
    /// </summary>
    internal class FieldSelection_CreateFieldOnContext : DocumentConstructionStep<FieldNode>
    {
        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the node if it cannot
        /// process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified node; otherwise, <c>false</c>.</returns>
        public override bool ShouldExecute(DocumentConstructionContext context)
        {
            return base.ShouldExecute(context) && ((FieldNode)context.ActiveNode)
                   .FieldName
                   .Span
                   .SequenceNotEqual(Constants.ReservedNames.TYPENAME_FIELD.AsSpan());
        }

        /// <summary>
        /// Executes the construction step the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (FieldNode)context.ActiveNode;
            var searchContainer = context.GraphType as IGraphFieldContainer;

            if (searchContainer == null)
                return false;

            var field = searchContainer.Fields.FindField(node.FieldName.ToString());
            var graphType = context.DocumentContext.Schema.KnownTypes.FindGraphType(field);
            if (graphType == null)
            {
                throw new InvalidOperationException($"A field named '{node.FieldName.ToString()}' was expected but was not valid in context.");
            }

            var fieldSelection = new FieldSelection(node, field, graphType);
            context.AddDocumentPart(fieldSelection);
            return true;
        }
    }
}