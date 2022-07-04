// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts.Common
{
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A base class with common functionality of all <see cref="IDocumentPart" />
    /// implementations.
    /// </summary>
    /// <typeparam name="TSyntaxNode">The syntax node from which this document part was created.</typeparam>
    internal abstract class DocumentPartBase<TSyntaxNode> : DocumentPartBase
       where TSyntaxNode : SyntaxNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentPartBase{TSyntaxNode}" /> class.
        /// </summary>
        /// <param name="parentPart">The parent document part, if any, that owns this instance.</param>
        /// <param name="node">The AST node from which this part was created.</param>
        protected DocumentPartBase(IDocumentPart parentPart, TSyntaxNode node)
            : base(parentPart, node)
        {
        }

        /// <summary>
        /// Gets the syntax node from which this instance was created.
        /// </summary>
        /// <value>The node.</value>
        protected new TSyntaxNode Node => base.Node as TSyntaxNode;
    }
}