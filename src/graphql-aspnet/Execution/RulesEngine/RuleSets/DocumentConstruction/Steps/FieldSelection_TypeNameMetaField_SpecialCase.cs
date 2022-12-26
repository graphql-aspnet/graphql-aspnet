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
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentConstruction.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// The '__typename' field has a few special case needs around it concerning union graph types. process the __typename
    /// field, when requested, by itself in this rule.
    /// </summary>
    internal class FieldSelection_TypeNameMetaField_SpecialCase : DocumentConstructionStep
    {
        private static readonly string TYPENAME_STRING = Constants.ReservedNames.TYPENAME_FIELD.AsSpan().ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldSelection_TypeNameMetaField_SpecialCase"/> class.
        /// </summary>
        public FieldSelection_TypeNameMetaField_SpecialCase()
            : base(SyntaxNodeType.Field)
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
            if (!base.ShouldExecute(context))
                return false;

            var fieldName = context.SourceText.Slice(context.ActiveNode.PrimaryValue.TextBlock);
            return fieldName
                   .SequenceEqual(Constants.ReservedNames.TYPENAME_FIELD.AsSpan());
        }

        /// <inheritdoc />
        public override bool Execute(ref DocumentConstructionContext context)
        {
            var node = context.ActiveNode;

            // the '__typename' field is a known static quantity that requires no special rule processing or validation
            // it just exists or it doesn't and would be valid on any graph type returning it.
            // However, it can also be applied to unions so make sure its added as a field
            IGraphType fieldGraphType = context
                .Schema
                .KnownTypes
                .FindGraphType(Constants.ScalarNames.STRING);

            var alias = context.SourceText.Slice(context.ActiveNode.SecondaryValue.TextBlock).ToString();

            if (context.ParentPart.GraphType is IGraphFieldContainer gfc)
            {
                var field = gfc.Fields.FindField(TYPENAME_STRING);

                var docPart = new DocumentFieldTypeName(
                    context.ParentPart,
                    field,
                    fieldGraphType,
                    node.Location,
                    alias);

                context = context.AssignPart(docPart);
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
                        var field = foundGraphType.Fields.FindField(TYPENAME_STRING);
                        if (field != null)
                        {
                            var docPart = new DocumentFieldTypeName(
                                 context.ParentPart,
                                 field,
                                 fieldGraphType,
                                 node.Location,
                                 alias);

                            parts.Add(docPart);
                        }
                    }
                }

                // for all potential fields to add, directly add all but the last one to
                // the parent. Allow the last one to be added via the completing the context
                for (var i = 0; i < parts.Count; i++)
                {
                    if (i + 1 < parts.Count)
                    {
                        context.ParentPart.Children.Add(parts[i]);
                    }
                    else
                    {
                        context = context.AssignPart(parts[i]);
                    }
                }
            }

            return true;
        }
    }
}