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
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
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
            // However, it can also be applied to unions so make sure its added as a field
            IGraphType fieldGraphType = context.Schema.KnownTypes.FindGraphType(Constants.ScalarNames.STRING);

            if (context.ParentPart.GraphType is IGraphFieldContainer gfc)
            {
                var field = gfc.Fields.FindField(node.FieldName.ToString());
                var docPart = new DocumentFieldTypeName(context.ParentPart, node, field, fieldGraphType);
                context.AssignPart(docPart);
            }
            else if (context.ParentPart.GraphType is IUnionGraphType ugt)
            {
                var parts = new List<IDocumentPart>();

                // we dont know how many of the possible types actually exist in
                // the schema so queue them
                foreach (var name in ugt.PossibleGraphTypeNames)
                {
                    var foundGraphType = context.Schema.KnownTypes.FindGraphType(name) as IGraphFieldContainer;
                    if (foundGraphType != null)
                    {
                        var field = foundGraphType.Fields.FindField(node.FieldName.ToString());
                        if (field != null)
                        {
                            var docPart = new DocumentFieldTypeName(context.ParentPart, node, field, fieldGraphType);
                            parts.Add(docPart);
                        }
                    }
                }

                // for all potential fields to add, directly add all but the last one to
                // the parent. Allow the last one to be added via the completing the context
                for (var i = 0; i < parts.Count; i++)
                {
                    if (i + 1 < parts.Count)
                        context.ParentPart.Children.Add(parts[i]);
                    else
                        context.AssignPart(parts[i]);
                }
            }

            return true;
        }
    }
}