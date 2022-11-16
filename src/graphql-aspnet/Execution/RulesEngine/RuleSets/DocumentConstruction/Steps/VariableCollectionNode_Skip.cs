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

    /// <summary>
    /// A step to pass through the <see cref="VariableCollectionNode"/> effectively
    /// allowing the attachment of variables directly to their assigned operation.
    /// </summary>
    internal class VariableCollectionNode_Skip : DocumentConstructionStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableCollectionNode_Skip"/> class.
        /// </summary>
        public VariableCollectionNode_Skip()
            : base(SyntaxNodeType.VariableCollection)
        {
        }

        /// <inheritdoc />
        public override bool Execute(ref DocumentConstructionContext context)
        {
            context = context.Skip();
            return true;
        }
    }
}