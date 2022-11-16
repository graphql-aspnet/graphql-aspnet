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
    /// Assigns the active query fragment on the current context to be the fragment pointed to by the spread.
    /// </summary>
    internal class FragmentSpread_RegisterNamedFragmentToContext : DocumentConstructionStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FragmentSpread_RegisterNamedFragmentToContext"/> class.
        /// </summary>
        public FragmentSpread_RegisterNamedFragmentToContext()
            : base(SyntaxNodeType.FragmentSpread)
        {
        }

        /// <inheritdoc />
        public override bool Execute(ref DocumentConstructionContext context)
        {
            var node = context.ActiveNode;

            var pointsToFragmentName = context.SourceText.Slice(node.PrimaryValue.TextBlock).ToString();

            var docPart = new DocumentFragmentSpread(
                context.ParentPart,
                pointsToFragmentName,
                node.Location);

            // cant set graphtype yet, that is done during linking after all named
            // fragments are parsed
            context = context.AssignPart(docPart);

            return true;
        }
    }
}