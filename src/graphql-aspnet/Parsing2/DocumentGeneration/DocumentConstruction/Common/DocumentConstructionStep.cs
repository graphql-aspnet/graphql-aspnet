// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing2.DocumentGeneration.DocumentConstruction.Common
{
    using System.Collections.Generic;

    /// <summary>
    /// A base construction step containing common logic for most active steps (not validators).
    /// </summary>
    internal abstract class DocumentConstructionStep : BaseDocumentConstructionRuleStep
    {
        private SynNodeType _acceptableNodeType;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentConstructionStep" /> class.
        /// </summary>
        /// <param name="acceptableNodeType">The active node on a given
        /// context must be of this type for the context to be processed by this rule.</param>
        protected DocumentConstructionStep(SynNodeType acceptableNodeType)
        {
            _acceptableNodeType = acceptableNodeType;
        }

        /// <inheritdoc />
        public override bool ShouldExecute(DocumentConstructionContext context)
        {
            return _acceptableNodeType == context.ActiveNode.NodeType;
        }
    }
}