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
    /// A step to pass through the <see cref="InputItemCollectionNode"/> effectively
    /// allowing the attachment of input items directly to their parent field, directive
    /// or other input argument.
    /// </summary>
    internal class InputItemCollection_Skip : DocumentConstructionStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputItemCollection_Skip"/> class.
        /// </summary>
        public InputItemCollection_Skip()
            : base(SynNodeType.InputItemCollection)
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