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
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Set the currently pointed at inline fragmnet to be the query fragment on the current context for further processing.
    /// </summary>
    internal class FragmentInline_CreateInlineFragmentOnDocument : DocumentConstructionStep<InlineFragmentNode>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentConstructionContext context)
        {
            var fragmentNode = (InlineFragmentNode)context.ActiveNode;

            var docPart = new DocumentInlineFragment(context.ParentPart, fragmentNode);
            if (string.IsNullOrWhiteSpace(docPart.TargetGraphTypeName))
            {
                docPart.AssignGraphType(context.ParentPart.GraphType);
            }
            else
            {
                // attempt to find a target type when one is given for the fragment
                var graphType = context.Schema.KnownTypes.FindGraphType(docPart.TargetGraphTypeName);
                docPart.AssignGraphType(graphType);
            }

            context.AssignPart(docPart);
            return true;
        }
    }
}