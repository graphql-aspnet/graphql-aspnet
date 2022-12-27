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
    using GraphQL.AspNet.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentConstruction.Common;

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