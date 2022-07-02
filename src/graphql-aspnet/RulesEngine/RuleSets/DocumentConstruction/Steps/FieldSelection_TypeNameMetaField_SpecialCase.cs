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
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// The '__typename' field has a few special case needs around it concerning union graph types. process the __typename
    /// field, when requested, by itself in this rule.
    /// </summary>
    internal class FieldSelection_TypeNameMetaField_SpecialCase
        : DocumentConstructionStep<FieldNode>
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
                       .SequenceEqual(Constants.ReservedNames.TYPENAME_FIELD.AsSpan());
        }

        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (FieldNode)context.ActiveNode;

            // the '__typename' field is a known static quantity that requires no special rule processing or validation
            // it just exists or it doesn't and would be valid on any graph type returning it.
            //
            // however it can also be applied to unions so make sure its added as a special field type
            // so we can account for it later
            IGraphField field = null;
            IGraphType fieldGraphType = context.Schema.KnownTypes.FindGraphType(Constants.ScalarNames.STRING);
            if (context.ParentPart.GraphType is IGraphFieldContainer gfc)
                field = gfc.Fields.FindField(node.FieldName.ToString());

            var docPart = new DocumentFieldTypeName(context.ParentPart, node, field, fieldGraphType);
            context.AssignPart(docPart);
            return true;
        }
    }
}