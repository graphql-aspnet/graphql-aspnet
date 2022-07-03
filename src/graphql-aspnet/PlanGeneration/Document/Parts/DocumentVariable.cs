// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.Common;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// A semi-parsed reference to a variable on an operation used during document validation.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Variable: {Name}")]
    internal class DocumentVariable : DocumentPartBase<VariableNode>, IVariableDocumentPart
    {
        private DocumentDirectiveCollection _directives = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentVariable" /> class.
        /// </summary>
        /// <param name="parentPart">The part, typically an operation, that owns this variable declaration.</param>
        /// <param name="node">The AST node from which this instance is created.</param>
        public DocumentVariable(IDocumentPart parentPart, VariableNode node)
            : base(parentPart, node)
        {
            this.Name = node.Name.ToString();
            this.TypeExpression = GraphTypeExpression.FromDeclaration(node.TypeExpression.Span);
        }

        protected override void OnChildPartAdded(IDocumentPart childPart, int relativeDepth)
        {
            base.OnChildPartAdded(childPart, relativeDepth);
            if (relativeDepth == 1 && childPart is IDirectiveDocumentPart ddp)
            {
                _directives = _directives ?? new DocumentDirectiveCollection(this);
                _directives.AddDirective(ddp);
            }
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public GraphTypeExpression TypeExpression { get; private set; }

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.Variable;

        /// <inheritdoc />
        public ISuppliedValueDocumentPart DefaultValue =>
            this.Children.OfType<ISuppliedValueDocumentPart>().FirstOrDefault();

        public IDirectiveCollectionDocumentPart Directives => _directives;
    }
}