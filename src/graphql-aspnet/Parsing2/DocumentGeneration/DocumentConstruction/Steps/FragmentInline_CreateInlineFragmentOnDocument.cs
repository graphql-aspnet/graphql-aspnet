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
    using GraphQL.AspNet.Parsing2.DocumentGeneration.DocumentConstruction.Common;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;

    /// <summary>
    /// Set the currently pointed at inline fragmnet to be the query fragment on the current context for further processing.
    /// </summary>
    internal class FragmentInline_CreateInlineFragmentOnDocument : DocumentConstructionStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FragmentInline_CreateInlineFragmentOnDocument"/> class.
        /// </summary>
        public FragmentInline_CreateInlineFragmentOnDocument()
            : base(SynNodeType.InlineFragment)
        {
        }

        /// <inheritdoc />
        public override bool Execute(ref DocumentConstructionContext context)
        {
            var fragmentNode = context.ActiveNode;

            var restrictToType = context
                .SourceText
                .Slice(fragmentNode.PrimaryValue.TextBlock)
                .ToString();

            var docPart = new DocumentInlineFragment(
                context.ParentPart,
                restrictToType,
                fragmentNode.Location);

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

            context = context.AssignPart(docPart);
            return true;
        }
    }
}