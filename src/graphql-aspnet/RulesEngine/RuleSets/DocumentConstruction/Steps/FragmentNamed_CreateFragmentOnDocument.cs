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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Generates a <see cref="INamedFragmentDocumentPart"/> out of the active named fragment node and injects it as the active
    /// item on the node context as well as adding it to the general document context.
    /// </summary>
    internal class FragmentNamed_CreateFragmentOnDocument
        : DocumentConstructionStep<NamedFragmentNode>
    {

        /// <inheritdoc />
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (NamedFragmentNode)context.ActiveNode;

            var docPart = new DocumentNamedFragment(context.ParentPart, node);
            var graphType = context.Schema.KnownTypes.FindGraphType(docPart.TargetGraphTypeName);
            docPart.AssignGraphType(graphType);

            context.AssignPart(docPart);
            return true;
        }
    }
}