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
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// A step to pass through the <see cref="InputItemCollectionNode"/> effectively
    /// allowing the attachment of input items directly to their parent field, directive
    /// or other input argument.
    /// </summary>
    internal class InputItemCollection_Skip
        : DocumentConstructionStep<InputItemCollectionNode>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentConstructionContext context)
        {
            context.Skip();
            return true;
        }
    }
}