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
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentConstruction.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;

    /// <summary>
    /// Generates a <see cref="INamedFragmentDocumentPart"/> out of the active named fragment node and injects it as the active
    /// item on the node context as well as adding it to the general document context.
    /// </summary>
    internal class FragmentNamed_CreateFragmentOnDocument : DocumentConstructionStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FragmentNamed_CreateFragmentOnDocument"/> class.
        /// </summary>
        public FragmentNamed_CreateFragmentOnDocument()
            : base(SyntaxNodeType.NamedFragment)
        {
        }

        /// <inheritdoc />
        public override bool Execute(ref DocumentConstructionContext context)
        {
            var node = context.ActiveNode;

            var fragmentName = context.SourceText.Slice(node.PrimaryValue.TextBlock).ToString();
            var targetGraphTypeName = context.SourceText.Slice(node.SecondaryValue.TextBlock).ToString();

            var docPart = new DocumentNamedFragment(
                context.ParentPart,
                fragmentName,
                targetGraphTypeName,
                node.Location);

            var graphType = context.Schema.KnownTypes.FindGraphType(docPart.TargetGraphTypeName);
            docPart.AssignGraphType(graphType);

            context = context.AssignPart(docPart);
            return true;
        }
    }
}