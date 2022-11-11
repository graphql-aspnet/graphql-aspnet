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
    /// Generates a <see cref="INamedFragmentDocumentPart"/> out of the active named fragment node and injects it as the active
    /// item on the node context as well as adding it to the general document context.
    /// </summary>
    internal class FragmentNamed_CreateFragmentOnDocument : DocumentConstructionStep
    {
        public FragmentNamed_CreateFragmentOnDocument()
            : base(SynNodeType.NamedFragment)
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