// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.DocumentParts
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// A semi-parsed reference to a variable on an operation used during document validation.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Variable: {Name}")]
    internal class DocumentVariable : DocumentPartBase, IVariableDocumentPart, IDescendentDocumentPartSubscriber
    {
        private DocumentDirectiveCollection _directives = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentVariable" /> class.
        /// </summary>
        /// <param name="parentPart">The part, typically an operation, that owns this variable declaration.</param>
        /// <param name="variableName">Name of the variable as declared in the source document.</param>
        /// <param name="typeExpression">The type expression assigned to the variable.</param>
        /// <param name="location">The location in the source text where this
        /// variable originated.</param>
        public DocumentVariable(
            IDocumentPart parentPart,
            string variableName,
            GraphTypeExpression typeExpression,
            SourceLocation location)
            : base(parentPart, location)
        {
            this.Name = variableName;
            this.TypeExpression = typeExpression;
        }

        /// <inheritdoc cref="IDescendentDocumentPartSubscriber.OnDescendentPartAdded" />
        void IDescendentDocumentPartSubscriber.OnDescendentPartAdded(IDocumentPart decendentPart, int relativeDepth)
        {
            if (decendentPart.Parent == this && decendentPart is IDirectiveDocumentPart ddp)
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
            this.Children.OfType<ISuppliedValueDocumentPart>().SingleOrDefault();

        /// <inheritdoc />
        public IDirectiveCollectionDocumentPart Directives => _directives;

        /// <inheritdoc />
        public override string Description => $"Variable: {this.Name ?? "-unknown-"}";
    }
}